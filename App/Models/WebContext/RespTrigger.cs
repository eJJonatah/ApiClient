using static ApiClient.App.Controllers.WebConnector;
using System;

namespace ApiClient.App.Models
{
    public static partial class WebContext
    {
        public struct OperationTrigger
        {
            public TriggerStyle Type;
            public string Message;
            public Action Solve;
            public OperationTrigger(TriggerStyle type, string message)
            {
                Type = type;
                Message = message;
                Solve = null;
            }
            public OperationTrigger(Http.Request request, bool atRuntime = false)
            {
                switch (true)
                {
                    case true when request.Response.Content.Contains(request.Triggers.Error):
                        Type = TriggerStyle.Error;
                        Message = request.Response.Message.ReasonPhrase;
                        Solve = null;
                        return;
                    case true when request.Response.Content.Contains(request.Triggers.Terminated):
                        Type = TriggerStyle.Stop;
                        Message = $"Request response invoked termination trigger: {request.Triggers.Terminated}";
                        Solve = null;
                        return;
                    case true when request.Response.Content.Contains(request.Triggers.VoidAnswer):
                        Type = TriggerStyle.Stop;
                        Message = "Response contains no value data";
                        Solve = null;
                        return;
                    case true when request.Response.Content.Contains(request.Triggers.Chaining) && atRuntime:
                        Type = TriggerStyle.Continue;
                        Message = $"Response has valid Chaining trigger {request.Triggers.Chaining}";
                        Solve = null;
                        return;
                    case true when string.IsNullOrEmpty(request.Response.Content):
                        Type = TriggerStyle.Error;
                        Message = "Request returned no response";
                        Solve = null;
                        return;
                    default:
                        Type    = TriggerStyle.NaN;
                        Message = "ResponseTrigger couldn't be determined";
                        Solve   = null;
                        return;
                }
            }
            public void Handle() => Solve.Invoke();
            public enum TriggerStyle
            {
                Stop,
                Start,
                Continue,
                Error,
                NaN
            }

        }

    }
}
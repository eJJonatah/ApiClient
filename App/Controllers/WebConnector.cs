using static ApiClient.App.Models.WebContext.SavedRequests;
using static ApiClient.App.Models.WebContext;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiClient.App.Models;
using System.Threading;
using System.Net.Http;
using System;
using System.CodeDom;

namespace ApiClient.App.Controllers
{
    public static class WebConnector
    {
        public class RequestOperation
        {
            public Http.Request RecognitionRequest;
            public bool RecognitionSucess;
            public Dictionary<string, string> RecognitionTransponder;
            public CancellationTokenSource RequestSending { get; set; }
            public HttpClient Connector { get; set; }
            public List<Http.Request> Requests_list { get; set; }
            public Http.Response[] Result { get => responses_list.ToArray(); }
            private List<Http.Response> responses_list;
            public void AddResponse(Http.Response response) => responses_list.Add(response);
            public RequestOperation(Http.Request recognitionRequest, TimeSpan timeOuter)
            {
                RecognitionRequest = recognitionRequest;
                RequestSending = new CancellationTokenSource();
                responses_list = new List<Http.Response>();
                Requests_list = new List<Http.Request>();
                Connector = new HttpClient();
                foreach(var header in recognitionRequest.Message.Headers)
                {
                    Connector.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                Connector.Timeout = timeOuter;
            }
            //Methods
            //Methods
        }
        public class Http
        {
            public struct Response
            {
                public int Result;
                public string Content;
                public HttpResponseMessage Message;
                public string Details;
            }
            public class Request
            {
                public int Index;
                public HttpRequestMessage Message { get; set; }
                public bool IsSucess { get; set; }
                public CancellationToken RequestSend { get; set; }
                public Request(RequestModule model)
                {
                    Message = new HttpRequestMessage()
                    {
                        Content = model.Body,
                        Method = model.Method,
                    };
                    if (model.BuildableUrl) 
                    {
                        model.BuildUri();
                        Message.RequestUri = model.queryAddress;
                    }
                    else
                    {
                        Message.RequestUri = new Uri(model.Url.ToString());
                    }
                    if(model.Headers != null)
                    foreach(var header in model.Headers)
                    {
                        Message.Headers.Add(header.Key, header.Value);
                    }
                    RequestSend = new CancellationToken();

                }
                public Request(RequestModule model, RequestOperation operation, int index)
                {
                    Index = index;
                    Message = new HttpRequestMessage()
                    {
                        Content = model.Body,
                        Method = model.Method,
                    };
                    if (model.BuildableUrl)
                    {
                        model.BuildUri();
                        model.Paginate(Index);
                        Message.RequestUri = model.queryAddress;
                    }
                    else
                    {
                        Message.RequestUri = new Uri(model.Url.ToString());
                    }
                    if (model.Headers != null)
                        foreach (var header in model.Headers)
                        {
                            Message.Headers.Add(header.Key, header.Value);
                        }
                    RequestSend = operation.RequestSending.Token;
                }
                public Response Response { get; set; }
                public RequestData.RetunType ResponseType { get; set; }
                public RequestData.RequestTriggers Triggers { get; set; }
                public async Task Send()
                {
                    if (Message == null) return;
                    using (var httpRequest = await new HttpClient().SendAsync(Message))
                    {
                        this.GetherResponse(httpRequest);
                    }
                }
                public async Task<int> Send( RequestOperation Operation )
                {
                    if (Message == null) return 1;
                    using (var httpRequest = await Operation.Connector.SendAsync(Message))
                    {
                        try
                        {
                            if (httpRequest.IsSuccessStatusCode)
                            {
                                IsSucess = true;
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        finally 
                        {
                            this.GetherResponse(httpRequest);
                        }
                    }
                }
                public async void GetherResponse(HttpResponseMessage response)
                {
                    Response = new Response()
                    {
                        Content = await response.Content.ReadAsStringAsync(),
                        Result = Convert.ToInt16(response.IsSuccessStatusCode),
                        Details = response.ReasonPhrase,
                        Message = response
                    };
                }
            }
        }
    }
}

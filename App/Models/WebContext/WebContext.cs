using static ApiClient.App.Models.WebContext.SavedRequests;
using System.Collections.Generic;
using System.Security.Policy;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Text;

namespace ApiClient.App.Models
{
    public static partial class WebContext
    {
        public class WebInstructions
        {
            public RequestData RequestInfo { get; set; }
            public string SelectedGuid { get; set; }
            public List<RequestModule> RequestModules_list { get; set; }
            public RequestModule CreateModule(string endPointName) 
            {
                var selectedEndpoint = RequestInfo.EndPoints[endPointName];
                var instructions = RequestInfo.Instructions[selectedEndpoint.EndPID];                
                string bodyContent = instructions.Body.Content;
                
                HttpContent targetBody = null;
                string targetUrl;
                switch (instructions.Body.Type) 
                {
                    case RequestData.BodyInfo.BodyTypes.Text:
                        targetBody = new StringContent(bodyContent);
                        break;
                    case RequestData.BodyInfo.BodyTypes.Json:
                        targetBody = new StringContent(JsonConvert.SerializeObject(bodyContent, Formatting.Indented));
                        break;
                    case RequestData.BodyInfo.BodyTypes.Form:
                        var formDictionary = new Dictionary<string, string>();
                        var formData = bodyContent.Split(RequestInfo.Parametry.Separator);
                        foreach (var form in formData)
                        {
                            var formValues = form.Split('=');
                            formDictionary.Add(formValues[0], formValues[1]);
                        }
                        targetBody = new FormUrlEncodedContent(formDictionary);
                        break;
                }
                targetUrl = RequestInfo.Url;
                if (!string.IsNullOrEmpty(selectedEndpoint.CustomURL))
                {
                    targetUrl = selectedEndpoint.CustomURL;
                }
                return new RequestModule()
                {
                    RequestGuid = Guid.Parse(SelectedGuid),
                    EndPointName = endPointName,
                    EndPoint = selectedEndpoint.Path,
                    TempAuth = instructions.Authorization.Temp,
                    Token = instructions.Authorization.Token,
                    Chained = instructions.Chained,
                    Parametry = RequestInfo.Parametry,
                    Url = new Url(targetUrl),
                    Method = new HttpMethod(instructions.Method),
                    Body = targetBody,
                    Parameters = instructions.Parameters.UrlParms.Select(p => new { p.Name, p.Value }).ToDictionary(p => p.Name, p => p.Value),
                    TimeOuter = new TimeSpan(0, 0, RequestInfo.TimeOut),
                    BuildableUrl = instructions.BuildableUrl,
                    Headers = instructions.Headers,
                    ResponseEncoding = instructions.ResponseType,
                    Triggers = instructions.Triggers
                };
            }
            public WebInstructions(string guid, SavedRequests savedRequests)
            {
                RequestModules_list = new List<RequestModule>();
                RequestInfo = savedRequests.Data[guid];
                SelectedGuid = guid;
            }
        }
        public struct RequestModule 
        {
            public Uri queryAddress;
            public Guid RequestGuid;
            public string EndPointName;
            public string EndPoint;
            public bool TempAuth;
            public string Token;
            public bool Chained;
            public Dictionary<string, string> Headers;
            public RequestData.ParametryInfo Parametry;
            public Url Url;
            public HttpMethod Method;
            public HttpContent Body;
            public RequestData.BodyInfo.BodyTypes ContentType;
            public RequestData.RequestTriggers Triggers;
            public Dictionary<string, string> Parameters;
            public TimeSpan TimeOuter;
            public bool BuildableUrl;
            public RequestData.RetunType ResponseEncoding;
            public void BuildUri()
            {
                var ConstructedUri = new StringBuilder(Url.ToString());
                if (Parametry.Starter != '\0')
                {
                    ConstructedUri.Append(Parametry.Starter);
                    if (Parameters != null)
                    {
                        foreach (var param in Parameters)
                        {
                            ConstructedUri.Append(param.Key + "=" + param.Value + Parametry.Separator.ToString());
                        }
                    }
                }
                queryAddress = new Uri(ConstructedUri.ToString());
            }
            public void Paginate(int index)
            {
                if (!BuildableUrl) return;
                if (string.IsNullOrEmpty(Parametry.Paginator)) return;
                var paginatedAddress = queryAddress.ToString() + Parametry.Paginator + "=" + index;
                queryAddress = new Uri(paginatedAddress);
            }

        }
        public class SavedRequests 
        {
            public Dictionary<string, RequestData> Data { get; set; }
            //Derivatives
            public class RequestData
            {
                public string Url { get; set; }
                public Dictionary<string,EndPoint> EndPoints { get; set; }
                public  Dictionary<string, RequestInstructions> Instructions { get; set; }
                public ParametryInfo Parametry { get; set; }
                public int TimeOut { get; set; }
                public class RequestInstructions
                {
                    public string Method { get; set; }
                    public BodyInfo Body { get; set; }
                    public AuthorizationInfo Authorization { get; set; }
                    public Dictionary<string,string> Headers { get; set; }
                    public bool Chained { get; set; }
                    public bool BuildableUrl { get; set; }
                    public ParametersInfo Parameters { get; set; }
                    public RetunType ResponseType { get; set; }
                    public RequestTriggers Triggers;
                }
                public struct BodyInfo
                {
                    public bool Parsable { get; set; }
                    public string[] ReparseTargets { get; set; }
                    public BodyTypes Type { get; set; }
                    public string Content { get; set; }
                    public enum BodyTypes
                    {
                        Form,
                        Json,
                        Text
                    }
                    public void Parse(Dictionary<string, string> parseParams)
                    {
                        if (!Parsable) return;
                        foreach(var item in parseParams)
                        {
                            Content.Replace(item.Key, item.Value);
                        }
                    }
                }
                public struct AuthorizationInfo
                {
                    public bool Temp { get; set; }
                    public string Token { get; set; }
                    public string LoginEndp { get; set; }
                }
                public class ParametersInfo
                {
                    public ParseParamsInfo ParseParams { get; set; }
                    public Param[] UrlParms { get; set; }
                    public Param[] OtherParms { get; set; }

                }
                public class ParseParamsInfo
                {
                    public Dictionary<string,string> Body { get; set; }
                    public Dictionary<string, string> Url { get; set; }
                    public Dictionary<string, string> Other { get; set; }
                }
                public struct Param
                {
                    public bool Parsable;
                    public string Name;
                    public string Value;
                    public void Parse(Dictionary<string,string> parseParams) 
                        => parseParams.TryGetValue(Name, out Value);
                }
                public struct ParametryInfo
                {
                    public char Starter;
                    public char Separator;
                    public string Paginator;
                }
                public struct EndPoint
                {
                    public string CustomURL { get; set; }
                    public string EndPID { get; set; }
                    public string Path { get; set; }
                }
                public enum RetunType 
                {
                    Json,
                    Html,
                    Text
                }
                public class RequestTriggers
                {
                    public string Error { get; set; }
                    public string Terminated { get; set; }
                    public string VoidAnswer { get; set; }
                    public string Chaining { get; set; }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RequestHandlers.Http
{
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Patch
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpRequestAttribute : Attribute
    {
        public class Result
        {
            public string Route { get; set; }
            public List<string> RouteVariable { get; set; }
            public List<string> QueryStringVariables { get; set; }
        }
        public string ServiceName { get; set; }

        public string Route { get; set; }
        public HttpMethod HttpMethod { get; set; }

        public HttpRequestAttribute(string route, HttpMethod httpMethod, string serviceName = null)
        {
            Route = route;
            HttpMethod = httpMethod;
            ServiceName = serviceName;
        }


        private static readonly Regex QuerystringRegex = new Regex(@"\?((?<test>[a-zA-Z0-9_]*)*)([\&]{0,1})(?<secondary>[a-zA-Z0-9&]*)");
        private static readonly Regex RouteVariableRegex = new Regex(@"\{(?<test>[a-zA-Z_0-9]{1,})\}");
        public Result Parse()
        {
            var result = new Result()
            {
                Route = "",
                RouteVariable = new List<string>(),
                QueryStringVariables = new List<string>()
            };

            var attributeRoute = Route;

            result.Route = QuerystringRegex.Replace(attributeRoute, "");

            AddRouteVariables(attributeRoute, result);

            AddQueryStringVariables(attributeRoute, result);

            return result;
        }

        private static void AddQueryStringVariables(string attributeRoute, Result result)
        {
            var test = QuerystringRegex.Match(attributeRoute);
            if (test.Success)
            {
                var first = test.Groups["test"];
                if (first.Success)
                {
                    foreach (Capture firstCapture in first.Captures)
                    {
                        if (!string.IsNullOrEmpty(firstCapture.Value)) result.QueryStringVariables.Add(firstCapture.Value);
                    }
                    var secondary = test.Groups["secondary"];
                    if (secondary.Success)
                    {
                        foreach (var secondaryArg in secondary.Value.Split('&'))
                        {
                            if (!string.IsNullOrEmpty(secondaryArg)) result.QueryStringVariables.Add(secondaryArg);
                        }
                    }
                }
            }
        }

        private static void AddRouteVariables(string attributeRoute, Result result)
        {
            var matches = RouteVariableRegex.Matches(attributeRoute);
            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Groups["test"].Captures)
                {
                    result.RouteVariable.Add(capture.Value);
                }
            }
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class GetRequestAttribute : HttpRequestAttribute
    {
        public GetRequestAttribute(string route, string serviceName = null) : base(route, HttpMethod.Get, serviceName)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchRequestAttribute : HttpRequestAttribute
    {
        public PatchRequestAttribute(string route, string serviceName = null) : base(route, HttpMethod.Patch, serviceName)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class PostRequestAttribute : HttpRequestAttribute
    {
        public PostRequestAttribute(string route, string serviceName = null) : base(route, HttpMethod.Post, serviceName)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class PutRequestAttribute : HttpRequestAttribute
    {
        public PutRequestAttribute(string route, string serviceName = null) : base(route, HttpMethod.Put, serviceName)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteRequestAttribute : HttpRequestAttribute
    {
        public DeleteRequestAttribute(string route, string serviceName = null) : base(route, HttpMethod.Delete, serviceName)
        {
        }
    }
}

using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpRequestAttribute : Attribute
    {
        public string Route { get; set; }
        public HttpMethod HttpMethod { get; set; }

        public HttpRequestAttribute(string route, HttpMethod httpMethod)
        {
            Route = route;
            HttpMethod = httpMethod;
        }
    }
}

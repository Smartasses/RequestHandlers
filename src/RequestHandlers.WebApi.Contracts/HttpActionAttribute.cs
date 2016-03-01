using System;

namespace RequestHandlers.WebApi.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpActionAttribute : Attribute
    {
        public HttpActionAttribute(string route, Method httpMethod)
        {
            Route = route;
            HttpMethod = httpMethod;
        }

        public string Route { get; }
        public Method HttpMethod { get; }
    }
}
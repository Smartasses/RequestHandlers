using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GetRequestAttribute : HttpRequestAttribute
    {
        public GetRequestAttribute(string route) : base(route, HttpMethod.Get)
        {
        }
    }
}
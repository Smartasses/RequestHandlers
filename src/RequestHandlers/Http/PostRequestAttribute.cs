using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PostRequestAttribute : HttpRequestAttribute
    {
        public PostRequestAttribute(string route) : base(route, HttpMethod.Post)
        {
        }
    }
}
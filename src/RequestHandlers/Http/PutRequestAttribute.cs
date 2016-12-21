using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PutRequestAttribute : HttpRequestAttribute
    {
        public PutRequestAttribute(string route) : base(route, HttpMethod.Put)
        {
        }
    }
}
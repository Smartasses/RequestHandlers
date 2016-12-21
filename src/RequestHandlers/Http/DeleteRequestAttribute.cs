using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteRequestAttribute : HttpRequestAttribute
    {
        public DeleteRequestAttribute(string route) : base(route, HttpMethod.Delete)
        {
        }
    }
}
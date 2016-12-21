using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchRequestAttribute : HttpRequestAttribute
    {
        public PatchRequestAttribute(string route) : base(route, HttpMethod.Patch)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestHandlers
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
    public class GetRequestAttribute : HttpRequestAttribute { public GetRequestAttribute(string route) : base(route, HttpMethod.Get) { } }
    [AttributeUsage(AttributeTargets.Class)]
    public class PostRequestAttribute : HttpRequestAttribute { public PostRequestAttribute(string route) : base(route, HttpMethod.Post) { } }
    [AttributeUsage(AttributeTargets.Class)]
    public class PutRequestAttribute : HttpRequestAttribute { public PutRequestAttribute(string route) : base(route, HttpMethod.Put) { } }
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteRequestAttribute : HttpRequestAttribute { public DeleteRequestAttribute(string route) : base(route, HttpMethod.Delete) { } }
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchRequestAttribute : HttpRequestAttribute { public PatchRequestAttribute(string route) : base(route, HttpMethod.Patch) { } }
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

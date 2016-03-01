using System;

namespace RequestHandlers.Http.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpActionAttribute : Attribute
    {
        public HttpActionAttribute(string url, Method method)
        {
            Url = url;
            Method = method;
        }

        public string Url { get; }
        public Method Method { get; }
    }
}
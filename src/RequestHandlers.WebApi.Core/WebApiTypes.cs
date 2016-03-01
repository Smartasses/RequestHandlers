using System;

namespace RequestHandlers.WebApi.Core
{
    public class WebApiTypes
    {
        public Type ApiController { get; set; }
        public Type RouteAttribute { get; set; }
        public Type HttpGetAttribute { get; set; }
        public Type HttpPostAttribute { get; set; }
        public Type HttpDeleteAttribute { get; set; }
        public Type HttpPutAttribute { get; set; }
        public Type ResponseTypeAttribute { get; set; }
    }
}
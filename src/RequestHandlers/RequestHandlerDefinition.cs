using System;

namespace RequestHandlers
{

    public class RequestHandlerDefinition
    {
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
        public Type RequestHandlerType { get; set; }
    }
}

using System;

namespace RequestHandlers
{
    public interface IRequestDefinition
    {
        Type RequestType { get; set; }
        Type ResponseType { get; set; }
    }
    public class RequestHandlerDefinition : IRequestDefinition
    {
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
        public Type RequestHandlerType { get; set; }
    }
}

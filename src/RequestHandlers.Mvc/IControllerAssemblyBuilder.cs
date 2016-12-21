using System.Reflection;

namespace RequestHandlers.Mvc
{
    public interface IControllerAssemblyBuilder
    {
        Assembly Build(HttpRequestHandlerControllerDefinition[] definitions);
    }
}
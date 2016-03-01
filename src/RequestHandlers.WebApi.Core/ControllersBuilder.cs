using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RequestHandlers.WebApi.Core
{
    public class ControllersBuilder
    {
        public Assembly CreateControllers(Type webApiRequestProcessor, RequestHandlerDefinition[] requestHandlers, WebApiTypes webApiTypes)
        {
            var assemblyName = new AssemblyName("Generated");
            var moduleName = $"{assemblyName.Name}.dll";

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assembly.DefineDynamicModule(moduleName, assemblyName.Name + ".dll", true);
            var controllerBuilder = new ControllerBuilder(moduleBuilder, webApiRequestProcessor, webApiTypes);
            foreach (var requestHandlerDefinition in requestHandlers)
            {
                controllerBuilder.CreateController(requestHandlerDefinition);
            }
            return moduleBuilder.Assembly;
        }
    }
}

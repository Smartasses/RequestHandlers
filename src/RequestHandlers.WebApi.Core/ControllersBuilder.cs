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
            var moduleName = string.Format("{0}.dll", assemblyName.Name);

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, "c:\\");
            var moduleBuilder = assembly.DefineDynamicModule(moduleName, assemblyName.Name + ".dll", true);
            var controllerBuilder = new ControllerBuilder(moduleBuilder, webApiRequestProcessor, webApiTypes);
            foreach (var requestHandlerDefinition in requestHandlers)
            {
                controllerBuilder.CreateController(requestHandlerDefinition);
            }
            assembly.Save(moduleName);
            return moduleBuilder.Assembly;
        }
    }
}

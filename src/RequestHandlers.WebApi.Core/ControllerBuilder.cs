using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RequestHandlers.Http.Contracts;

namespace RequestHandlers.WebApi.Core
{
    class ControllerBuilder
    {
        private readonly ModuleBuilder _moduleBuilder;
        private readonly Type _webApiRequestProcessor;
        private readonly WebApiTypes _webApiTypes;

        public ControllerBuilder(ModuleBuilder moduleBuilder, Type webApiRequestProcessor, WebApiTypes webApiTypes)
        {
            _moduleBuilder = moduleBuilder;
            _webApiRequestProcessor = webApiRequestProcessor;
            _webApiTypes = webApiTypes;
            _proxyRequestBuilder = new RequestProxyBuilder();
        }
        static MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        private readonly RequestProxyBuilder _proxyRequestBuilder;

        public Type CreateController(RequestHandlerDefinition requestHandler)
        {
            var actionInfo = requestHandler.RequestType.GetCustomAttributes<HttpActionAttribute>(true).FirstOrDefault();
            if (actionInfo != null)
            {
                var controllerName = "GeneratedControllers." + GetControllerName(requestHandler);
                var type = CreateType(controllerName);
                var requestProcessorField = type.DefineField("_requestProcessor", _webApiRequestProcessor, FieldAttributes.Private);
                CreateConstructor(type, requestProcessorField);
                var someMethod = requestProcessorField.FieldType.GetMethod("Process").MakeGenericMethod(requestHandler.RequestType, requestHandler.ResponseType);

                bool canHaveBody = actionInfo.Method == Method.Post || actionInfo.Method == Method.Put;

                var routeVariablesPropertyInfo = requestHandler.RequestType.GetProperties().Where(x => x.CanWrite && x.GetCustomAttributes<FromRouteAttribute>().Any()).ToArray();
                var routeVariables = routeVariablesPropertyInfo
                    .Select(x => new
                    {
                        x.Name,
                        x.PropertyType,
                        x.SetMethod
                    }).ToArray();
                Type requestType = null;
                var actionParameters = routeVariables.Select(x => x.PropertyType);
                if (canHaveBody)
                {
                    if (routeVariables.Any())
                    {
                        var propertiesToDuplicate = requestHandler.RequestType.GetProperties().Where(x => x.CanWrite && x.CanRead && !routeVariablesPropertyInfo.Contains(x)).ToArray();
                        var name = "ProxyRequest." + requestHandler.RequestType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "");
                        requestType = _proxyRequestBuilder.BuildRequestProxy(name, propertiesToDuplicate, _moduleBuilder);
                    }
                    else
                    {
                        requestType = requestHandler.RequestType;
                    }
                }
                if (canHaveBody)
                {
                    actionParameters = actionParameters.Concat(new[] { requestType });
                }

                var methodBuilder = type.DefineMethod(requestHandler.RequestType.Name, MethodAttributes.Public | MethodAttributes.HideBySig, someMethod.ReturnType, actionParameters.ToArray());
                int index = 0;
                for (; index < routeVariables.Length; index++)
                {
                    var routeVariable = routeVariables[index];
                    methodBuilder.DefineParameter(index + 1, ParameterAttributes.None, routeVariable.Name);
                }
                methodBuilder.DefineParameter(index + 1, ParameterAttributes.None, "request");



                var httpMethodAttribute = 
                    actionInfo.Method == Method.Post ? _webApiTypes.HttpPostAttribute :
                    actionInfo.Method == Method.Delete ? _webApiTypes.HttpDeleteAttribute :
                    actionInfo.Method == Method.Get ? _webApiTypes.HttpGetAttribute :
                    actionInfo.Method == Method.Put ? _webApiTypes.HttpPutAttribute : 
                    null;


                methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(httpMethodAttribute.GetConstructor(Type.EmptyTypes), new object[0]));
                methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(_webApiTypes.ResponseTypeAttribute.GetConstructor(new[] {typeof(Type)}), new object[] { requestHandler.ResponseType}));

                var route = new CustomAttributeBuilder(_webApiTypes.RouteAttribute.GetConstructor(new[] { typeof(string) }), new object[] { actionInfo.Url });
                methodBuilder.SetCustomAttribute(route);
                
                var il = methodBuilder.GetILGenerator();

                var local = il.DeclareLocal(requestHandler.RequestType);

                il.Emit(OpCodes.Newobj, requestHandler.RequestType.GetConstructor(Type.EmptyTypes));
                index = 0;
                for (; index < routeVariables.Length; index++)
                {
                    var routeVariable = routeVariables[index];
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldarg, index + 1);
                    il.Emit(OpCodes.Callvirt, routeVariable.SetMethod);
                    il.Emit(OpCodes.Nop);
                }
                if (canHaveBody)
                {
                    foreach (var source in requestType.GetProperties().Where(x => x.CanWrite && x.CanRead))
                    {
                        var targetProprety = requestHandler.RequestType.GetProperty(source.Name);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldarg, index + 1);
                        il.Emit(OpCodes.Callvirt, source.GetMethod);
                        il.Emit(OpCodes.Callvirt, targetProprety.SetMethod);
                        il.Emit(OpCodes.Nop);
                    }
                }

                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, requestProcessorField);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, someMethod);
                il.Emit(OpCodes.Ret);
                return type.CreateType();
            }
            else
            {
                return null;
            }
        }

        private TypeBuilder CreateType(string controllerName)
        {
            var type = _moduleBuilder.DefineType(controllerName, TypeAttributes.Public | TypeAttributes.BeforeFieldInit);
            type.SetParent(_webApiTypes.ApiController);
            return type;
        }

        private void CreateConstructor(TypeBuilder type, FieldBuilder requestProcessorField)
        {
            var myConstructorBuilder = type.DefineConstructor(attrs, CallingConventions.HasThis | CallingConventions.Standard,
                new[] {_webApiRequestProcessor});
            myConstructorBuilder.DefineParameter(1, ParameterAttributes.None, "processor");
            var ctorIL = myConstructorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call,
                _webApiTypes.ApiController.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .First(x => x.GetParameters().Length == 0));
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, requestProcessorField);
            ctorIL.Emit(OpCodes.Ret);
        }

        private string GetControllerName(RequestHandlerDefinition requestHandler)
        {
            return requestHandler.RequestType.Name + "HandlerController";
        }
    }
}
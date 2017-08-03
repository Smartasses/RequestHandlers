using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers
{
    public static class RequestHandlerFinder
    {
        public static RequestHandlerDefinition[] InAssembly(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => x.GetLoadableTypes())
                .Select(x => new
                {
                    Type = x,
                    TypeInfo = x.GetTypeInfo()
                })
                .Where(x => x.TypeInfo.IsClass && !x.TypeInfo.IsAbstract && !x.TypeInfo.IsInterface && !x.TypeInfo.IsGenericTypeDefinition)
                .SelectMany(x => GetRequestHandlerInterfaces(x.Type),
                    (type, definition) =>
                        new RequestHandlerDefinition
                        {
                            RequestHandlerType = type.Type,
                            RequestType = definition.Item1,
                            ResponseType = definition.Item2
                        }).ToArray();
        }

        private static IEnumerable<Tuple<Type, Type>> GetRequestHandlerInterfaces(Type type)
        {
            var requestHandlers = type.GetTypeInfo().GetInterfaces().Where(x => !x.GetTypeInfo().IsGenericTypeDefinition && x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            foreach (var requestHandler in requestHandlers)
            {
                var typeArguments = requestHandler.GetTypeInfo().GetGenericArguments();
                yield return Tuple.Create(typeArguments[0], typeArguments[1]);
            }
        }
        
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
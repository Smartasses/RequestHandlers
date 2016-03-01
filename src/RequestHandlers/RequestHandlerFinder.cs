using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers
{
    public static class RequestHandlerFinder
    {
        public static RequestHandlerDefinition[] InAssembly(Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition)
                .SelectMany(GetRequestHandlerInterfaces,
                    (type, definition) =>
                        new RequestHandlerDefinition
                        {
                            RequestHandlerType = type,
                            RequestType = definition.Item1,
                            ResponseType = definition.Item2
                        }).ToArray();
        }

        private static IEnumerable<Tuple<Type, Type>> GetRequestHandlerInterfaces(Type type)
        {
            var requestHandlers = type.GetInterfaces().Where(x => !x.IsGenericTypeDefinition && x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            foreach (var requestHandler in requestHandlers)
            {
                var typeArguments = requestHandler.GetGenericArguments();
                yield return Tuple.Create(typeArguments[0], typeArguments[1]);
            }
        }
    }
}
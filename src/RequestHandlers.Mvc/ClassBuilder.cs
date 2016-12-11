using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers.TestHost.RequestHandlers
{
    class ClassBuilder
    {
        public IEnumerable<string> GetValue(string className, string route, IRequestDefinition requestDefinition, List<string> args, bool hasBody)
        {
            var allProperties = requestDefinition.RequestType.GetProperties(BindingFlags.Public | BindingFlags.Instance |
                                                                            BindingFlags.SetProperty | BindingFlags.GetProperty);
            var argumentsToCopy = args.Select(x => new
                {
                    argName = x,
                    propertyInfo = allProperties.SingleOrDefault(p => p.Name.Equals(x, StringComparison.OrdinalIgnoreCase))
                }).Where(x => x.propertyInfo != null)
                .Select(x => new
                {
                    x.argName,
                    x.propertyInfo.Name,
                    TypeName = x.propertyInfo.PropertyType.FullName
                }).ToList();

            yield return "namespace Proxy";
            yield return "{";
            var assignments = argumentsToCopy.Select(x => new
            {
                From = x.argName,
                To = x.Name
            }).ToList();
            var actionArguments = argumentsToCopy.Select(x => new {x.argName, x.TypeName});
            if (allProperties.Length != argumentsToCopy.Count && hasBody)
            {
                var requestClass = requestDefinition.RequestType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "");
                yield return $"public {requestClass}";
                yield return "{";
                foreach (var source in allProperties.Where(x => argumentsToCopy.All(z => z.Name != x.Name)))
                {
                    yield return $"public {source.PropertyType.FullName} {source.Name} {{ get; set; }}";
                    assignments.Add(new
                    {
                        From = $"request.{source.Name}",
                        To = source.Name
                    });
                }
                yield return "}";
                actionArguments = actionArguments.Concat(new[] {new {argName = "request", TypeName = requestClass}});
            }

            var methodArgs = string.Join(",  ", actionArguments.Select(x => $"{x.TypeName} {x.argName}"));
            yield return $"    public class {className} : Microsoft.AspNetCore.Mvc.Controller";
            yield return "    {";
            yield return "        private readonly RequestHandlers.IRequestDispatcher _requestDispatcher;";
            yield return "";
            yield return $"        public {className}(RequestHandlers.IRequestDispatcher requestDispatcher)";
            yield return "        {";
            yield return "            _requestDispatcher = requestDispatcher;";
            yield return "        }";
            yield return $"        [Microsoft.AspNetCore.Mvc.HttpGetAttribute(\"{route}\")]";
            yield return $"        public {requestDefinition.ResponseType.FullName} Handle({methodArgs})";
            yield return "        {";
            yield return $"            var request = new {requestDefinition.RequestType.FullName}";
            yield return "            {";
            foreach (var assignment in assignments)
            {
                yield return $"{assignment.To} = {assignment.From},";
            }
            yield return "            };";
            yield return $"            var response = _requestDispatcher.Process<{requestDefinition.RequestType.FullName},{requestDefinition.ResponseType.FullName}>(request);";
            yield return "            return response;";
            yield return "        }";
            yield return "    }";
            yield return "}";
        }
    }
}
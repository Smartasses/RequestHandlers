using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RequestHandlers.TestHost.RequestHandlers
{
    static class DynamicBuilder
    {
        public static HashSet<string> classNames = new HashSet<string>();
        public static Assembly Build(IRequestDefinition[] inAssembly, string assemblyName = "ProxyControllers", string namespaceName = "ProxyControllers")
        {

            var neededAssemblies = new Dictionary<string, Assembly>();
            AddAssembly(neededAssemblies, typeof(Controller).GetTypeInfo().Assembly);
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
                , MetadataReference.CreateFromFile(typeof(DynamicBuilder).GetTypeInfo().Assembly.Location)
                , MetadataReference.CreateFromFile(typeof(IRequestHandler<,>).GetTypeInfo().Assembly.Location)
            }.Concat(neededAssemblies.Keys.Select(x => MetadataReference.CreateFromFile(x)));
            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references);

            foreach (var temp in inAssembly.SelectMany(x => x.RequestType.GetTypeInfo().GetCustomAttributes(true).OfType<HttpRequestAttribute>().Select(d => new { Definition = x, Attribute = d})))
            {
                var requestDefinition = temp.Definition;
                var attribute = temp.Attribute;

                var parsed = ParseUrlVariables(attribute.Route).ToArray();

                var className = GetClassName(requestDefinition.RequestType);
                var route = parsed[0];

                var args = parsed.Skip(1).ToList();

                var hasBody = attribute.HttpMethod == HttpMethod.Patch 
                    || attribute.HttpMethod == HttpMethod.Post 
                    || attribute.HttpMethod == HttpMethod.Put;
                var sb = new StringBuilder();
                foreach(var line in new ClassBuilder().GetValue(className, route, requestDefinition, args, hasBody))
                    sb.AppendLine(line);
                compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sb.ToString()));
            }

            var assemblyStream = new MemoryStream();
            var result = compilation.Emit(assemblyStream);
            if (!result.Success)
            {
                var errormsg = new StringBuilder();
                foreach (var diagnostic in result.Diagnostics)
                {
                    errormsg.AppendLine(diagnostic.ToString());
                }
                throw new Exception(errormsg.ToString());
            }
            assemblyStream.Seek(0, SeekOrigin.Begin);
            return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
        }

        private static IEnumerable<string> ParseUrlVariables(string attributeRoute)
        {
            var queryStringMatcher = new Regex(@"\?((?<test>[a-zA-Z0-9_]*)*)([\&]{0,1})(?<secondary>[a-zA-Z0-9&]*)");
            yield return queryStringMatcher.Replace(attributeRoute, "");

            var urlMatcher = new Regex(@"\{(?<test>[a-zA-Z_0-9]{1,})\}");
            var matches = urlMatcher.Matches(attributeRoute);
            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Groups["test"].Captures)
                {
                    yield return capture.Value;
                }
            }

            var test = queryStringMatcher.Match(attributeRoute);
            if (test.Success)
            {
                var first = test.Groups["test"];
                if (first.Success)
                {
                    foreach (Capture firstCapture in first.Captures)
                    {
                        if (!string.IsNullOrEmpty(firstCapture.Value)) yield return firstCapture.Value;
                    }
                    var secondary = test.Groups["secondary"];
                    if (secondary.Success)
                    {
                        foreach (var secondaryArg in secondary.Value.Split('&'))
                        {
                            if (!string.IsNullOrEmpty(secondaryArg)) yield return secondaryArg;
                        }
                    }
                }
            }
        }

        private static string GetClassName(Type requestType)
        {
            var name = requestType.Name;
            string className;
            int? addition = null;
            do
            {
                var add = addition.HasValue ? addition.ToString() : "";
                className = $"{name}Handler{add}Controller";
                addition = addition + 1 ?? 2;
            } while (classNames.Contains(className));
            return className;
        }

        private static void AddAssembly(Dictionary<string, Assembly> neededAssemblies, Assembly assembly)
        {
            if (neededAssemblies.ContainsKey(assembly.Location)) return;
            neededAssemblies.Add(assembly.Location, assembly);
            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                var refAssembly = Assembly.Load(referencedAssembly);
                AddAssembly(neededAssemblies, refAssembly);
            }
        }
    }

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
                    TypeName = x.propertyInfo.PropertyType.Name
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
    [GetRequest("api/test/{param1}?test&test2")]
    public class TestRequest : IReturn<TestResponse>
    {
        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }

    public class TestResponse
    {

        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public TestResponse Handle(TestRequest request)
        {
            return new TestResponse
            {
                Param1 = request.Param1,
                Test2 = request.Test2,
                Test = request.Test
            };
        }
    }
}

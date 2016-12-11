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
    class ReferencesHelper
    {
        public ReferencesHelper()
        {
            
        }
    }
    public static class DynamicBuilder
    {
        public static HashSet<string> classNames = new HashSet<string>();
        public static Assembly Build(IRequestDefinition[] inAssembly)
        {
            var neededAssemblies = new Dictionary<string, Assembly>();
            AddAssembly(neededAssemblies, typeof(object).GetTypeInfo().Assembly);
            AddAssembly(neededAssemblies, typeof(Controller).GetTypeInfo().Assembly);
            AddAssembly(neededAssemblies, typeof(DynamicBuilder).GetTypeInfo().Assembly);
            inAssembly.SelectMany(x => new[] {x.RequestType, x.ResponseType})
                .Select(x => x.GetTypeInfo().Assembly)
                .Distinct()
                .ToList().ForEach(x => AddAssembly(neededAssemblies, x));
            var references = neededAssemblies.Keys.Select(x => MetadataReference.CreateFromFile(x));

            var compilation = CSharpCompilation.Create("Proxy")
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
                compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sb.ToString()));
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
}

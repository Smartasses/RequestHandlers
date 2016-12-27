using System.Linq;
using System.Reflection;

namespace RequestHandlers.Http
{
    public class HttpRequestHandlerDefinition
    {
        public HttpRequestHandlerDefinition(HttpRequestAttribute attribute, IRequestDefinition definition)
        {
            var parsedRoute = attribute.Parse();

            var isFormRequest = definition.RequestType.GetTypeInfo().GetCustomAttribute<Http.FromFormAttribute>() != null;

            var canHaveBody = attribute.HttpMethod == HttpMethod.Patch
                              || attribute.HttpMethod == HttpMethod.Post
                              || attribute.HttpMethod == HttpMethod.Put;

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty;

            var defaultBinder = // when unable to detect binding, this will be the default
                canHaveBody
                    ? isFormRequest
                        ? BindingType.FromForm
                        : BindingType.FromBody
                    : BindingType.FromQuery;

            var binderHelper = new HttpRequestPropertyBinderHelper(defaultBinder, parsedRoute);
            var allProperties = definition.RequestType.GetProperties(bindingFlags);
            foreach (var propertyInfo in allProperties)
            {
                binderHelper.AddBinder(propertyInfo);
            }
            Parameters = binderHelper.GetPropertiesAndBinding().ToArray();
            HttpMethod = attribute.HttpMethod;
            Route = parsedRoute.Route;
            Definition = definition;
            ServiceName = attribute.ServiceName ?? definition.RequestType.Name + "Handler";
            ActionName = definition.RequestType.Name.EndsWith("Request")
                ? definition.RequestType.Name.Substring(0, definition.RequestType.Name.Length - "Request".Length)
                : definition.RequestType.Name;
        }

        public string ActionName { get; set; }

        public string ServiceName { get; set; }

        public IRequestDefinition Definition { get; set; }

        public string Route { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public HttpPropertyBinding[] Parameters { get; }
    }
}
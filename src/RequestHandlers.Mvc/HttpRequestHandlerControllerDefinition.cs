using System.Linq;
using System.Reflection;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc
{
    public class HttpRequestHandlerControllerDefinition
    {
        public HttpRequestHandlerControllerDefinition(HttpRequestAttribute attribute, IRequestDefinition definition)
        {
            var parsedRoute = HttpRequestAttributeParser.Parse(attribute);

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

            var binderHelper = new PropertyBinderHelper(defaultBinder, parsedRoute);
            var allProperties = definition.RequestType.GetProperties(bindingFlags);
            foreach (var propertyInfo in allProperties)
            {
                binderHelper.AddBinder(propertyInfo);
            }
            Parameters = binderHelper.GetPropertiesAndBinding().ToArray();
            HttpMethod = attribute.HttpMethod;
            Route = parsedRoute.Route;
            Definition = definition;
        }

        public IRequestDefinition Definition { get; set; }

        public string Route { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public PropertyBinderHelperResult[] Parameters { get; }
    }
}
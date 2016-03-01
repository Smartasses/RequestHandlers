using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using RequestHandlers.WebApi.Core;

namespace RequestHandlers.WebApi
{
    public static class WebApiConfig
    {
        public static Assembly ConfigureRequestHandlers(this HttpConfiguration config, params Assembly[] assemblies)
        {
            return config.ConfigureRequestHandlers(RequestHandlerFinder.InAssembly(assemblies));
        }
        public static Assembly ConfigureRequestHandlers(this HttpConfiguration config, RequestHandlerDefinition[] requestHandlerDefinitions)
        {
            var assembly = new ControllersBuilder().CreateControllers(typeof(IWebApiRequestProcessor<IHttpActionResult>),
                requestHandlerDefinitions, new WebApiTypes
                {
                    ApiController = typeof(ApiController),
                    HttpDeleteAttribute = typeof(HttpDeleteAttribute),
                    HttpGetAttribute = typeof(HttpGetAttribute),
                    HttpPostAttribute = typeof(HttpPostAttribute),
                    HttpPutAttribute = typeof(HttpPutAttribute),
                    RouteAttribute = typeof(RouteAttribute),
                    ResponseTypeAttribute = typeof(ResponseTypeAttribute)
                });

            var assemblyResolver = new DynamicAssemblyResolver(assembly);
            config.Services.Replace(typeof(IAssembliesResolver), assemblyResolver);
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new DynamicHttpControllerTypeResolver(assembly));
            return assembly;
        }
    }
}

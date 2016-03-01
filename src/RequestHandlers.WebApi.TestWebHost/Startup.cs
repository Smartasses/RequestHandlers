using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Newtonsoft.Json.Serialization;
using Owin;
using RequestHandlers.WebApi.Core;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(RequestHandlers.WebApi.TestWebHost.Startup))]

namespace RequestHandlers.WebApi.TestWebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            var assembly = config.ConfigureRequestHandlers(typeof (TestRequestHandler).Assembly);

            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var builder = new ContainerBuilder();

            builder.RegisterType<TestRequestHandler>().As<IRequestHandler<TestRequest, TestResponse>>();
            builder.RegisterType<WebApiProcessor>().As<IWebApiRequestProcessor<IHttpActionResult>>();
            builder.RegisterType<DefaultRequestProcessor>().As<IRequestProcessor>();
            builder.RegisterType<DefaultRequestDispacher>().As<IRequestDispatcher>();
            builder.RegisterType<RequestHandlerResolver>().As<IRequestHandlerResolver>();
            builder.RegisterApiControllers(assembly);
            builder.RegisterApiControllers(typeof(Startup).Assembly);
            builder.RegisterWebApiFilterProvider(config);
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            config.EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API")).EnableSwaggerUi();

            app.UseAutofacWebApi(config);
            app.UseAutofacMiddleware(container);
            app.UseWebApi(config);
        }
    }
}

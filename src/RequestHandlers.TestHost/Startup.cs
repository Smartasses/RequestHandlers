using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RequestHandlers.TestHost.RequestHandlers;

namespace RequestHandlers.TestHost
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IRequestProcessor, DefaultRequestProcessor>();
            services.AddTransient<IRequestDispatcher, DefaultRequestDispacher>();
            services.AddTransient<IRequestHandlerResolver>(x => new RequestHandlerResolver(x));
            var requestHandlerInterface = typeof(IRequestHandler<,>);
            foreach (var requestHandler in RequestHandlerFinder.InAssembly(this.GetType().GetTypeInfo().Assembly))
            {
                services.Add(new ServiceDescriptor(requestHandlerInterface.MakeGenericType(requestHandler.RequestType, requestHandler.ResponseType), requestHandler.RequestHandlerType, ServiceLifetime.Transient));
            }
            // Add framework services.
            services.AddMvc().AddApplicationPart(DynamicBuilder.Build(RequestHandlerFinder.InAssembly(GetType().GetTypeInfo().Assembly)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }

    class RequestHandlerResolver : IRequestHandlerResolver
    {
        private readonly IServiceProvider _provider;

        public RequestHandlerResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IRequestHandler<TRequest, TResponse> Resolve<TRequest, TResponse>()
        {
            var result = (IRequestHandler<TRequest, TResponse>)_provider.GetService(typeof(IRequestHandler<TRequest, TResponse>));
            return result;
        }
    }
}

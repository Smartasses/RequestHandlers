using Autofac;

namespace RequestHandlers.WebApi.TestWebHost
{
    public class RequestHandlerResolver : IRequestHandlerResolver
    {
        private readonly ILifetimeScope _scope;

        public RequestHandlerResolver(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IRequestHandler<TRequest, TResponse> Resolve<TRequest, TResponse>()
        {
            return _scope.Resolve<IRequestHandler<TRequest, TResponse>>();
        }
    }
}
using System.Linq;
using System.Reflection;

namespace RequestHandlers
{
    public class DefaultRequestDispacher : IRequestDispatcher
    {
        private readonly IRequestHandlerResolver _resolver;
        private readonly MethodInfo _mainMethod;

        public DefaultRequestDispacher(IRequestHandlerResolver resolver)
        {
            _resolver = resolver;
            _mainMethod = this.GetType().GetMethods().Single(x => x.Name == nameof(Process) && x.GetGenericArguments().Count() == 2);
        }

        public TResponse Process<TRequest, TResponse>(TRequest request)
        {
            var requestHandler = _resolver.Resolve<TRequest, TResponse>();
            return requestHandler.Handle(request);
        }

        public TResponse Process<TResponse>(IReturn<TResponse> request)
        {
            return (TResponse)_mainMethod
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(this, new object[] {request});
        }
    }
}
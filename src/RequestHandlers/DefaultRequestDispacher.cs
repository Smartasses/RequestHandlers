using System.Linq;
using System.Reflection;

namespace RequestHandlers
{
    public static class RequestDispatcherExtensions
    {
        private static readonly MethodInfo _mainMethod;

        static RequestDispatcherExtensions()
        {
            _mainMethod = typeof(IRequestDispatcher).GetMethods().Single(x => x.Name == nameof(Process) && x.GetGenericArguments().Count() == 2);
        }
        public static TResponse Process<TResponse>(this IRequestDispatcher dispatcher, IReturn<TResponse> request)
        {
            return (TResponse)_mainMethod
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(dispatcher, new object[] { request });
        }
    }
    public class DefaultRequestDispacher : IRequestDispatcher
    {
        private readonly IRequestHandlerResolver _resolver;

        public DefaultRequestDispacher(IRequestHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        public TResponse Process<TRequest, TResponse>(TRequest request)
        {
            var requestHandler = _resolver.Resolve<TRequest, TResponse>();
            return requestHandler.Handle(request);
        }
    }
}
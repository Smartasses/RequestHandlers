using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RequestHandlers
{
    public static class RequestDispatcherExtensions
    {
        private static readonly MethodInfo MainMethod;

        static RequestDispatcherExtensions()
        {
            MainMethod = typeof(IRequestDispatcher).GetTypeInfo().GetMethods().Single(x => x.Name == nameof(Process) && x.GetGenericArguments().Count() == 2);
        }
        public static TResponse Process<TResponse>(this IRequestDispatcher dispatcher, IReturn<TResponse> request)
        {
            return (TResponse)MainMethod
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(dispatcher, new object[] { request });
        }
        public static async Task<TResponse> ProcessAsync<TResponse>(this IRequestDispatcher dispatcher, IReturn<TResponse> request)
        {
            return await (Task<TResponse>)MainMethod
                .MakeGenericMethod(request.GetType(), typeof(Task<TResponse>))
                .Invoke(dispatcher, new object[] { request });
        }
    }
}
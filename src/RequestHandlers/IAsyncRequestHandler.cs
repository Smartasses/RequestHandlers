using System.Threading.Tasks;

namespace RequestHandlers
{
    public interface IAsyncRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, Task<TResponse>>
    {
        
    }
}
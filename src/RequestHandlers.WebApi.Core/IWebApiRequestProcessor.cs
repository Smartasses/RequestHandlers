namespace RequestHandlers.WebApi.Core
{
    public interface IWebApiRequestProcessor<out TResult>
    {
        TResult Process<TRequest, TResponse>(TRequest request, object controller);
    }
}
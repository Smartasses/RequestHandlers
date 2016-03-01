namespace RequestHandlers
{
    public interface IRequestHandlerResolver
    {
        IRequestHandler<TRequest, TResponse> Resolve<TRequest, TResponse>();
    }
}
namespace RequestHandlers
{
    public interface IRequestProcessor
    {
        TResponse Process<TRequest, TResponse>(TRequest request);
    }
}
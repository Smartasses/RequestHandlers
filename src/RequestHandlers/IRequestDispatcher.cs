namespace RequestHandlers
{
    public interface IRequestDispatcher
    {
        TResponse Process<TRequest, TResponse>(TRequest request);
        TResponse Process<TResponse>(IReturn<TResponse> request);
    }
}
namespace RequestHandlers
{
    public interface IRequestDispatcher
    {
        TResponse Process<TRequest, TResponse>(TRequest request);
    }
}
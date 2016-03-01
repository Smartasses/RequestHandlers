namespace RequestHandlers
{
    public class DefaultRequestProcessor : IRequestProcessor
    {
        private readonly IRequestDispatcher _requestDispatcher;

        public DefaultRequestProcessor(IRequestDispatcher requestDispatcher)
        {
            _requestDispatcher = requestDispatcher;
        }

        public TResponse Process<TRequest, TResponse>(TRequest request)
        {
            return _requestDispatcher.Process<TRequest, TResponse>(request);
        }
    }
}

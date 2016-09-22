namespace RequestHandlers
{
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
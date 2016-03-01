using System.Web.Http;
using System.Web.Http.Results;
using RequestHandlers.WebApi.Core;

namespace RequestHandlers.WebApi.TestWebHost
{
    public class WebApiProcessor : IWebApiRequestProcessor<IHttpActionResult>
    {
        private readonly IRequestProcessor _requestProcessor;

        public WebApiProcessor(IRequestProcessor requestProcessor)
        {
            _requestProcessor = requestProcessor;
        }

        public IHttpActionResult Process<TRequest, TResponse>(TRequest request, object controller)
        {
            var response = _requestProcessor.Process<TRequest, TResponse>(request);
            return new OkNegotiatedContentResult<TResponse>(response, (ApiController)controller);
        }
    }
}
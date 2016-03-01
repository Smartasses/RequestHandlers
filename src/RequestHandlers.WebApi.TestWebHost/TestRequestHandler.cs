using System;
using RequestHandlers.WebApi.Contracts;

namespace RequestHandlers.WebApi.TestWebHost
{
    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public TestResponse Handle(TestRequest request)
        {
            return new TestResponse
            {
                Now = DateTime.Now.AddMinutes(request.AddMinutes)
            };
        }
    }
    [HttpAction("api/test/{id}", Method.Post)]
    public class TestRequest : IReturn<TestResponse>
    {
        [FromRoute]
        public int Id { get; set; }
        public double AddMinutes { get; set; }
    }
    public class TestResponse
    {
        public DateTime Now { get; set; }
    }
}
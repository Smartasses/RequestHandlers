namespace RequestHandlers.TestHost.RequestHandlers
{
    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public TestResponse Handle(TestRequest request)
        {
            return new TestResponse
            {
                Param1 = request.Param1,
                Test2 = request.Test2,
                Test = request.Test
            };
        }
    }
}
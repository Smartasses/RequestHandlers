namespace RequestHandlers.TestHost.RequestHandlers
{
    [GetRequest("api/test/{param1}?test&test2")]
    public class TestRequest : IReturn<TestResponse>
    {
        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
}
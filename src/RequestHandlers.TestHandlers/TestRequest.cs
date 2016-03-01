namespace RequestHandlers.TestHandlers
{
    public class TestRequest : IReturn<TestResponse>
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}
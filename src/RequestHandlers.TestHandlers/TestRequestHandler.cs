using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestHandlers.TestHandlers
{
    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public TestResponse Handle(TestRequest request)
        {
            return new TestResponse
            {
                Result = request.A + request.B
            };
        }
    }
}

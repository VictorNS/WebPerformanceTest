using WebLoadTestUtils;

namespace WebLoadTest
{
	internal class SimplestWebAutoLoginTest : ILoadTestLoginAndResultParser
	{
		public RequestParameters GetRequestParameter()
		{
			return new RequestParameters("/Account/Login");
		}

		public ParseResult Parse(TestResponseResult testResponseResult)
		{
			return new ParseResult
			{
				IsSuccess = true
			};
		}
	}
}

using WebLoadTestUtils;

namespace WebLoadTest.Tests
{
	public class RegisterTest : ILoadTest
	{
		public bool AcceptLoginResult(object loginResult)
		{
			return true;
		}

		public RequestParameters GetRequestParameter()
		{
			return new RequestParameters("/Account/Register");
		}
	}
}

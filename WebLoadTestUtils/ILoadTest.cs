using System.Collections.Generic;
using System.Net;

namespace WebLoadTestUtils
{
	public interface ILoadTest
	{
		bool AcceptLoginResult(object loginResult);
		RequestParameters GetRequestParameter();
	}

	public interface ILoadTestAndResultParser : ILoadTest, IResultParser
	{
	}
}

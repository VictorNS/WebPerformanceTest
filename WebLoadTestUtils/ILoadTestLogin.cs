
namespace WebLoadTestUtils
{

	public interface ILoadTestLogin
	{
		RequestParameters GetRequestParameter();
	}

	public interface ILoadTestLoginAndResultParser : ILoadTestLogin, IResultParser
	{
	}

}

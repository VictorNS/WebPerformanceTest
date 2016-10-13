
namespace WebLoadTestUtils
{
	public interface IResultParser
	{
		ParseResult Parse(TestResponseResult testResponseResult);
	}
}

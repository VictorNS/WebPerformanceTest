
namespace WebLoadTestUtils
{
	public interface ILogger
	{
		void Debug(string format, params object[] args);

		void Info(string message);
		void Info(string format, object arg0, object arg1);
		void Info(string format, object arg0, object arg1, object arg2);
		void Info(string format, params object[] args);

		void Warn(string format, params object[] args);

		void Error(string format, params object[] args);
	}
}

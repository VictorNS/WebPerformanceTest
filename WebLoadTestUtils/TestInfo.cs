
namespace WebLoadTestUtils
{
	public class TestInfo
	{
		public int TaskNumber { get; set; }
		public TestStatus TestStatus { get; set; }

		public TestInfo(int taskNumber)
		{
			TaskNumber = taskNumber;
			TestStatus = TestStatus.Started;
		}
	}

	public enum TestStatus
	{
		Started,
		SuccessLogin,
		Error,
		Stopped
	}
}

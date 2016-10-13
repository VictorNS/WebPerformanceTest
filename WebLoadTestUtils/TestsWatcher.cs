using System.Collections.Generic;

namespace WebLoadTestUtils
{
	public interface ITestsWatcher
	{
		bool IsNeedStopTest { get; set; }
		bool IsNeedMeasureTest { get; set; }
		Dictionary<int, TestInfo> Tests { get; }
		void Initialize();
		void RegisterTest(int taskNumber);
		void MarkTestAsSuccessLogin(int taskNumber);
		void MarkTestAsStopped(int taskNumber);
		void MarkTestAsStoppedWithError(int taskNumber);
	}

	public class TestsWatcher : ITestsWatcher
	{
		public bool IsNeedStopTest { get; set; }
		public bool IsNeedMeasureTest { get; set; }
		public Dictionary<int, TestInfo> Tests { get; private set; }
		public object testsLock = new object();

		public TestsWatcher()
		{
			Tests = new Dictionary<int, TestInfo>(100);
		}

		public void Initialize()
		{
			lock (testsLock)
			{
				IsNeedStopTest = false;
				IsNeedMeasureTest = false;
				Tests.Clear();
			}
		}

		public void RegisterTest(int taskNumber)
		{
			lock (testsLock)
			{
				if (!Tests.ContainsKey(taskNumber))
				{
					Tests.Add(taskNumber, new TestInfo(taskNumber));
				}
			}
		}
		public void MarkTestAsSuccessLogin(int taskNumber)
		{
			lock (testsLock)
			{
				if (Tests.ContainsKey(taskNumber))
				{
					Tests[taskNumber].TestStatus = TestStatus.SuccessLogin;
				}
			}
		}
		public void MarkTestAsStopped(int taskNumber)
		{
			lock (testsLock)
			{
				if (Tests.ContainsKey(taskNumber))
				{
					Tests[taskNumber].TestStatus = TestStatus.Stopped;
				}
			}
		}
		public void MarkTestAsStoppedWithError(int taskNumber)
		{
			lock (testsLock)
			{
				if (Tests.ContainsKey(taskNumber))
				{
					Tests[taskNumber].TestStatus = TestStatus.Error;
				}
			}
		}
	}
}

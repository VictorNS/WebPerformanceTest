using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace WebLoadTestUtils
{
	public class TestExecutor
	{
		private Type _tLogin;
		private readonly Dictionary<string, Type> _tTests = new Dictionary<string, Type>();

		public void RegisterLogger<Tlogin>() where Tlogin : ILoadTestLogin
		{
			_tLogin = typeof(Tlogin);
		}
		public void RegisterTest<Ttest>(string key) where Ttest : ILoadTest
		{
			_tTests.Add(key, typeof(Ttest));
		}
		public void RegisterTest<Ttest>() where Ttest : ILoadTest
		{
			_tTests.Add(typeof(Ttest).Name, typeof(Ttest));
		}

		public void ExecuteTests(int amountTasks, IContainer container, TestParameters testParameters)
		{
			foreach (var tTest in _tTests.Values)
			{
				Execute(_tLogin, tTest, amountTasks, container, testParameters);
			}
		}
		public void ExecuteTests(string[] keys, int amountTasks, IContainer container, TestParameters testParameters)
		{
			foreach (var key in keys)
			{
				if (_tTests.ContainsKey(key))
					Execute(_tLogin, _tTests[key], amountTasks, container, testParameters);
			}
		}

		static void Execute(Type tLogin, Type tTest, int amountTasks, IContainer container, TestParameters testParameters)
		{
			var testName = tTest.Name;
			using (var scope = container.BeginLifetimeScope())
			{
				var logger = scope.Resolve<ILogger>();
				logger.Info("");
				logger.Info("TEST STARTED at {0} name: {1}", DateTime.Now, testName);
				logger.Info("");
				var testsWatcher = scope.Resolve<ITestsWatcher>();
				var taskWatcher = TestTaskExecutor.RunWatcherAsync(testsWatcher, logger, testParameters, testName);

				var taskList = new List<Task<int>>();
				for (var i = 0; i < amountTasks; i++)
				{
					try
					{
						var loginClass = (ILoadTestLogin)scope.Resolve(tLogin);
						var testClass = (ILoadTest)scope.Resolve(tTest);
						var task = TestTaskExecutor.ExecuteAsync(i, testsWatcher, logger, testParameters, loginClass, testClass);
						taskList.Add(task);
					}
					catch (Exception ex)
					{
						logger.Info(ex.ToString());
					}
				}
				var allTasks = taskList.Cast<Task>().ToList();
				allTasks.Add(taskWatcher);
				Task.WaitAll(allTasks.ToArray());

				var totalRequests = 0;
				var countTasks = 0;
				foreach (var endedTask in taskList)
				{
					totalRequests += endedTask.Result;
					if (endedTask.Result > 0)
						countTasks++;
				}

				logger.Info("");
				logger.Info("TEST COMPLETED at {0} name: {1}", DateTime.Now, testName);
				logger.Info("Throughput {0} : requests {1} in {2} sec ({3} threads)", totalRequests / testParameters.DurationCountSec, totalRequests, testParameters.DurationCountSec, countTasks);
				logger.Info("");

				var rep = scope.Resolve<IReportWriter>();
				rep.Write(testName, totalRequests / testParameters.DurationCountSec);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace WebLoadTestUtils
{
	public static class TestExecutor
	{
		public static void Execute<Tlogin, Ttest>(int amountTasks, IContainer container, TestParameters testParameters)
			where Tlogin : ILoadTestLogin
			where Ttest : ILoadTest
		{
			var testName = typeof(Ttest).Name;
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
						var loginClass = scope.Resolve<Tlogin>();
						var testClass = scope.Resolve<Ttest>();
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using PowerArgs;
using WebLoadTestUtils;
using SettingsReader = WebLoadTestUtils.Helpers.SettingsReader;

namespace WebLoadTest
{
	class Program
	{
		static int Main(string[] args)
		{
			MyArgs pars;
			string rootFolder;
			TestParameters testParameters;
			int amountTasks;
			ILogger logger;
			try
			{
				pars = Args.Parse<MyArgs>(args);

				rootFolder = SettingsReader.GetSetting("logRoot", "${basedir}/");
				var debug = pars.Debug ?? SettingsReader.GetSettingAsInt("debug", 0);
				logger = Helpers.NLogger.CreateLogger("WebLoadTestResult", rootFolder, debug == 1);
				amountTasks = pars.AmountTasks ?? SettingsReader.GetSettingAsInt("amountTasks", 1);
				testParameters = SettingsReader.GetTestParameters("http://localhost:80/");
				if (pars.PreheatSec.HasValue)
					testParameters.PreheatSec = pars.PreheatSec.Value;
				if (pars.DurationCountSec.HasValue)
					testParameters.DurationCountSec = pars.DurationCountSec.Value;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Console.ReadKey();
				return 1;
			}

			var container = createContainer(logger, rootFolder);
			if (container == null)
				return 1;

			runTests(logger, container, amountTasks, testParameters, pars.Tests);

			return 0;
		}

		private static IContainer createContainer(ILogger logger, string rootFolder)
		{
			try
			{
				var builder = new ContainerBuilder();
				builder.RegisterInstance(logger).As<ILogger>();
				var reporter = new Helpers.Reporter();
				builder
					.RegisterInstance(reporter.CreateReport(System.IO.Path.Combine(rootFolder, "WebLoadTestResult.csv")))
					.As<IReportWriter>().SingleInstance();
				builder.RegisterType<TestsWatcher>().As<ITestsWatcher>().SingleInstance();

				builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetExecutingAssembly())
					.Where(t => t.Name.EndsWith("Test"))
					.AsSelf()
					.InstancePerDependency();

				var container = builder.Build();
				return container;
			}
			catch (Exception ex)
			{
				logger.Info(ex.ToString());
				return null;
			}
		}

		private static void runTests(ILogger logger, IContainer container, int amountTasks, TestParameters testParameters, string[] tests)
		{
			var testExecutor = new TestExecutor();
			testExecutor.RegisterLogger<SimplestWebAutoLoginTest>();
			testExecutor.RegisterTest<Tests.RegisterTest>();
			testExecutor.ExecuteTests(amountTasks, container, testParameters);

			var rep = container.Resolve<IReportWriter>();
			rep.Dispose();
		}
	}

	public class MyArgs
	{
		[ArgRange(0, 1)]
		public int? Debug { get; set; }

		[ArgRange(1, 300)]
		public int? AmountTasks { get; set; }

		[ArgRange(0, 300)]
		public int? PreheatSec { get; set; }

		[ArgRange(1, 300)]
		public int? DurationCountSec { get; set; }

		public string[] Tests { get; set; }
	}
}

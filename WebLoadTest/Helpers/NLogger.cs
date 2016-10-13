using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace WebLoadTest.Helpers
{
	public class NLogger : WebLoadTestUtils.ILogger
	{
		private readonly Logger nLogger;

		public NLogger(Logger nLogger)
		{
			this.nLogger = nLogger;
		}

		public void Debug(string format, params object[] args)
		{
			nLogger.Debug(format, args);
		}

		public void Info(string message)
		{
			nLogger.Info(message);
		}
		public void Info(string format, object arg0, object arg1)
		{
			nLogger.Info(format, arg0, arg1);
		}
		public void Info(string format, object arg0, object arg1, object arg2)
		{
			nLogger.Info(format, arg0, arg1, arg2);
		}
		public void Info(string format, params object[] args)
		{
			nLogger.Info(format, args);
		}

		public void Warn(string format, params object[] args)
		{
			nLogger.Warn(format, args);
		}

		public void Error(string format, params object[] args)
		{
			nLogger.Error(format, args);
		}

		public static WebLoadTestUtils.ILogger CreateLogger(string testName, string rootFolder, bool includeDebug)
		{
			var config = new LoggingConfiguration();
			var consoleTarget = new ColoredConsoleTarget
			{
				Layout = "${message}"
			};
			var fileTarget = new FileTarget
			{
				FileName = rootFolder + testName + " " + DateTime.Now.ToString("u") + ".txt",
				Layout = "${message}"
			};
			config.AddTarget("console", consoleTarget);
			config.AddTarget("file", fileTarget);

			var actualLevel = includeDebug ? LogLevel.Debug : LogLevel.Info;
			var ruleConsole = new LoggingRule("*", actualLevel, consoleTarget);
			config.LoggingRules.Add(ruleConsole);
			var ruleFile = new LoggingRule("*", actualLevel, fileTarget);
			config.LoggingRules.Add(ruleFile);
			LogManager.Configuration = config;
			var nLogger = LogManager.GetLogger("log");
			var logger = new NLogger(nLogger);
			return logger;
		}

	}
}

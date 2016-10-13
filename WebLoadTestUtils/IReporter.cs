using System;
using System.Collections.Generic;

namespace WebLoadTestUtils
{
	public interface IReporter
	{
		IReportWriter CreateReport(string fileName);
	}
	public interface IReportWriter : IDisposable
	{
		void Write(params object[] cells);
	}
}

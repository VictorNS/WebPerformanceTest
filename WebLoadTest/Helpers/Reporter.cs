using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLoadTestUtils;

namespace WebLoadTest.Helpers
{
	public class Reporter : IReporter
	{
		public IReportWriter CreateReport(string fileName)
		{
			var reportWriter = new ReportWriter(fileName);
			return reportWriter;
		}
	}

	public class ReportWriter : IReportWriter
	{
		private readonly string fileName;
		private readonly List<ReportRow> rows = new List<ReportRow>();

		public ReportWriter(string fileName)
		{
			this.fileName = fileName;
		}

		public void Write(params object[] cells)
		{
			var reportRow = new ReportRow();
			foreach (var cell in cells)
			{
				var t = cell.GetType();
				if (t == typeof(byte) || t == typeof(int) || t == typeof(long))
					reportRow.Cells.Add(new ReportCell { Type = ReportCellType.Int, Value = cell });
				else if (t == typeof(decimal) || t == typeof(double))
					reportRow.Cells.Add(new ReportCell { Type = ReportCellType.Numeric, Value = cell });
				else if (t == typeof(DateTime))
					reportRow.Cells.Add(new ReportCell { Type = ReportCellType.DateTime, Value = cell });
				else
					reportRow.Cells.Add(new ReportCell { Type = ReportCellType.String, Value = cell });
			}
			rows.Add(reportRow);
		}

		public void Dispose()
		{
			var sb = new StringBuilder();
			foreach (var row in rows)
			{
				var length = row.Cells.Count();
				var i = 0;
				foreach (var cell in row.Cells)
				{
					i++;
					switch (cell.Type)
					{
						case ReportCellType.Int:
							sb.Append(cell.Value);
							break;
						case ReportCellType.Numeric:
							sb.Append(((decimal)cell.Value).ToString("F"));
							break;
						case ReportCellType.DateTime:
							sb.Append(((DateTime)cell.Value).ToString("MM.dd.yyyy HH:mm"));
							break;
						default:
							sb.Append("\"");
							sb.Append(cell.Value.ToString().Replace("\"", "\"\""));
							sb.Append("\"");
							break;
					}
					if (i != length)
						sb.Append(";");
				}
				sb.AppendLine();
			}

			System.IO.File.WriteAllText(fileName, sb.ToString());
		}
	}

	public class ReportRow
	{
		public List<ReportCell> Cells { get; set; }

		public ReportRow()
		{
			Cells = new List<ReportCell>();
		}
	}
	public class ReportCell
	{
		public ReportCellType Type { get; set; }
		public object Value { get; set; }
	}
	public enum ReportCellType
	{
		Int = 0,
		Numeric = 1,
		String = 2,
		DateTime
	}
}

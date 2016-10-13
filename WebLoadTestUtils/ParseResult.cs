using System;

namespace WebLoadTestUtils
{
	public class ParseResult
	{
		public bool IsSuccess { get; set; }
		public string ErrorMessage { get; set; }
		public object Result { get; set; }
	}
}

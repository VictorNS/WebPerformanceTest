using System;

namespace WebLoadTestUtils
{
	public class TestParameters
	{
		public string RequestBaseUri { get; set; }
		public int PreheatSec { get; set; }
		public int DurationCountSec { get; set; }
	}
}

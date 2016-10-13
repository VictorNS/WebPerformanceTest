using System.Collections.Generic;
using System.Net;

namespace WebLoadTestUtils
{
	public class TestResponseResult
	{
		public RequestParameters RequestParameters { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public IEnumerable<Cookie> Cookies { get; set; }
		public IEnumerable<string> NewCookies { get; set; }
		public string Response { get; set; }
		public object ParseResult { get; set; }
	}
}

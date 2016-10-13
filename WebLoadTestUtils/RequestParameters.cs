using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace WebLoadTestUtils
{
	public class RequestParameters
	{
		public HttpMethod HttpMethod { get; set; }
		public string RequestUri { get; set; }
		public HttpContent Content { get; set; }
		public string ContentAsString { get; set; }

		/// <summary>
		/// GET
		/// </summary>
		public RequestParameters(string uri)
		{
			HttpMethod = HttpMethod.Get;
			RequestUri = uri;
		}

		public static RequestParameters CreatePostRequest(string uri, object formContent)
		{
			var list = new List<KeyValuePair<string, string>>();
			var contentAsString = "";

			foreach (PropertyInfo pInfo in formContent.GetType().GetProperties())
			{
				var value = pInfo.GetValue(formContent, null).ToString();
				list.Add(new KeyValuePair<string, string>(pInfo.Name, value));
				contentAsString += pInfo.Name + "=" + value + " ";
			} 

			return new RequestParameters(uri)
			{
				HttpMethod = HttpMethod.Post,
				ContentAsString = contentAsString,
				Content = new FormUrlEncodedContent(list)
			};
		}

		public static RequestParameters CreatePostJsonRequest(string uri, object jsonContent)
		{
			var contentAsString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonContent);
			return new RequestParameters(uri)
			{
				HttpMethod = HttpMethod.Post,
				ContentAsString = contentAsString,
				Content = new StringContent(
					contentAsString,
					System.Text.Encoding.UTF8,
					"application/json")
			};
		}

	}
}

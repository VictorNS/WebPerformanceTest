using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebLoadTestUtils
{
	public static class TestTaskExecutor
	{
		public static async Task<int> ExecuteAsync(int taskNumber, ITestsWatcher testsWatcher, ILogger logger, TestParameters testParameters, ILoadTestLogin loginClass, ILoadTest testClass)
		{
			System.Threading.Thread.Sleep(10 * taskNumber); // we don't want to kill IIS
			testsWatcher.RegisterTest(taskNumber);
			#region prepare HttpClientHandler
			var cookieContainer = new CookieContainer();
			using (var handler = new HttpClientHandler()
			{
				CookieContainer = cookieContainer,
				UseCookies = true,
				AllowAutoRedirect = false
			})
			#endregion prepare HttpClientHandler
			using (var client = new HttpClient(handler))
			{
				client.BaseAddress = new Uri(testParameters.RequestBaseUri);
				client.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("deflate"));
				HttpResponseMessage httpResponseMessage = null;
				object parseResult = null;

				#region login
				var loginClassAndResultParser = loginClass as ILoadTestLoginAndResultParser;
				var isSuccess = false;
				for (int i = 0; i < 3; i++)
				{
					RequestParameters loginRequest;
					try
					{
						loginRequest = loginClass.GetRequestParameter();
					}
					catch (Exception ex)
					{
						logger.Error("Task: {0} Can't get login request: {1}", taskNumber, ex.ToString());
						return 0;
					}
					try
					{
						httpResponseMessage = await clientSendAsync(client, loginRequest);
						string contentStringResult;

						#region gather response result
						IEnumerable<string> cookieSet;
						var newCookies = httpResponseMessage.Headers.TryGetValues("set-cookie", out cookieSet) ? cookieSet.ToList() : new List<string>();
						var cookies = cookieContainer.GetCookies(client.BaseAddress).Cast<Cookie>().ToList();
						using (var content = httpResponseMessage.Content)
						{
							contentStringResult = await content.ReadAsStringAsync();
						}
						logger.Debug("Task: {0} Status: {1} Uri: {3} Result: {2}", taskNumber, httpResponseMessage.StatusCode, (contentStringResult ?? "NULL"), httpResponseMessage.RequestMessage.RequestUri);
						if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
						{
							logger.Warn("Task: {0} Move to {1}", taskNumber, httpResponseMessage.Headers.Location);
						}
						#endregion gather response result

						if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
						{
							#region
							if (loginClassAndResultParser != null)
							{
								try
								{
									var res = loginClassAndResultParser.Parse(new TestResponseResult
									{
										RequestParameters = loginRequest,
										StatusCode = httpResponseMessage.StatusCode,
										Cookies = cookies,
										NewCookies = newCookies,
										Response = contentStringResult,
										ParseResult = parseResult
									});
									isSuccess = res.IsSuccess;
									parseResult = res.Result;
									if (!res.IsSuccess)
									{
										logger.Info("Task: {0} Invalid parse result: {1}", taskNumber, res.ErrorMessage);
									}
								}
								catch (Exception ex)
								{
									logger.Error("Task: {0} Invalid login: {1}", taskNumber, ex.ToString());
									return 0;
								}
							}
							else
							{
								isSuccess = true;
							}
							#endregion
						}

						#region log

						if (!isSuccess)
						{
							if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
							{
								logger.Info("Task: {0} Moved to {1}", taskNumber, httpResponseMessage.Headers.Location);
							}
							logger.Info("Cookies for {0}:", client.BaseAddress);
							foreach (var cookie in cookies)
								logger.Info(cookie.Name + ": " + cookie.Value);
							logger.Info("Set cookies:");
							foreach (var c in newCookies)
								logger.Info(c);
						}

						#endregion log
					}
					catch (Exception ex)
					{
						logger.Error("Task: {0} Error: {1}", taskNumber, ex.ToString());
					}
					finally
					{
						if (httpResponseMessage != null)
							httpResponseMessage.Dispose();
					}
					if (isSuccess)
						break;
				}
				#endregion login

				testsWatcher.MarkTestAsSuccessLogin(taskNumber);
				System.Threading.Thread.Sleep(1000);
				var count = 0;

				#region repeat
				httpResponseMessage = null;
				try
				{
					if (!testClass.AcceptLoginResult(parseResult))
					{
						logger.Error("Task: {0} Can't initialize test", taskNumber);
						testsWatcher.MarkTestAsStopped(taskNumber);
						return 0;
					}
				}
				catch (Exception ex)
				{
					logger.Error("Task: {0} Can't initialize test: {1}", taskNumber, ex.ToString());
					testsWatcher.MarkTestAsStoppedWithError(taskNumber);
					return 0;
				}
				var testClassAndResultParser = testClass as ILoadTestAndResultParser;
				while (!testsWatcher.IsNeedStopTest)
				{
					RequestParameters testRequest;
					try
					{
						testRequest = testClass.GetRequestParameter();
					}
					catch (Exception ex)
					{
						logger.Error("Task: {0} Can't get test request: {1}", taskNumber, ex.ToString());
						testsWatcher.MarkTestAsStoppedWithError(taskNumber);
						return 0;
					}
					try
					{
						httpResponseMessage = await clientSendAsync(client, testRequest);
						#region gather response result
						string contentStringResult;
						using (var content = httpResponseMessage.Content)
						{
							contentStringResult = await content.ReadAsStringAsync();
						}
						var logResult = (contentStringResult == null)
							? "NULL"
							: string.IsNullOrWhiteSpace(contentStringResult)
								? "EMPTY"
								: contentStringResult.Replace(Environment.NewLine, "");
						var logRequestParams = (testRequest.ContentAsString == null) ? "" : "Request: " + testRequest.ContentAsString;
						if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
						{
							logger.Debug(Environment.NewLine + "Task: {0} Status: {1} Uri: {2} {3} Result: {4}", taskNumber, httpResponseMessage.StatusCode, httpResponseMessage.RequestMessage.RequestUri,
								((logRequestParams.Length > 1024) ? logRequestParams.Substring(0, 1024) + "..." : logRequestParams),
								((contentStringResult.Length > 1024) ? logResult.Substring(0, 1024) + "..." : logResult));
						}
						else
						{
							logger.Warn(Environment.NewLine + "Task: {0} Status: {1} Uri: {2} {3} Result: {4}", taskNumber, httpResponseMessage.StatusCode, httpResponseMessage.RequestMessage.RequestUri,
								((logRequestParams.Length > 1024) ? logRequestParams.Substring(0, 1024) + "..." : logRequestParams),
								((contentStringResult.Length > 1024) ? logResult.Substring(0, 1024) + "..." : logResult));
						}
						if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
						{
							logger.Warn("Task: {0} Move to {1}", taskNumber, httpResponseMessage.Headers.Location);
						}
						#endregion gather response result
						if (testClassAndResultParser != null)
						{
							#region
							IEnumerable<string> cookieSet;
							var newCookies = httpResponseMessage.Headers.TryGetValues("set-cookie", out cookieSet) ? cookieSet.ToList() : new List<string>();
							var cookies = cookieContainer.GetCookies(client.BaseAddress).Cast<Cookie>().ToList();

							try
							{
								var res = testClassAndResultParser.Parse(new TestResponseResult
								{
									RequestParameters = testRequest,
									StatusCode = httpResponseMessage.StatusCode,
									Cookies = cookies,
									NewCookies = newCookies,
									Response = contentStringResult,
									ParseResult = parseResult
								});
								if (!res.IsSuccess)
								{
									logger.Error("Task: {0} Break: {1}", taskNumber, res.ErrorMessage);
									testsWatcher.MarkTestAsStoppedWithError(taskNumber);
									return 0;
								}
							}
							catch (Exception ex)
							{
								logger.Error("Task: {0} Parse result error: {1}", taskNumber, ex.ToString());
								testsWatcher.MarkTestAsStoppedWithError(taskNumber);
								return 0;
							}
							#endregion
						}
					}
					catch (Exception ex)
					{
						logger.Error("Task: {0} Error: {1}", taskNumber, ex.ToString());
						testsWatcher.MarkTestAsStoppedWithError(taskNumber);
						return 0;
					}
					finally
					{
						if (httpResponseMessage != null)
							httpResponseMessage.Dispose();
					}
					if (testsWatcher.IsNeedMeasureTest)
						count++;
				}
				#endregion repeat

				testsWatcher.MarkTestAsStopped(taskNumber);
				return count;
			}

		}

		public static async Task RunWatcherAsync(ITestsWatcher testsWatcher, ILogger logger, TestParameters testParameters, string testName)
		{
			testsWatcher.Initialize();
			
			await Task.Delay(1000); // after login we wait 1 sec
			while (true)
			{
				await Task.Delay(100);
				if (testsWatcher.Tests.Values.All(it => it.TestStatus != TestStatus.Started))
				{
					break;
				}
			}
			logger.Info("");
			logger.Info("start preheat {0} tests", testsWatcher.Tests.Values.Count(it => it.TestStatus == TestStatus.SuccessLogin));

			var startTestTime = DateTime.Now.AddSeconds(testParameters.PreheatSec);
			var endTestTime = startTestTime.AddSeconds(testParameters.DurationCountSec);
			var endTime = endTestTime.AddSeconds(3);
			var currentDateTime = DateTime.Now;
			while (currentDateTime < startTestTime)
			{
				await Task.Delay(100);
				currentDateTime = DateTime.Now;
			}
			testsWatcher.IsNeedMeasureTest = true;
			logger.Info("");
			logger.Info("START MEASURE {0} TESTS at {1} name: {2}", testsWatcher.Tests.Values.Count(it => it.TestStatus == TestStatus.SuccessLogin), DateTime.Now, testName);

			while (currentDateTime < endTestTime)
			{
				await Task.Delay(100);
				currentDateTime = DateTime.Now;
			}
			testsWatcher.IsNeedMeasureTest = false;
			logger.Info("STOP  MEASURE {0} TESTS at {1} name: {2}", testsWatcher.Tests.Values.Count(it => it.TestStatus == TestStatus.SuccessLogin), DateTime.Now, testName);

			while (currentDateTime < endTime)
			{
				await Task.Delay(100);
				currentDateTime = DateTime.Now;
			}
			testsWatcher.IsNeedStopTest = true;
		}

		private static async Task<HttpResponseMessage> clientSendAsync(HttpClient httpClient, RequestParameters requestParameters)
		{
			if (requestParameters.HttpMethod == HttpMethod.Post)
			{
				return await httpClient.PostAsync(requestParameters.RequestUri, requestParameters.Content);
			}
			if (requestParameters.HttpMethod == HttpMethod.Delete)
			{
				return await httpClient.DeleteAsync(requestParameters.RequestUri);
			}
			if (requestParameters.HttpMethod == HttpMethod.Put)
			{
				return await httpClient.PutAsync(requestParameters.RequestUri, requestParameters.Content);
			}
			return await httpClient.GetAsync(requestParameters.RequestUri);
		}
	}
}

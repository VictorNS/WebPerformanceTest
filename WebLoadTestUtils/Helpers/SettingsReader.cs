using System.Configuration;

namespace WebLoadTestUtils.Helpers
{
	public static class SettingsReader
	{
		public static int GetSettingAsInt(string key, int defaultValue)
		{
			var str = ConfigurationManager.AppSettings[key];
			if (str != null)
			{
				int.TryParse(str, out defaultValue);
			}
			return defaultValue;
		}

		public static string GetSetting(string key, string defaultValue)
		{
			var str = ConfigurationManager.AppSettings[key];
			if (str != null)
			{
				return str;
			}
			return defaultValue;
		}

		public static TestParameters GetTestParameters(string defaultUri)
		{
			var requestBaseUri = GetSetting("requestBaseUri", defaultUri);
			var preheatSec = GetSettingAsInt("preheatSec", 120);
			var durationCountSec = GetSettingAsInt("durationCountSec", 60);
			var testParameters = new TestParameters
			{
				RequestBaseUri = requestBaseUri,
				PreheatSec = preheatSec,
				DurationCountSec = durationCountSec
			};
			return testParameters;
		}
	}
}

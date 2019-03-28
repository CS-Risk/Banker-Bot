using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BankerBot
{
	/// <summary>
	/// usage: var debugSetting = AppSettings.Get<bool>("Debug");
	/// </summary>
	public static class AppSettings
	{
		public static T Get<T>(string key)
		{
			var appSetting = ConfigurationManager.AppSettings[key];
			if (string.IsNullOrWhiteSpace(appSetting)) throw new AppSettingNotFoundException(key);

			var converter = TypeDescriptor.GetConverter(typeof(T));
			return (T)(converter.ConvertFromInvariantString(appSetting));
		}
	}


	internal class AppSettingNotFoundException : Exception
	{
		public AppSettingNotFoundException()
		{
		}

		public AppSettingNotFoundException(string message) : base(message)
		{
		}

		public AppSettingNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AppSettingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}


}

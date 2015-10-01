using System;
using System.Collections.Generic;
using System.IO;
using Kelpie.Core.Domain;
using Environment = Kelpie.Core.Domain.Environment;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Kelpie.Core
{
	public class Configuration : IConfiguration
	{
		public string ConfigFile { get; set; }

		public List<string> Applications { get; set; }
		public List<Environment> Environments { get; set; }
		public int ImportBufferSize { get; set; }
		public int PageSize { get; set; }
		public int MaxAgeDays { get; set; }

		private Configuration()
		{
			ConfigFile = "";
			Applications = new List<string>();
		}

		public static IConfiguration Read()
		{
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kelpie.config");
			if (!File.Exists(configFilePath))
				throw new KelpieException("Cannot find the Kelpie.config file.");

			string json = File.ReadAllText(configFilePath);
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			if (!string.IsNullOrEmpty(config.ConfigFile) && File.Exists(config.ConfigFile))
			{
				json = File.ReadAllText(config.ConfigFile);
				config = JsonConvert.DeserializeObject<Configuration>(json);
			}

			return config;
		}
	}
}

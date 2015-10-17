using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Kelpie.Core.Domain;
using Kelpie.Core.Exceptions;
using Newtonsoft.Json;
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
		public int MaxAgeDays { get; set; }
		public bool IsLuceneEnabled { get; set; }
		public string LuceneIndexDirectory { get; set; }

		private Configuration()
		{
			ConfigFile = "";
			Applications = new List<string>();
		}

		/// <summary>
		/// Reads and parses the JSON-based config file.
		/// </summary>
		/// <param name="configFilePath">If null or empty, reads the config file from the current directory the assembly is located in.</param>
		/// <returns></returns>
		public static IConfiguration Read(string configFilePath = "")
		{
			if (string.IsNullOrEmpty(configFilePath))
				configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kelpie.config");

			try
			{
				if (!File.Exists(configFilePath))
					throw new InvalidConfigurationFileException("Cannot find the Kelpie.config file.");

				string json = File.ReadAllText(configFilePath);
				var config = JsonConvert.DeserializeObject<Configuration>(json);

				if (!string.IsNullOrEmpty(config.ConfigFile) && File.Exists(config.ConfigFile))
				{
					configFilePath = config.ConfigFile;
                    json = File.ReadAllText(configFilePath);
					config = JsonConvert.DeserializeObject<Configuration>(json);
				}

				if (config.Environments.Count == 0)
					throw new InvalidConfigurationFileException("No environments were found in the configuration file.");

				if (!config.Environments.SelectMany(x => x.Servers).Any())
					throw new InvalidConfigurationFileException("No servers were found in the configuration file.");

				return config;
			}
			catch (JsonException e)
			{
				throw new InvalidConfigurationFileException("The kelpie configuration file '{0}' has some invalid JSON: {1}", configFilePath, e.Message);
			}
		}
	}
}

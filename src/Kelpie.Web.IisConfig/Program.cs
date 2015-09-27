using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IisConfiguration;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;

namespace Kelpie.Web.IisConfig
{
	class Program
	{
		static void Main(string[] args)
		{
			var logger = new ConsoleLogger();
			var serverConfig = new WebServerConfig(logger);

			if (!serverConfig.IsIis7OrAbove)
			{
				logger.LogHeading("IIS7 is not installed on this machine. IIS configuration setup terminated.");
				return;
			}

			var envConfig = new Config();

			try
			{
				serverConfig
					.AddAppPool(envConfig.SiteName, "v4.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalSystem)
					.WithProcessModel(envConfig.IdleTimeout, envConfig.PingingEnabled)
					.Commit();

				serverConfig
					.AddSite(envConfig.SiteName, envConfig.PortNumber, envConfig.PortNumber)
					.AddApplication("/", envConfig.WebRoot, envConfig.SiteName)
					.WithLogging(false)
					.Commit();
			}
			catch (Exception e)
			{
				logger.LogError(e);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelpie.Core
{
	public class Configuration : IConfiguration
	{
		public IEnumerable<string> Applications { get; private set; }
		public IEnumerable<string> Servers { get; private set; }
		public string ServerUsername { get; set; }
		public string ServerPassword { get; set; }

		public Configuration()
		{
			ServerUsername = ConfigurationManager.AppSettings["serverUsername"];
			ServerPassword = ConfigurationManager.AppSettings["serverPassword"];

			string appsCsv = ConfigurationManager.AppSettings["applications"];
			string serversCsv = ConfigurationManager.AppSettings["servers"];

			if (string.IsNullOrEmpty(appsCsv))
				throw new InvalidOperationException("Oops! You have no applications defined. Add them to the web.config using <add key=\"applications\" value=\"app1,app2\" />");

			if (string.IsNullOrEmpty(serversCsv))
				throw new InvalidOperationException(@"Oops! You have no servers defined. Add them to the web.config using <add key=""servers"" value=""\\localhost\d$\logs1,\\serverWithLongName.domain.com\Logs"" />");

			Applications = new List<string>(appsCsv.Split(','));
			Servers = new List<string>(serversCsv.Split(','));
		}
	}
}

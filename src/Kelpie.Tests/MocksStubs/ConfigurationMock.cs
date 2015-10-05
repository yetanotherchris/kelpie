using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Tests.MocksStubs
{
	public class ConfigurationMock : IConfiguration
	{
		public string ConfigFile { get; set; }
		public List<string> Applications { get; set; }
		public List<Environment> Environments { get; set; }
		public int ImportBufferSize { get; set; }
		public int PageSize { get; set; }
		public int MaxAgeDays { get; set; }

		public ConfigurationMock()
		{
			Environments = new List<Environment>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using Kelpie.Core.Domain;

namespace Kelpie.Tests.MocksStubs
{
	public class ConfigurationStub : IConfiguration
	{
		public string ConfigFile { get; set; }
		public List<string> Applications { get; set; }
		public List<Server> Servers { get; set; }
		public int ImportBufferCount { get; set; }
		public int PageSize { get; set; }
		public int MaxAgeDays { get; set; }
	}
}

using System.Collections.Generic;
using Kelpie.Core.Domain;

namespace Kelpie.Core
{
	public interface IConfiguration
	{
		string ConfigFile { get; set; }
		List<string> Applications { get; set; }
		List<Server> Servers { get; set; }
		int ImportBufferCount { get; set; }
		int PageSize { get; set; }
		int MaxAgeDays { get; set; }
	}
}
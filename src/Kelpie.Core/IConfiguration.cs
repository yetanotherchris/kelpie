using System.Collections.Generic;
using Kelpie.Core.Domain;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Core
{
	public interface IConfiguration
	{
		string ConfigFile { get; set; }
		List<string> Applications { get; set; }
		List<Environment> Environments { get; set; }
		int ImportBufferSize { get; set; }
		int PageSize { get; set; }
		int MaxAgeDays { get; set; }
	}
}
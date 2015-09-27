using System.Collections.Generic;

namespace Kelpie.Core
{
	public interface IConfiguration
	{
		IEnumerable<string> Applications { get; }
		IEnumerable<string> ServerPaths { get; }
		string ServerUsername { get; set; }
	}
}
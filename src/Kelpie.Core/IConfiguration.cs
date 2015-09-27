using System.Collections.Generic;

namespace Kelpie.Core
{
	public interface IConfiguration
	{
		IEnumerable<string> Applications { get; }
		IEnumerable<string> Servers { get; }
		string ServerUsername { get; set; }
	}
}
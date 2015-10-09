using System.Collections.Generic;
using Kelpie.Core.Domain;

namespace Kelpie.Core.Import
{
	public class ServerLogFileContainer
	{
		public Environment Environment { get; set; }
		public Server Server { get; set; }
		public IEnumerable<AppLogFiles> AppLogFiles { get; set; }

		public ServerLogFileContainer()
		{
			AppLogFiles = new List<AppLogFiles>();
		}
	}
}
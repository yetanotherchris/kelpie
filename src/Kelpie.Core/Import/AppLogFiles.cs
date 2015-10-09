using System.Collections.Generic;

namespace Kelpie.Core.Import
{
	public class AppLogFiles
	{
		public string Appname { get; set; }
		public List<string> LogfilePaths { get; set; }

		public AppLogFiles()
		{
			LogfilePaths = new List<string>();
		}

		public void UpdatePath(string oldPath, string newPath)
		{
			// Doesn't need to be thread safe as we're not removing any items
			int index = LogfilePaths.IndexOf(oldPath);
			LogfilePaths[index] = newPath;
		}
	}
}
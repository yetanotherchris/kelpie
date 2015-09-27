using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelpie.Core
{
	public class ServerLogReader
	{
		private string[] _servers =
		{
			"", ""
		};

		public IEnumerable<LogEntry> LogsForServer()
		{
			foreach (string directory in Directory.EnumerateDirectories(@"D:\ErrorLogs"))
			{
				string applicationName = Path.GetFileName(directory);
				string[] logFiles = Directory.GetFiles(directory, "*.log");
				foreach (string file in logFiles)
				{
					var parser = new LogFileParser(file, "localhost", applicationName);
					IEnumerable<LogEntry> entries = parser.Read();
				}
			}

			return null;
		}
	}
}

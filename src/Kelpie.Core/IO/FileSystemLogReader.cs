using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kelpie.Core.Domain;
using Kelpie.Core.Parser;

namespace Kelpie.Core.IO
{
	public class FileSystemLogReader
	{
		private readonly Configuration _configuration;

		public FileSystemLogReader(Configuration configuration)
		{
			_configuration = configuration;
		}

		public IEnumerable<LogEntry> ScanLogDirectories()
		{
			var entries = new List<LogEntry>();

			Parallel.ForEach(_configuration.ServerPaths, (serverPath) =>
			{
				if (serverPath.StartsWith("\\") && !serverPath.StartsWith("\\localhost"))
				{
					// Net use the server
					PinvokeWindowsNetworking.ConnectToRemote(serverPath, _configuration.ServerUsername, _configuration.ServerPassword);
				}

				entries.AddRange(GetLogsForServer(serverPath));
			});

			return entries;
		}

		private IEnumerable<LogEntry> GetLogsForServer(string fullPath)
		{
			var serverEntries = new List<LogEntry>();

			Parallel.ForEach(Directory.EnumerateDirectories(fullPath), (directory) =>
			{
				string applicationName = Path.GetFileName(directory);
				string[] logFiles = Directory.GetFiles(directory, "*.log");

				foreach (string file in logFiles)
				{
					var uri = new Uri(fullPath);
					string serverName = uri.Host;
					if (string.IsNullOrEmpty(serverName))
						serverName = "localhost";

					var parser = new LogFileParser(file, serverName, applicationName);
					serverEntries.AddRange(parser.Read());
				}
			});

			return serverEntries;
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kelpie.Core.Domain;
using Kelpie.Core.Parser;
using Kelpie.Core.Repository;

namespace Kelpie.Core.IO
{
	public class FileSystemLogReader
	{
		private readonly Configuration _configuration;
		private readonly LogEntryRepository _repository;

		public FileSystemLogReader(Configuration configuration, LogEntryRepository repository)
		{
			_configuration = configuration;
			_repository = repository;
		}

		private void WriteCyan(string text, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(text, args);
			Console.ForegroundColor = ConsoleColor.White;
		}

		public void ScanLogDirectoriesAndAdd()
		{
			foreach (string serverPath in _configuration.ServerPaths)
			{
                if (serverPath.StartsWith("\\") && !serverPath.StartsWith("\\localhost"))
				{
					// Net use the server
					PinvokeWindowsNetworking.ConnectToRemote(serverPath, _configuration.ServerUsername, _configuration.ServerPassword);
				}

				var uri = new Uri(serverPath);
				string serverName = uri.Host;
				if (string.IsNullOrEmpty(serverName))
					serverName = "localhost";

				AddLogsForServer(serverPath, serverName);
			}
		}

		private void AddLogsForServer(string fullPath, string serverName)
		{
			IEnumerable<string> directories = Directory.EnumerateDirectories(fullPath).ToList();

			string tempRoot = Path.Combine(Path.GetTempPath(), "Kelpie", serverName);
			if (!Directory.Exists(tempRoot))
				Directory.CreateDirectory(tempRoot);

			foreach (string directory in directories)
			{
				string[] logFiles = Directory.GetFiles(directory, "*.log");

				if (logFiles.Any())
				{
                    string appName = Path.GetFileName(directory);
					string destPath = Path.Combine(tempRoot, appName);
					WriteCyan("Copying files from {0} to {1}", directory, destPath);

					if (!Directory.Exists(destPath))
						Directory.CreateDirectory(destPath);

					foreach (string file in logFiles)
					{
						string destFileName = file.Replace(directory, destPath);
						File.Copy(file, destFileName, true);
						Console.WriteLine("- Copied {0}", file);

						var parser = new LogFileParser(destFileName, serverName, appName);
						IEnumerable<LogEntry> entries = parser.Read();
						Console.WriteLine("- Saving {0} items for {1}", entries.Count(), appName);

						_repository.BulkSave(entries);
					}
				}
			}
		}
	}
}

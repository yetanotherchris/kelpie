using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
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

		public void ScanLogDirectoriesAndAdd()
		{
			foreach (string serverPath in _configuration.ServerPaths)
			{
                if (serverPath.StartsWith(@"\\") && !serverPath.StartsWith(@"\\localhost"))
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
			Console.WriteLine("- Search for all .log files in {0}", fullPath);
			IEnumerable<string> logFiles = Directory.EnumerateFiles(fullPath, "*.log", SearchOption.AllDirectories);

			if (logFiles.Any())
			{
				string tempRoot = Path.Combine(Path.GetTempPath(), "Kelpie", serverName);
				if (!Directory.Exists(tempRoot))
					Directory.CreateDirectory(tempRoot);

				Parallel.ForEach(logFiles, (file) =>
				{
					// Ignore old log files as they bloat the database
					if (File.GetLastWriteTime(file) >= DateTime.UtcNow.AddDays(-7))
					{
						string directory = Path.GetDirectoryName(file);

						// Copy the log file to the %TEMP% directory
						string appName = Path.GetFileName(directory);
						string destPath = Path.Combine(tempRoot, appName);

						if (!Directory.Exists(destPath))
							Directory.CreateDirectory(destPath);

						Console.WriteLine("- Copying {0} to local disk", file);
						string destFileName = file.Replace(directory, destPath);
						File.Copy(file, destFileName, true);

						Console.WriteLine("Parsing {0} ({1})", file, ByteSize.FromBytes(new FileInfo(file).Length).ToString());
						var parser = new LogFileParser(destFileName, serverName, appName, _repository);
						parser.ParseAndSave();
					}
					else
					{
						Console.WriteLine("Ignoring {0} as it's more than 7 days old", file);
					}
				});
			}
		}
	}
}

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
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Core.IO
{
	public class LogReaderOptions
	{
		public bool CopyFiles { get; set; }
		public bool ImportData { get; set; }
		public bool DeleteData { get; set; }
		public bool SmartUpdate { get; set; }
	}

	public class FileSystemLogReader
	{
		private readonly IConfiguration _configuration;
		private readonly ILogEntryRepository _repository;

		public FileSystemLogReader(IConfiguration configuration, ILogEntryRepository repository)
		{
			_configuration = configuration;
			_repository = repository;
		}

		public void ScanLogDirectoriesAndAdd()
		{
			foreach (Environment environment in _configuration.Environments)
			{
				foreach (Server server in environment.Servers)
				{
					if (!string.IsNullOrEmpty(server.Username))
					{
						// Net use the server
						PinvokeWindowsNetworking.ConnectToRemote(server.Path, server.Username, server.Password);
					}

					AddLogsForServer(environment.Name, server);
				}
			}
		}

		private void AddLogsForServer(string environment, Server server)
		{
			Console.WriteLine("- Search for all .log files in {0}", server.Path);
			IEnumerable<string> logFiles = Directory.EnumerateFiles(server.Path, "*.log", SearchOption.AllDirectories);

			if (logFiles.Any())
			{
				string tempRoot = Path.Combine(Path.GetTempPath(), "Kelpie", server.Name);
				if (!Directory.Exists(tempRoot))
					Directory.CreateDirectory(tempRoot);

				Parallel.ForEach(logFiles, (file) =>
				{
					// Ignore old log files as they bloat the database
					if (File.GetLastWriteTime(file) >= DateTime.UtcNow.AddDays(- _configuration.MaxAgeDays))
					{
						string destFileName = file;
						string directory = Path.GetDirectoryName(file);
						string appName = Path.GetFileName(directory);

						if (server.CopyFilesToLocal)
						{
							// Copy the log file to the %TEMP% directory
							string destPath = Path.Combine(tempRoot, appName);

							if (!Directory.Exists(destPath))
								Directory.CreateDirectory(destPath);

							Console.WriteLine("- Copying {0} to local disk", file);
							destFileName = file.Replace(directory, destPath);
							File.Copy(file, destFileName, true);
						}

						Console.WriteLine("Parsing {0} ({1})", file, ByteSize.FromBytes(new FileInfo(file).Length).ToString());
						var parser = new LogFileParser(destFileName, server.Name, appName, environment, _repository);
						parser.ParseAndSave();
					}
					else
					{
						Console.WriteLine("Ignoring {0} as it's more than {1} days old", file, _configuration.MaxAgeDays);
					}
				});
			}
		}
	}
}

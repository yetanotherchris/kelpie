using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using Kelpie.Core.Domain;
using Kelpie.Core.Import.Parser;
using Kelpie.Core.Repository;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Core.Import
{
	public class FileSystemLogReader
	{
		private readonly IConfiguration _configuration;

		public FileSystemLogReader(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Scans all servers for a given environment.
		/// </summary>
		/// <param name="environmentName">The name of the environment, which should match the kelpie.config file.</param>
		public IEnumerable<ServerLogFileContainer> ScanSingleEnvironment(string environmentName)
		{
			var list = new List<ServerLogFileContainer>();

			Environment environment =
				_configuration.Environments.FirstOrDefault(
					x => x.Name.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase));

			if (environment != null)
			{
				foreach (Server server in environment.Servers)
				{
					IEnumerable<AppLogFiles> apps = GetLogfilePaths(server);
					var container = new ServerLogFileContainer()
					{
						Server = server,
						Environment = environment,
						AppLogFiles = apps
					};
					list.Add(container);
				}
			}

			return list;
		}

		/// <summary>
		/// Scans a single server's log files.
		/// </summary>
		/// <param name="serverName">The name of the server, which should match the kelpie.config file.</param>
		public ServerLogFileContainer ScanSingleServer(string serverName)
		{
			foreach (Environment environment in _configuration.Environments)
			{
				var server = environment.Servers.FirstOrDefault(x => x.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase));

				if (server != null)
				{
					IEnumerable<AppLogFiles> apps = GetLogfilePaths(server);
					var container = new ServerLogFileContainer()
					{
						Server = server,
						Environment = environment,
						AppLogFiles = apps
					};
					return container;
					break;
				}
			}

			return null;
		}

		/// <summary>
		/// Scans every environment and each server in that environment.
		/// </summary>
		public IEnumerable<ServerLogFileContainer> ScanAllEnvironments()
		{
			var list = new List<ServerLogFileContainer>();

			foreach (Environment environment in _configuration.Environments)
			{
				foreach (Server server in environment.Servers)
				{
					IEnumerable<AppLogFiles> apps = GetLogfilePaths(server);
					var container = new ServerLogFileContainer()
					{
						Server = server,
						Environment = environment,
						AppLogFiles = apps
					};
					list.Add(container);
				}
			}

			return list;
		}

		private IEnumerable<AppLogFiles> GetLogfilePaths(Server server)
		{
			if (!string.IsNullOrEmpty(server.Username))
			{
				// Net use the server
				LogLine("- Attempting to authenticate with {0} using {1}", server.Path, server.Username);
				PinvokeWindowsNetworking.ConnectToRemote(server.Path, server.Username, server.Password);
			}

			LogLine("- Searching for all .log files in {0}", server.Path);
			var appDictionary = new Dictionary<string, AppLogFiles>();
			IEnumerable<string> paths = Directory.EnumerateFiles(server.Path, "*.log", SearchOption.AllDirectories);

			// Assume the parent folder of the each log file is the name of the application,
			// e.g. D:\ErrorLogs\MyApp\errors.log => "MyApp"
			foreach (string path in paths)
			{
				string appName = Path.GetDirectoryName(path);
				if (appDictionary.ContainsKey(appName))
				{
					appDictionary[appName].LogfilePaths.Add(path);
				}
				else
				{
					var appLogFile = new AppLogFiles() { Appname = appName };
					appLogFile.LogfilePaths.Add(path);

					appDictionary.Add(appName, appLogFile);
				};
			}

			return appDictionary.Select(app => app.Value);
		}

		/// <summary>
		/// Copies all log files to the local disk in the temporary folder.
		/// </summary>
		/// <param name="container"></param>
		public void CopyToLocalDisk(ServerLogFileContainer container)
		{
			if (!container.AppLogFiles.Any())
				return;


			string tempRoot = Path.Combine(Path.GetTempPath(), "Kelpie", container.Server.Name);
			if (!Directory.Exists(tempRoot))
				Directory.CreateDirectory(tempRoot);

			foreach (AppLogFiles appLogFile in container.AppLogFiles)
			{
				Parallel.ForEach(appLogFile.LogfilePaths, (filePath) =>
				{
					if (File.GetLastWriteTime(filePath) >= DateTime.UtcNow.AddDays(-_configuration.MaxAgeDays))
					{
						// Copy the log file to the %TEMP% directory
						string sourceDir = Path.GetDirectoryName(filePath);
						string destDir = Path.Combine(tempRoot, appLogFile.Appname);

						if (!Directory.Exists(destDir))
							Directory.CreateDirectory(destDir);

						LogLine("- Copying {0} to local disk", filePath);
						string destFilePath = filePath.Replace(sourceDir, destDir);
						File.Copy(filePath, destFilePath, true);

						appLogFile.UpdatePath(filePath, destFilePath);
					}
					else
					{
						LogLine("Ignoring {0} as it's more than {1} days old", filePath, _configuration.MaxAgeDays);
					}
				});
			}
		}

		private void LogLine(string format, params object[] args)
		{
			// TODO: add logger
			System.Console.WriteLine(format, args);
		}
	}
}

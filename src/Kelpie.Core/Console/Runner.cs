using System.Collections.Generic;
using System.Linq;
using CommandLine.Text;
using Kelpie.Core.Import;
using Kelpie.Core.Import.Parser;
using Kelpie.Core.Repository;
using MongoDB.Driver;

namespace Kelpie.Core.Console
{
	public class Runner
	{
		private readonly ILogEntryRepository _repository;
		private readonly IConfiguration _configuration;

		public Runner()
		{
			_configuration = Configuration.Read();
			_repository = new LogEntryRepository(new MongoClient(), _configuration);
		}

		internal Runner(IConfiguration configuration, ILogEntryRepository repository)
		{
			_configuration = configuration;
			_repository = repository;
		}

		public string Run(string[] args)
		{
			var options = new Options();
			bool result = CommandLine.Parser.Default.ParseArguments(args, options);

			if (result == false)
			{
				// Display the default usage information
				return options.GetUsage();
			}
			else
			{
				if (options.WipeData)
				{
					_repository.DeleteAll();
				}

				var logReader = new FileSystemLogReader(_configuration);
				var containerList = new List<ServerLogFileContainer>() ;

				if (!string.IsNullOrEmpty(options.Environment))
				{
					containerList = logReader.ScanSingleEnvironment(options.Environment).ToList();
				}
				else if (!string.IsNullOrEmpty(options.Server))
				{
					containerList.Add(logReader.ScanSingleServer(options.Server));
				}
				else
				{
					containerList = logReader.ScanAllEnvironments().ToList();
				}

				if (options.CopyFiles)
				{
					foreach (ServerLogFileContainer container in containerList)
					{
						logReader.CopyToLocalDisk(container);
					}
				}

				if (options.SkipImport == false)
				{
					var parser = new LogFileParser(_repository);
                    foreach (ServerLogFileContainer container in containerList)
					{
						parser.ParseAndSave(container);
					}
				}

				return "done";
			}
		}
	}
}
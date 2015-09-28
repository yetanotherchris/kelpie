using System;
using System.Net;
using Kelpie.Core;
using Kelpie.Core.IO;
using Kelpie.Core.Repository;
using MongoDB.Driver;

namespace Kelpie.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var runner = new Runner();
			runner.Refresh();

			Console.WriteLine("Done");
		}
	}

	public class Runner
	{
		private readonly LogEntryRepository _repository;
		private readonly Configuration _configuration;

		public Runner()
		{
			_configuration = new Configuration();
			_repository = new LogEntryRepository(new MongoClient());
		}

		public void Refresh()
		{
			_repository.DeleteAll();

			var logReader = new FileSystemLogReader(_configuration, _repository);
			logReader.ScanLogDirectoriesAndAdd();
		}
	}
}

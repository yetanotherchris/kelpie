using Kelpie.Core;
using Kelpie.Core.IO;
using Kelpie.Core.Repository;
using MongoDB.Driver;

namespace Kelpie.ConsoleApp
{
	public class Runner
	{
		private readonly LogEntryRepository _repository;
		private readonly IConfiguration _configuration;

		public Runner()
		{
			_configuration = Configuration.Read();
			_repository = new LogEntryRepository(new MongoClient(), _configuration);
		}

		public void Refresh()
		{
			_repository.DeleteAll();

			var logReader = new FileSystemLogReader(_configuration, _repository);
			logReader.ScanLogDirectoriesAndAdd();
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kelpie.Core.Domain;
using Kelpie.Core.Import;
using Kelpie.Core.Import.Parser;
using Kelpie.Tests.MocksStubs;
using NUnit.Framework;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Tests.Integration
{
    public class LogFileParserTests
    {
	    private const string APP_NAME = "appname";

	    private ServerLogFileContainer CreateContainer(string logPath)
	    {
			var server = new Server()
			{
				CopyFilesToLocal = true,
				Name = "server",
				Username = "uid",
				Password = "pwd",
				Path = ""
			};
			var environment = new Environment() { Name = "environment" };
			environment.Servers.Add(server);

			var appLogFiles = new List<AppLogFiles>();
			appLogFiles.Add(new AppLogFiles()
			{
				LogfilePaths = { logPath },
				Appname = APP_NAME
			});

			var container = new ServerLogFileContainer();
		    container.Environment = environment;
		    container.Server = server;
		    container.AppLogFiles = appLogFiles;

		    return container;
	    }

		[Test]
	    public void read_should_return_empty_list_when_file_is_empty()
	    {
		    // Arrange
			var container = CreateContainer("ExampleLogs/empty.log");
            var repository = new RepositoryMock();
            var logFileParser = new LogFileParser(repository);

			// Act
			logFileParser.ParseAndSave(container);
			IEnumerable<LogEntry> list = repository.LogEntries;

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(0));
	    }

		[Test]
		public void read_should_parse_all_entries()
		{
			// Arrange
			var container = CreateContainer("ExampleLogs/full.log");
			var repository = new RepositoryMock();
			DateTime expectedDate = DateTime.Parse("2015-09-24 10:25:13.7780");
            var logFileParser = new LogFileParser(repository);

			// Act
			logFileParser.ParseAndSave(container);
			IEnumerable<LogEntry> list = repository.LogEntries;

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(648));

			var entry = list.FirstOrDefault();
			Assert.That(entry.DateTime, Is.EqualTo(expectedDate));
			Assert.That(entry.ApplicationName, Is.EqualTo(APP_NAME));
			Assert.That(entry.Server, Is.EqualTo(container.Server.Name));
			Assert.That(entry.Environment, Is.EqualTo(container.Environment.Name));
			Assert.That(entry.Source, Is.EqualTo("AmazingApp"));
			Assert.That(entry.Message, Is.EqualTo("A non critical error occured on Page:http://www.example.com/Places/To/Buy/Caravans/WestLondon.html\r\n"));
		}

		[Test]
		public void read_should_parse_last_exception_type_and_message_from_entry_stack()
		{
			// Arrange
			var container = CreateContainer("ExampleLogs/full.log");
			var repository = new RepositoryMock();
			var logFileParser = new LogFileParser(repository);

			// Act
			logFileParser.ParseAndSave(container);
			List<LogEntry> list = repository.LogEntries;

			// Assert
			var entry = list[1];
			Assert.That(entry.DateTime, Is.GreaterThan(DateTime.MinValue));
			Assert.That(entry.ExceptionType, Is.EqualTo("System.NeedSleepException"));
			Assert.That(entry.ExceptionMessage, Is.EqualTo("I can't get no sleep sleep sleep."));
		}
	}
}

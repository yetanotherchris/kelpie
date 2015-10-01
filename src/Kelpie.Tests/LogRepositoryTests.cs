using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;
using Kelpie.Tests.MocksStubs;
using MongoDB.Driver;
using NUnit.Framework;

namespace Kelpie.Tests
{
	public class LogRepositoryTests
	{
		private ConfigurationStub _configuration;
		private string _environmentName = "Dev";

		[SetUp]
		public void SetUp()
		{
			_configuration = new ConfigurationStub();
			_configuration.MaxAgeDays = 99;
			CreateRepository().DeleteCollection("LogEntry");
        }

		private LogEntryRepository CreateRepository()
		{
			return new LogEntryRepository(new MongoClient(), _configuration, "Kelpie-tests");
		}

		[Test]
		public void should_save_entry()
		{
			// Arrange
			string environment = _environmentName;
			string applicationName = "FooApp";

            var repository = CreateRepository();
			var entry = CreatLogEntry(applicationName, "crap");

			// Act
			repository.Save(entry);

			// Assert
			IEnumerable<LogEntry> entries = repository.GetEntriesForApp(environment, applicationName);
			Assert.That(entries.Count(), Is.EqualTo(1));
		}

		[Test]
		public void should_load_entry_for_application()
		{
			// Arrange
			string environment = _environmentName;
			string applicationName = "FooApp";

			var repository = CreateRepository();
			var entry1 = CreatLogEntry(applicationName, "crap1");
			var entry2 = CreatLogEntry(applicationName, "crap2");
			repository.Save(entry1);
			repository.Save(entry2);

			// Act
			IEnumerable<LogEntry> entries = repository.GetEntriesForApp(environment, applicationName);

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(2));
		}

		[Test]
		public void should_not_load_entry_for_different_application()
		{
			// Arrange
			string environment = _environmentName;
			string fooApp = "FooApp";
			string barApp = "Bar"; 

			var repository = CreateRepository();
			var entry1 = CreatLogEntry(fooApp, "crap1");
			var entry2 = CreatLogEntry(fooApp, "crap2");
			repository.Save(entry1);
			repository.Save(entry2);

			// Act
			IEnumerable<LogEntry> entries = repository.GetEntriesForApp(environment, barApp);

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(0));
		}

		[Test]
		public void should_load_entries_for_today()
		{
			// Arrange
			string environment = _environmentName;
			string applicationName = "FooApp";

			var repository = CreateRepository();
			var entry1 = CreatLogEntry(applicationName, "today");
			entry1.DateTime = DateTime.Now;

			var entry2 = CreatLogEntry(applicationName, "today");
			entry2.DateTime = DateTime.Now;

			var entry3 = CreatLogEntry(applicationName, "yesterday");
			entry3.DateTime = DateTime.Now.AddDays(-1);

			var entry4 = CreatLogEntry(applicationName, "2 days ago");
			entry4.DateTime = DateTime.Now.AddDays(-2);

			repository.Save(entry1);
			repository.Save(entry2);
			repository.Save(entry3);
			repository.Save(entry4);

			// Act
			IEnumerable<LogEntry> entries = repository.GetEntriesToday(environment, applicationName);

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(2));
		}

		[Test]
		public void should_load_entries_for_this_week()
		{
			// Arrange
			string environment = _environmentName;
			string applicationName = "FooApp";

			var repository = CreateRepository();
			var entry1 = CreatLogEntry(applicationName, "today");
			entry1.DateTime = DateTime.Now;

			var entry2 = CreatLogEntry(applicationName, "this time last week");
			entry2.DateTime = DateTime.Now.AddDays(-7);

			var entry3 = CreatLogEntry(applicationName, "yesterday");
			entry3.DateTime = DateTime.Now.AddDays(-1);

			var entry4 = CreatLogEntry(applicationName, "8 days ago");
			entry4.DateTime = DateTime.Now.AddDays(-8);

			repository.Save(entry1);
			repository.Save(entry2);
			repository.Save(entry3);
			repository.Save(entry4);

			// Act
			IEnumerable<LogEntry> entries = repository.GetEntriesThisWeek(environment, applicationName);

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(3));
		}

		// TODO: GetEntriesThisWeekGroupedByException
		// TODO: FindByExceptionType
		// TODO: GetEntry

		private LogEntry CreatLogEntry(string application, string message)
		{
			return new LogEntry()
			{
				DateTime = DateTime.UtcNow,
				Source = "FooAppLogger",
				Level = "Error",
				ApplicationName = application,
				Message = message,
				Environment = _environmentName
			};
		}
	}
}

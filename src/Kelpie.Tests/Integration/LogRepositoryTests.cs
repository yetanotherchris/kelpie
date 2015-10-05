using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;
using Kelpie.Tests.MocksStubs;
using MongoDB.Driver;
using NUnit.Framework;

namespace Kelpie.Tests.Integration
{
    public class LogRepositoryTests
    {
        private ConfigurationMock _configuration;
        private string _environmentName = "Dev";

		[SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationMock();
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
			var entry = CreateLogEntry(applicationName, "crap");

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
			var entry1 = CreateLogEntry(applicationName, "crap1");
			var entry2 = CreateLogEntry(applicationName, "crap2");
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
			var entry1 = CreateLogEntry(fooApp, "crap1");
			var entry2 = CreateLogEntry(fooApp, "crap2");
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
			var entry1 = CreateLogEntry(applicationName, "today");
			entry1.DateTime = DateTime.Now;

			var entry2 = CreateLogEntry(applicationName, "today");
			entry2.DateTime = DateTime.Now;

			var entry3 = CreateLogEntry(applicationName, "yesterday");
			entry3.DateTime = DateTime.Now.AddDays(-1);

			var entry4 = CreateLogEntry(applicationName, "2 days ago");
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
			var entry1 = CreateLogEntry(applicationName, "today");
			entry1.DateTime = DateTime.Now;

			var entry2 = CreateLogEntry(applicationName, "this time last week");
			entry2.DateTime = DateTime.Now.AddDays(-7);

			var entry3 = CreateLogEntry(applicationName, "yesterday");
			entry3.DateTime = DateTime.Now.AddDays(-1);

			var entry4 = CreateLogEntry(applicationName, "8 days ago");
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
		// TODO: paging

		private LogEntry CreateLogEntry(string application, string message)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;
using Kelpie.Tests.MocksStubs;
using MongoDB.Driver;
using NUnit.Framework;

namespace Kelpie.Tests.Integration
{
	public class LogRepositoryPagingTests
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
        [TestCase(1, 1, 1)]
        [TestCase(-1, -1, 2)]
        public void should_load_entry_for_application_paged(int page, int rows, int expectedResults)
		{
			// Arrange
			string logApplication = "FooApp";

			var repository = CreateRepository();
			AddEntryToLog(repository, logApplication, "foo1");
			AddEntryToLog(repository, logApplication, "foo2");
			Thread.Sleep(1000);

			// Act
			IEnumerable<LogEntry> entries = repository.GetFilterEntriesForApp(new LogEntryFilter() { LogApplication = logApplication, Page = page, Rows = rows });

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(expectedResults));
		}

		[Test]
        [TestCase(1, 1, 1)]
        [TestCase(-1, -1, 2)]
        [TestCase(10, 1, 0)]
        public void should_load_entry_for_application_for_today(int page, int rows, int expectedResults)
		{
			// Arrange
			string logApplication = "FooApp";
			DateTime start = DateTime.Now.Date;
			DateTime end = start.AddHours(24);

			var repository = CreateRepository();
			AddEntryToLog(repository, logApplication, "foo1");
			AddEntryToLog(repository, logApplication, "foo2");
			Thread.Sleep(1000);

			// Act
			IEnumerable<LogEntry> entries = repository.GetFilterEntriesForApp(new LogEntryFilter()
			{
				LogApplication = logApplication,
				Page = page,
				Rows = rows,
				Start = start,
				End = end
			});

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(expectedResults));
		}

		[Test]
		[TestCase(1, 1, 1)]
		public void should_load_entry_for_application_for_this_week(int page, int rows, int expectedResults)
		{
			// Arrange
			string logApplication = "FooApp";
			DateTime start = DateTime.Now.Date;
			DateTime end = start.AddDays(7);

			var repository = CreateRepository();
			AddEntryToLog(repository, logApplication, "foo1");
			AddEntryToLog(repository, logApplication, "foo2");
			Thread.Sleep(1000);

			// Act
			IEnumerable<LogEntry> entries = repository.GetFilterEntriesForApp(new LogEntryFilter()
			{
				LogApplication = logApplication,
				Page = page,
				Rows = rows,
				Start = start,
				End = end
			});

			// Assert
			Assert.That(entries.Count(), Is.EqualTo(expectedResults));
		}

		private void AddEntryToLog(LogEntryRepository repository, string application, string message)
		{
			repository.Save(CreateLogEntry(application, message));
		}

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

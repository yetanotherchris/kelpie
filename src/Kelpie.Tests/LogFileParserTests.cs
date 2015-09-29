using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Kelpie.Core.Parser;
using Kelpie.Tests.MocksStubs;
using NUnit.Framework;

namespace Kelpie.Tests
{
    public class LogFileParserTests
    {
		[Test]
	    public void read_should_return_empty_list_when_file_is_empty()
	    {
		    // Arrange
			var repository = new RepositoryMock();
            var logFileParser = new LogFileParser("ExampleLogs/empty.log", "local", "Sand", repository);

			// Act
			logFileParser.ParseAndSave();
			IEnumerable<LogEntry> list = repository.LogEntries;

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(0));
	    }

		[Test]
		public void read_should_parse_all_entries()
		{
			// Arrange
			var repository = new RepositoryMock();
			DateTime expectedDate = DateTime.Parse("2015-09-24 10:25:13.7780");
            var logFileParser = new LogFileParser("ExampleLogs/full.log", "local", "Sand", repository);

			// Act
			logFileParser.ParseAndSave();
			IEnumerable<LogEntry> list = repository.LogEntries;

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(648));

			var entry = list.FirstOrDefault();
			Assert.That(entry.DateTime, Is.EqualTo(expectedDate));
			Assert.That(entry.ApplicationName, Is.EqualTo("Sand"));
			Assert.That(entry.Server, Is.EqualTo("local"));
			Assert.That(entry.Source, Is.EqualTo("AmazingApp"));
			Assert.That(entry.Message, Is.EqualTo("A non critical error occured on Page:http://www.example.com/Places/To/Buy/Caravans/WestLondon.html\r\n"));
		}

		[Test]
		public void read_should_parse_last_exception_type_and_message_from_entry_stack()
		{
			// Arrange
			var repository = new RepositoryMock();
			var logFileParser = new LogFileParser("ExampleLogs/full.log", "local", "Sand", repository);

			// Act
			logFileParser.ParseAndSave();
			List<LogEntry> list = repository.LogEntries;

			// Assert
			var entry = list[1];
			Assert.That(entry.DateTime, Is.GreaterThan(DateTime.MinValue));
			Assert.That(entry.ExceptionType, Is.EqualTo("System.NeedSleepException"));
			Assert.That(entry.ExceptionMessage, Is.EqualTo("I can't get no sleep sleep sleep."));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using NUnit.Framework;

namespace Kelpie.Tests
{
    public class LogFileParserTests
    {
		[Test]
	    public void read_should_return_empty_list_when_file_is_empty()
	    {
		    // Arrange
			var logFileParser = new LogFileParser("ExampleLogs/empty.log", "local", "Sand");

			// Act
			IEnumerable<LogEntry> list = logFileParser.Read();

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(0));
	    }

		[Test]
		public void read_should_parse_all_entries()
		{
			// Arrange
			var logFileParser = new LogFileParser("ExampleLogs/full.log", "local", "Sand");

			// Act
			IEnumerable<LogEntry> list = logFileParser.Read();

			// Assert
			Assert.That(list, Is.Not.Null);
			Assert.That(list.Count(), Is.EqualTo(648));

			var entry = list.LastOrDefault();
			Assert.That(entry.DateTime, Is.GreaterThan(DateTime.MinValue));
			Assert.That(entry.Source, Is.EqualTo("AmazingApp"));
			Assert.That(entry.Message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void read_should_parse_last_exception_type_and_message_from_entry_stack()
		{
			// Arrange
			var logFileParser = new LogFileParser("ExampleLogs/full.log", "local", "Sand");

			// Act
			List<LogEntry> list = logFileParser.Read().ToList();

			// Assert
			var entry = list[1];
			Assert.That(entry.DateTime, Is.GreaterThan(DateTime.MinValue));
			Assert.That(entry.ExceptionType, Is.EqualTo("System.NeedSleepException"));
			Assert.That(entry.ExceptionMessage, Is.EqualTo("I can't get no sleep sleep sleep."));
		}
	}
}

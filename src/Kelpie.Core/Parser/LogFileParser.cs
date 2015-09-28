using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;

namespace Kelpie.Core.Parser
{
	public class LogFileParser
	{
		private static readonly Regex _entryRegex = new Regex(@"(?<date>\d{4}-\d{2}-\d{2}\s{1}\d{2}:\d{2}:\d{2}\.\d{4})\|ERROR\|(?<source>\w+?)\|(?<message>.*?)",
																RegexOptions.Singleline | RegexOptions.Compiled);

		private readonly string _filePath;
		private readonly string _serverName;
		private readonly string _application;
		private readonly ILogEntryRepository _repository;

		/// <summary>
		/// Number of items to parse before calling repository.Save(), and thus GC'ing the list of entries.
		/// </summary>
		public int MaxEntriesBeforeSave { get; set; }

		public LogFileParser(string filePath, string serverName, string application, ILogEntryRepository repository)
		{
			_filePath = filePath;
			_serverName = serverName;
			_application = application;
			_repository = repository;

			MaxEntriesBeforeSave = 100;
		}

		public void ParseAndSave()
		{
			var list = new List<LogEntry>();

			var stringBuilder = new StringBuilder();
			using (var streamReader = new StreamReader(new FileStream(_filePath, FileMode.Open)))
			{
				//  Read lines of text in, until we find an "|ERROR" as the delimiter and parse everything up to that line.
				int lineCount = 0;
				while (!streamReader.EndOfStream)
				{
					string currentLine = streamReader.ReadLine();

					if (!string.IsNullOrEmpty(currentLine) && currentLine.Contains("|ERROR") && lineCount > 0)
					{
						list.Add(ParseLogEntry(stringBuilder.ToString()));

						lineCount = 0;
						stringBuilder = new StringBuilder();

						if (list.Count >= MaxEntriesBeforeSave)
						{
							Console.WriteLine("- Saving {0} items for {1}", list.Count, _application);
							_repository.BulkSave(list);
							list = new List<LogEntry>();
                        }
					}

					stringBuilder.AppendLine(currentLine);
					lineCount++;
				}

				// Any remaining
				if (list.Count > 0)
				{
					Console.WriteLine("- Saving {0} items for {1}", list.Count, _application);
					_repository.BulkSave(list);
				}
			}
		}

		private LogEntry ParseLogEntry(string contents)
		{
			MatchCollection matches = _entryRegex.Matches(contents);
			var entry = new LogEntry();

			if (matches.Count > 0)
			{
				try
				{
					Match match = matches[0];

					entry.DateTime = DateTime.Parse(match.Groups["date"].Value);
					entry.Source = match.Groups["source"].Value;
					entry.Level = "Error";
					entry.Server = _serverName;
					entry.ApplicationName = _application;
					entry.Message = contents.Substring((match.Groups["source"].Index + 1) + match.Groups["source"].Length);
					FillExceptionType(entry);
				}
				catch (Exception)
				{
					// Ignore for now
				}
			}

			return entry;
		}

		private void FillExceptionType(LogEntry entry)
		{
			if (string.IsNullOrEmpty(entry.Message))
				return;

			string message = entry.Message;

			int start = message.LastIndexOf("Exception Type: ");
			if (start > -1)
			{
				int end = message.IndexOf(Environment.NewLine, start);
				if (end > -1)
				{
					int typeStart = start + "Exception Type: ".Length;
					string exceptionType = message.Substring(typeStart, end - typeStart);
					entry.ExceptionType = exceptionType;

					int messageStart = message.IndexOf("Message: ", start);
					if (messageStart > -1)
					{
						int messageEnd = message.IndexOf(Environment.NewLine, messageStart);
						if (messageEnd > -1)
						{
							messageStart = messageStart + "Message: ".Length;
							string exceptionMessage = message.Substring(messageStart, messageEnd - messageStart);
							entry.ExceptionMessage = exceptionMessage;
						}
					}
				}
			}
		}
	}
}

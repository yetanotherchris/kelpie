using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ByteSizeLib;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;
using Environment = System.Environment;

namespace Kelpie.Core.Import.Parser
{
	public class LogFileParser
	{
		private static readonly Regex _entryRegex = new Regex(@"(?<date>\d{4}-\d{2}-\d{2}\s{1}\d{2}:\d{2}:\d{2}\.\d{4})\|ERROR\|(?<source>\w+?)\|(?<message>.*?)",
																RegexOptions.Singleline | RegexOptions.Compiled);


		private readonly ILogEntryRepository _repository;

		/// <summary>
		/// Number of items to parse before calling repository.Save(), and thus GC'ing the list of entries.
		/// </summary>
		public int MaxEntriesBeforeSave { get; set; }

		public LogFileParser(ILogEntryRepository repository)
		{
			_repository = repository;
			MaxEntriesBeforeSave = 25;
		}

		public void ParseAndSave(ServerLogFileContainer container)
		{
			if (!container.AppLogFiles.Any())
				return;

			foreach (AppLogFiles appLogFile in container.AppLogFiles)
			{
				Parallel.ForEach(appLogFile.LogfilePaths, (filePath) =>
				{
					LogLine("Parsing {0} ({1})", filePath, ByteSize.FromBytes(new FileInfo(filePath).Length).ToString());
					ParseAndSaveSingleLog(container.Environment.Name, container.Server.Name, appLogFile.Appname, filePath);
				});
			}
		}

		private void LogLine(string format, params object[] args)
		{
			// TODO: add logger
			System.Console.WriteLine(format, args);
		}

		private void ParseAndSaveSingleLog(string environment, string server, string appName, string filePath)
		{
			var list = new List<LogEntry>();

			var stringBuilder = new StringBuilder();
			using (var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
			{
				//  Read lines of text in, until we find an "|ERROR" as the delimiter and parse everything up to that line.
				int lineCount = 0;
				while (!streamReader.EndOfStream)
				{
					string currentLine = streamReader.ReadLine();

					if (!string.IsNullOrEmpty(currentLine) && currentLine.Contains("|ERROR") && lineCount > 0)
					{
						LogEntry logEntry = ParseLogEntry(environment, server, appName, stringBuilder.ToString());
						list.Add(logEntry);

						lineCount = 0;
						stringBuilder = new StringBuilder();

						if (list.Count >= MaxEntriesBeforeSave)
						{
							System.Console.WriteLine("- Saving {0} items from {1}", list.Count, filePath);
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
					System.Console.WriteLine("- Saving {0} items from {1}", list.Count, filePath);
					_repository.BulkSave(list);
				}
			}
		}

		private LogEntry ParseLogEntry(string environment, string server, string appName, string contents)
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
					entry.Message = contents.Substring((match.Groups["source"].Index + 1) + match.Groups["source"].Length);
					entry.Level = "Error";
					entry.Server = server;
					entry.ApplicationName = appName;
					entry.Environment = environment;
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

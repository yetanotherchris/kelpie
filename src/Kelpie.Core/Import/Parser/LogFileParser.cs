﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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
		private readonly bool _useSmartParsing;

		/// <summary>
		/// Number of items to parse before calling repository.Save(), and thus GC'ing the list of entries.
		/// </summary>
		public int MaxEntriesBeforeSave { get; set; }

		public LogFileParser(ILogEntryRepository repository, bool useSmartParsing)
		{
			_repository = repository;
			_useSmartParsing = useSmartParsing;
			MaxEntriesBeforeSave = 25;
		}

		private void LogLine(string format, params object[] args)
		{
			// TODO: add logger
			System.Console.WriteLine(format, args);
		}

		public void ParseAndSave(ServerLogFileContainer container)
		{
			if (!container.AppLogFiles.Any())
				return;

			foreach (AppLogFiles appLogFile in container.AppLogFiles)
			{
				DateTime lastEntryDate = DateTime.MinValue;
				LastLogEntryInfo lastLogEntryInfo = null;

				if (_useSmartParsing)
				{
					lastLogEntryInfo = _repository.GetLastEntryInfo(container.Environment.Name, container.Server.Name, appLogFile.Appname);
				}

				if (lastLogEntryInfo != null)
				{
					lastEntryDate = lastLogEntryInfo.DateTime;
					LogLine("- Using smart update. Last entry for {0}/{1}/{2} was {3}", container.Environment.Name, container.Server.Name, appLogFile.Appname, lastEntryDate);
				}
				else
				{
					LogLine("- No latest date found for {0} or smart update is off.", appLogFile.Appname);
				}

				_entryDates = new ConcurrentBag<DateTime>();
				Parallel.ForEach(appLogFile.LogfilePaths, (filePath) =>
				{
					LogLine("Parsing {0} ({1})", filePath, ByteSize.FromBytes(new FileInfo(filePath).Length).ToString());

					if (_useSmartParsing)
					{
						// Check #1 - ignore files that are older than the most recent log entry
						FileInfo info = new FileInfo(filePath);
						if (info.LastWriteTimeUtc > lastEntryDate)
						{
							ParseAndSaveSingleLogFile(container.Environment.Name, container.Server.Name, appLogFile.Appname, filePath, lastLogEntryInfo);
						}
						else
						{
							LogLine("Ignoring {0} as it's older than {1} (the last log entry)", filePath, lastEntryDate);
						}
					}
					else
					{
						ParseAndSaveSingleLogFile(container.Environment.Name, container.Server.Name, appLogFile.Appname, filePath, null);
					}
				});


				LastLogEntryInfo cachedEntryInfo = new LastLogEntryInfo()
				{
					Id = LastLogEntryInfo.GenerateId(container.Environment.Name, container.Server.Name, appLogFile.Appname),
					DateTime = _entryDates.OrderByDescending(x => x.ToUniversalTime()).FirstOrDefault()
				};

				_repository.SaveLastEntry(cachedEntryInfo);
			}
		}

		private ConcurrentBag<DateTime> _entryDates;

		private void ParseAndSaveSingleLogFile(string environment, string server, string appName, string filePath, LastLogEntryInfo lastLogEntryInfo)
		{
			var list = new List<LogEntry>();
			int smartSkipCount = 0;

			var stringBuilder = new StringBuilder();
			using (var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
			{
				//  Read lines of text in, until we find an "|ERROR" as the delimiter and parse everything up to that line.
				int lineCount = 0;
				LogEntry logEntry = null;
				DateTime lastDateTime = DateTime.MinValue;
                while (!streamReader.EndOfStream)
				{
					string currentLine = streamReader.ReadLine();

					if (!string.IsNullOrEmpty(currentLine) && currentLine.Contains("|ERROR") && lineCount > 0)
					{
						logEntry = ParseLogEntry(environment, server, appName, stringBuilder.ToString());
						if (logEntry != null)
						{
							if (_useSmartParsing && lastLogEntryInfo != null)
							{
								// Check #2 - ignore entries that older than the last log entry date
								if (logEntry.DateTime > lastLogEntryInfo.DateTime)
								{
									list.Add(logEntry);
									lastDateTime = new DateTime(logEntry.DateTime.Ticks);
								}
							}
							else
							{
								list.Add(logEntry);
								lastDateTime = new DateTime(logEntry.DateTime.Ticks);
							}
						}

						lineCount = 0;
						stringBuilder = new StringBuilder();

						if (list.Count >= MaxEntriesBeforeSave)
						{
							LogLine("- Saving {0} items from {1}{2}", 
										list.Count, 
										filePath, 
										_useSmartParsing ? " (smart update skipped " + smartSkipCount+ " entries)" : "");

							_repository.BulkSave(list);

							list = new List<LogEntry>();
							GC.Collect();
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

				_entryDates.Add(lastDateTime);
            }
		}

		private LogEntry ParseLogEntry(string environment, string server, string appName, string contents)
		{
			MatchCollection matches = _entryRegex.Matches(contents);
			LogEntry entry = null;

			if (matches.Count > 0)
			{
				try
				{
					Match match = matches[0];

					entry = new LogEntry();
					entry.Id = Guid.NewGuid();
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

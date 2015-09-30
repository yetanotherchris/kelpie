using System;
using System.Collections.Generic;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;

namespace Kelpie.Tests
{
	public class RepositoryMock : ILogEntryRepository
	{
		public List<LogEntry> LogEntries { get; set; }

		public RepositoryMock()
		{
			LogEntries = new List<LogEntry>();
        }

		public void Save(LogEntry entry)
		{
			LogEntries.Add(entry);
        }

		public void BulkSave(IEnumerable<LogEntry> entries)
		{
			LogEntries.AddRange(entries);
		}

		public void DeleteAll()
		{
			LogEntries = new List<LogEntry>();
        }

		public IEnumerable<LogEntry> GetEntriesForApp(string logApplication)
		{
			yield break;
		}

		public IEnumerable<LogEntry> GetEntriesToday(string applicationName)
		{
			yield break;
		}

		public IEnumerable<LogEntry> GetEntriesThisWeek(string logApplication)
		{
			yield break;
		}

	    public IEnumerable<LogEntry> GetFilterEntriesForApp(LogEntryFilter filter)
	    {
            yield break;
        }

	    public LogEntry GetEntry(Guid id)
		{
			return null;
		}
	}
}
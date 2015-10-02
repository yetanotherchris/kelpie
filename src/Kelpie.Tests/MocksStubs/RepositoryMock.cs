using System;
using System.Collections.Generic;
using System.Linq;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;

namespace Kelpie.Tests.MocksStubs
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

		public IEnumerable<LogEntry> GetEntriesForApp(string environment, string applicationName)
		{
			return null;
		}

		public IEnumerable<LogEntry> GetEntriesToday(string environment, string applicationName)
		{
			return null;
		}

		public IEnumerable<LogEntry> GetEntriesThisWeek(string environment, string applicationName)
		{
			return null;
		}

		public IEnumerable<IGrouping<string, LogEntry>> GetEntriesThisWeekGroupedByException(string environment, string applicationName)
		{
			return null;
		}

		public IEnumerable<LogEntry> FindByExceptionType(string environment, string applicationName, string exceptionType)
		{
			return null;
		}

	    public IEnumerable<LogEntry> GetFilterEntriesForApp(LogEntryFilter filter)
	    {
	        return null;
	    }

	    public LogEntry GetEntry(Guid id)
		{
			return null;
		}
	}
}
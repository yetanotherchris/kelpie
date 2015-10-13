using System;
using System.Collections.Generic;
using System.Linq;
using Kelpie.Core.Domain;

namespace Kelpie.Core.Repository
{
	public interface ILogEntryRepository
	{
		void Save(LogEntry entry);
		void BulkSave(IEnumerable<LogEntry> entries);
		void DeleteAll();

		int Count();
		IEnumerable<LogEntry> GetAllEntries(int index, int rowCount);
		IEnumerable<LogEntry> GetEntriesForApp(string environment, string applicationName);
		IEnumerable<LogEntry> GetEntriesToday(string environment, string applicationName);
		IEnumerable<LogEntry> GetEntriesThisWeek(string environment, string applicationName);
		IEnumerable<LogEntry> GetEntriesSince(string environment, string applicationName, int numberOfDays);
        IEnumerable<IGrouping<string, LogEntry>> GetEntriesThisWeekGroupedByException(string environment,string applicationName);
		IEnumerable<LogEntry> FindByExceptionType(string environment, string applicationName, string exceptionType);

        IEnumerable<LogEntry> GetFilterEntriesForApp(LogEntryFilter filter);
        LogEntry GetEntry(Guid id);
		IEnumerable<LogEntry> Search(string environment, string applicationName, string query);

		LastLogEntryInfo GetLastEntryInfo(string environment, string server, string appName);
		void SaveLastEntry(LastLogEntryInfo lastLogEntryInfo);
	}
}
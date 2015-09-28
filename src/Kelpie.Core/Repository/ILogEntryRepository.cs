using System;
using System.Collections.Generic;
using Kelpie.Core.Domain;

namespace Kelpie.Core.Repository
{
	public interface ILogEntryRepository
	{
		void Save(LogEntry entry);
		void BulkSave(IEnumerable<LogEntry> entries);
		void DeleteAll();
		IEnumerable<LogEntry> GetEntriesForApp(string logApplication);
		IEnumerable<LogEntry> GetEntriesToday(string applicationName);
		IEnumerable<LogEntry> GetEntriesThisWeek(string logApplication);
		LogEntry GetEntry(Guid id);
	}
}
﻿using System;
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
		IEnumerable<LogEntry> GetEntriesForApp(string environment, string applicationName);
		IEnumerable<LogEntry> GetEntriesToday(string environment, string applicationName);
		IEnumerable<LogEntry> GetEntriesThisWeek(string environment, string applicationName);
		IEnumerable<IGrouping<string, LogEntry>> GetEntriesThisWeekGroupedByException(string environment,string applicationName);
		IEnumerable<LogEntry> FindByExceptionType(string environment, string applicationName, string exceptionType);
		LogEntry GetEntry(Guid id);
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Kelpie.Core.Domain;
using Raven.Client;
using Raven.Client.Document;

namespace Kelpie.Core.Repository
{
	public class LogEntryRepository : IDisposable
	{
		private readonly IDocumentSession _documentSession;

		public LogEntryRepository(IDocumentStore documentStore)
		{
			_documentSession = documentStore.OpenSession();
		}

		public void Save(LogEntry entry)
		{
			_documentSession.Store(entry);
			_documentSession.SaveChanges();
		}

		public void BulkSave(IEnumerable<LogEntry> entries)
		{
			foreach (LogEntry entry in entries)
			{
				_documentSession.Store(entry);
			}

			_documentSession.SaveChanges();
		}

		public void Delete(int id)
		{
			_documentSession.Delete<LogEntry>(id);
			_documentSession.SaveChanges();
		}

		public void DeleteAll()
		{
			_documentSession.Advanced.MaxNumberOfRequestsPerSession = 100;
			var objects = _documentSession.Query<LogEntry>().ToList();
			while (objects.Any())
			{
				foreach (var obj in objects)
				{
					_documentSession.Delete(obj);
				}

				_documentSession.SaveChanges();
				objects = _documentSession.Query<LogEntry>().ToList();
			}
		}

		public IEnumerable<LogEntry> GetEntriesForApp(string logApplication)
		{
			return _documentSession.Query<LogEntry>().Where(x => x.ApplicationName.Equals(logApplication));
		}

		public IEnumerable<LogEntry> GetEntriesToday(string applicationName)
		{
			var items = _documentSession.Query<LogEntry>().Where(x => x.ApplicationName.Equals(applicationName) && x.DateTime > DateTime.Today);

			return items.ToList().OrderByDescending(x => x.DateTime);
		}

		public IEnumerable<LogEntry> GetEntriesThisWeek(string logApplication)
		{
			var items =
				_documentSession.Query<LogEntry>()
					.Where(x => x.ApplicationName.Equals(logApplication) && x.DateTime > DateTime.Today.AddDays(-7));

			return items.ToList().OrderByDescending(x => x.DateTime);
		}

		public void Dispose()
		{
			_documentSession.SaveChanges();
			_documentSession?.Dispose();
		}

		public LogEntry GetEntry(DateTime dateTime, string applicationName)
		{
			return _documentSession.Query<LogEntry>().FirstOrDefault(x => x.ApplicationName.Equals(applicationName) && x.DateTime == dateTime);
		}
	}
}

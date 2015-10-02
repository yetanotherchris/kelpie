using System;
using System.Collections.Generic;
using System.Linq;
using Kelpie.Core.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Kelpie.Core.Repository
{
    public class LogEntryRepository : ILogEntryRepository
    {
        private readonly MongoClient _mongoClient;
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<LogEntry> _collection;

        public LogEntryRepository(MongoClient mongoClient, IConfiguration configuration, string databaseName = "Kelpie")
        {
            _mongoClient = mongoClient;
            _configuration = configuration;
            _database = _mongoClient.GetDatabase(databaseName);
            _collection = _database.GetCollection<LogEntry>("LogEntry");
        }

        public void Save(LogEntry entry)
        {
            _collection.InsertOneAsync(entry);
        }

        public void BulkSave(IEnumerable<LogEntry> entries)
        {
            _collection.InsertManyAsync(entries);
        }

        public void DeleteAll()
        {
            // This should be used for imports, to ensure the data directory doesn't bloat.
            DropDatabase("Kelpie");
        }

        public async void DropDatabase(string databaseName = "Kelpie")
        {
            await _mongoClient.DropDatabaseAsync(databaseName);
        }

        public void DeleteCollection(string collectionName = "Kelpie")
        {
            _database.DropCollectionAsync(collectionName);
        }

        public IEnumerable<LogEntry> GetEntriesForApp(string environment, string applicationName)
        {
            return _collection.AsQueryable<LogEntry>().Where(x => x.Environment.Equals(environment)
                                                                && x.ApplicationName.Equals(applicationName));
        }

        public IEnumerable<LogEntry> GetEntriesToday(string environment, string applicationName)
        {
            var items = _collection.AsQueryable<LogEntry>().Where(x => x.Environment.Equals(environment)
                                                                        && x.ApplicationName.Equals(applicationName)
                                                                        && x.DateTime > DateTime.Today);

            return items.ToList().OrderByDescending(x => x.DateTime);
        }

        public IEnumerable<LogEntry> GetEntriesThisWeek(string environment, string applicationName)
        {
            var items =
                _collection.AsQueryable<LogEntry>()
                    .Where(x => x.Environment.Equals(environment)
                                && x.ApplicationName.Equals(applicationName)
                                && x.DateTime > DateTime.Today.AddDays(-7));

            return items.ToList().OrderByDescending(x => x.DateTime);
        }

        public IEnumerable<IGrouping<string, LogEntry>> GetEntriesThisWeekGroupedByException(string environment, string applicationName)
        {
            var items =
                _collection.AsQueryable<LogEntry>()
                    .Where(x => x.Environment.Equals(environment)
                            && x.ApplicationName.Equals(applicationName)
                            && x.DateTime > DateTime.Today.AddDays(-7)
                            && !string.IsNullOrEmpty(x.ExceptionType));

            return items.ToList().GroupBy(x => x.ExceptionType).OrderByDescending(x => x.Count()); // make sure to call ToList, or the groupby fails
        }

        public IEnumerable<LogEntry> FindByExceptionType(string environment, string applicationName, string exceptionType)
        {
            var items =
                _collection.AsQueryable<LogEntry>()
                    .Where(x => x.Environment.Equals(environment)
                            && x.ApplicationName.Equals(applicationName)
                            && x.DateTime > DateTime.Today.AddDays(-_configuration.MaxAgeDays)
                            && x.ExceptionType == exceptionType);

            return items.ToList().OrderByDescending(x => x.DateTime);
        }

        public LogEntry GetEntry(Guid id)
        {
            return _collection.AsQueryable<LogEntry>().FirstOrDefault(x => x.Id == id);
        }



        public IEnumerable<LogEntry> GetFilterEntriesForApp(LogEntryFilter filter)
        {
            if (!filter.Page.HasValue)
                filter.Page = 1;
            if (!filter.Rows.HasValue)
                filter.Rows = 100;
            var query = _collection.AsQueryable<LogEntry>();

            if (filter.Start.HasValue)
            {
                query = query.Where(logEntry => logEntry.DateTime >= filter.Start.Value);
            }
            
            if (filter.End.HasValue)
            {
                query = query.Where(logEntry => logEntry.DateTime < filter.End.Value);
            }

            return query.Where(x => x.ApplicationName.Equals(filter.LogApplication))
                .Skip((filter.Page.Value - 1) * filter.Rows.Value)
                .Take(filter.Rows.Value);
        }
    }
}

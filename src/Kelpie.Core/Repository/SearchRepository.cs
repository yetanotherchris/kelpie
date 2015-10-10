using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kelpie.Core.Domain;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

namespace Kelpie.Core.Repository
{
	public class SearchRepository
	{
		private readonly IConfiguration _configuration;
		private static Version LUCENEVERSION = Version.LUCENE_30;
		public string IndexPath { get; set; }

		public SearchRepository(IConfiguration configuration)
		{
			_configuration = configuration;

			// TODO: resolve ~/
			IndexPath = @"d:\lucene";
		}

		/// <summary>
		/// This will delete any existing search index.
		/// </summary>
		/// <param name="repository"></param>
		public void CreateIndex(ILogEntryRepository repository)
		{
			System.Console.WriteLine("Updating search index");
            EnsureDirectoryExists();

			StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);

			// Clear the index
			using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
			{
				writer.Optimize();
			}

			FSDirectory fsDirectory = FSDirectory.Open(new DirectoryInfo(IndexPath));
			_progress = 0;

            try
			{
				// Chunk the items into groups of work
				int groupSize = 5000;
				int totalItems = repository.Count();

				int numOfGroups = totalItems/groupSize;
				var ranges = new List<int>();
				for (int i = 0; i < numOfGroups; i++)
				{
					ranges.Add(i * groupSize);
				}

				using (IndexWriter writer = new IndexWriter(fsDirectory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					Parallel.ForEach(ranges, (startIndex) =>
					{
						IEnumerable<LogEntry> items = repository.GetAllEntries(startIndex, groupSize);
					
						foreach (LogEntry logEntry in items)
						{
							Document document = CreateDocument(logEntry);
							writer.AddDocument(document);
						}

						Interlocked.Increment(ref _progress);
						decimal percent = ((decimal) _progress/numOfGroups);
                        System.Console.Write("\rLucene - index items: {0:P}\t{1}-{2} {3}", percent, startIndex, startIndex + groupSize, new string(' ', 10));

					});

					writer.Optimize();
				}

				// Do the remainder

			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while creating the search index");
			}
		}

		private int _progress;

		public void Add(LogEntry logEntry)
		{
			try
			{
				EnsureDirectoryExists();

				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					Document document = CreateDocument(logEntry);

					writer.AddDocument(document);
					writer.Optimize();
				}
			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while adding entry '{0}' to the search index", logEntry.DateTime);
			}
		}

		private Document CreateDocument(LogEntry logEntry)
		{
			Document document = new Document();

			document.Add(new Field("id", logEntry.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("environment", logEntry.Environment, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("server", logEntry.Server, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("application", logEntry.ApplicationName, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("date", logEntry.DateTime.ToShortDateString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("exceptionMessage", logEntry.ExceptionMessage??"", Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("exceptionType", logEntry.ExceptionType ?? "", Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("message", logEntry.Message ?? "", Field.Store.NO, Field.Index.ANALYZED));

			return document;
		}

		public int Delete(LogEntry logEntry)
		{
			try
			{
				int count = 0;
				using (IndexReader reader = IndexReader.Open(FSDirectory.Open(new DirectoryInfo(IndexPath)), false))
				{
					count += reader.DeleteDocuments(new Term("id", logEntry.Id.ToString()));
				}

				return count;
			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while deleting entry '{0}' from the search index", logEntry.DateTime);
			}
		}

		public virtual void Update(LogEntry logEntry)
		{
			EnsureDirectoryExists();
			Delete(logEntry);
			Add(logEntry);
		}

		private void EnsureDirectoryExists()
		{
			try
			{
				if (!Directory.Exists(IndexPath))
					Directory.CreateDirectory(IndexPath);
			}
			catch (IOException ex)
			{
				throw new SearchException(ex, "An error occured while creating the search directory '{0}'", IndexPath);
			}
		}

		public IEnumerable<LogEntry> Search(string searchText)
		{
			List<LogEntry> list = new List<LogEntry>();

			if (string.IsNullOrWhiteSpace(searchText))
				return list;

			StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
			MultiFieldQueryParser parser = new MultiFieldQueryParser(LUCENEVERSION, new string[] { "exceptionMessage", "exceptionType" }, analyzer);

			Query query = null;
			try
			{
				query = parser.Parse(searchText);
			}
			catch (ParseException)
			{
				// Catch syntax errors in the search and remove them.
				searchText = QueryParser.Escape(searchText);
				query = parser.Parse(searchText);
			}

			if (query != null)
			{
				try
				{
					using (IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(IndexPath)), true))
					{
						TopDocs topDocs = searcher.Search(query, 50);

						foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs.OrderByDescending(x => x.Score))
						{
							Document document = searcher.Doc(scoreDoc.Doc);
							list.Add(DocumentToLogEntry(document));
						}
					}
				}
				catch (Exception ex)
				{
					throw new SearchException(ex, "An error occured while searching the index, try rebuilding the search index via the admin tools to fix this.");
				}
			}

			return list;
		}

		private LogEntry DocumentToLogEntry(Document document)
		{
			return new LogEntry()
			{
				Id               = Guid.Parse(document.GetField("id").StringValue),
				ApplicationName  = document.GetField("application").StringValue,
				Server           = document.GetField("server").StringValue,
				Environment      = document.GetField("environment").StringValue,
				DateTime         = DateTime.Parse(document.GetField("date").StringValue),
				ExceptionMessage = document.GetField("exceptionMessage").StringValue,
				ExceptionType    = document.GetField("exceptionType").StringValue,
				Message = ""
			};
		}
	}
}

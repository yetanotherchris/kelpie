using System;
using System.IO;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Database.Server;

namespace Kelpie.Core
{
	/// <summary>
	/// To manage RavenDB, go to http://localhost:8087/
	/// </summary>
	public class RavenDbServer
	{
		public static IDocumentStore DocumentStore = new EmbeddableDocumentStore
		{
			DefaultDatabase = "kelpie",
			DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "RavenDB"),
			UseEmbeddedHttpServer = true,
			Configuration = { Port = 411 }
		};

		public static void Start()
		{
			NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8087);
			DocumentStore.Initialize();
			//IndexCreation.CreateIndexes(typeof(LogEntry).Assembly, DocumentStore);
		}

		public static void Stop()
		{
			DocumentStore.Dispose();
		}
	}

	public class LogEntryIndex : AbstractIndexCreationTask<LogEntry>
	{
		public LogEntryIndex()
		{
			Index(x => x.ApplicationName, FieldIndexing.Default);
			Index(x => x.Server, FieldIndexing.Default);

			Map = entries => from e in entries select e.DateTime;
		}
	}
}
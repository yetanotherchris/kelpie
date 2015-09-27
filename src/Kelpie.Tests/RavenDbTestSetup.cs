using System.Linq;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Listeners;

namespace Kelpie.Tests
{
	public class RavenDbTestSetup
	{
		public static IDocumentStore DocumentStore { get; set; }

		static RavenDbTestSetup()
		{
			DocumentStore = new EmbeddableDocumentStore
			{
				DefaultDatabase = "kelpie-tests",
				RunInMemory = true,
			};

			DocumentStore.Initialize();
			DocumentStore.Listeners.RegisterListener(new NoStaleQueriesListener());
		}

		public static void ClearDocuments<T>()
		{
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var objects = session.Query<T>().ToList();
				while (objects.Any())
				{
					foreach (var obj in objects)
					{
						session.Delete(obj);
					}

					session.SaveChanges();
					objects = session.Query<T>().ToList();
				}
			}
		}

		// Force embedded database to catchup after saves - http://stackoverflow.com/questions/9181204/ravendb-how-to-flush
		public class NoStaleQueriesListener : IDocumentQueryListener
		{
			public void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
			{
				queryCustomization.WaitForNonStaleResults();
			}
		}
	}
}
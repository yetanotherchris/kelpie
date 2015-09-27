using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kelpie.Core;

namespace Kelpie.Web.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult ByApplication(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			using (var respository = new LogEntryRepository(RavenDbServer.DocumentStore))
			{
				var entries = respository.GetEntriesThisWeek(applicationName);
                return View(entries);
			}
		}

		public ActionResult LoadMessage(long ticks, string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			using (var respository = new LogEntryRepository(RavenDbServer.DocumentStore))
			{
				LogEntry entry = respository.GetEntry(new DateTime(ticks), applicationName);
				return Content(entry.Message);
			}
		}

		public ActionResult Refresh()
		{
			using (var respository = new LogEntryRepository(RavenDbServer.DocumentStore))
			{
				respository.DeleteAll();

				foreach (string directory in Directory.EnumerateDirectories(@"D:\ErrorLogs"))
				{
					string applicationName = Path.GetFileName(directory);
					string[] logFiles = Directory.GetFiles(directory, "*.log");
					foreach (string file in logFiles)
					{
						var parser = new LogFileParser(file, "localhost", applicationName);
						IEnumerable<LogEntry> entries = parser.Read();
						respository.BulkSave(entries);
					}
				}
			}

			return Content("Finished");
		}
	}
}
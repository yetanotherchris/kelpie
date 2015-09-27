using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kelpie.Core;
using Newtonsoft.Json;

namespace Kelpie.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly LogEntryRepository _repository;

		public HomeController()
		{
			_repository = new LogEntryRepository(RavenDbServer.DocumentStore);
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Today(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			using (_repository)
			{
				var entries = _repository.GetEntriesToday(applicationName);
				return View(entries);
			}
		}

		public ActionResult ThisWeek(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			using (_repository)
			{
				var entries = _repository.GetEntriesThisWeek(applicationName);
				return View(entries);
			}
		}

		public ActionResult LoadMessage(long ticks, string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			using (_repository)
			{
				LogEntry entry = _repository.GetEntry(new DateTime(ticks), applicationName);
				return Content(entry.Message);
			}
		}

		public ActionResult Refresh()
		{
			using (_repository)
			{
				_repository.DeleteAll();

				foreach (string directory in Directory.EnumerateDirectories(@"D:\ErrorLogs"))
				{
					string applicationName = Path.GetFileName(directory);
					string[] logFiles = Directory.GetFiles(directory, "*.log");
					foreach (string file in logFiles)
					{
						var parser = new LogFileParser(file, "localhost", applicationName);
						IEnumerable<LogEntry> entries = parser.Read();
						_repository.BulkSave(entries);
					}
				}
			}

			return Content("Finished");
		}

		public ActionResult GetApplications()
		{
			var configuration = new Configuration();
			return Content(JsonConvert.SerializeObject(configuration.Applications));
		}
	}
}
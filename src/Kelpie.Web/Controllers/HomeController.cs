using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Kelpie.Core.IO;
using Kelpie.Core.Repository;
using Newtonsoft.Json;

namespace Kelpie.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly LogEntryRepository _repository;
		private readonly Configuration _configuration;

		public HomeController()
		{
			_configuration = new Configuration();
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
			var entries = _repository.GetEntriesThisWeek(applicationName);
			return View(entries);
		}

		public ActionResult LoadMessage(long ticks, string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			LogEntry entry = _repository.GetEntry(new DateTime(ticks), applicationName);
			return Content(entry.Message.Trim());
		}

		public ActionResult Refresh()
		{
			// This should be in a service
			_repository.DeleteAll();

			var logReader = new FileSystemLogReader(_configuration);
			var allEntries = logReader.ScanLogDirectories();
			_repository.BulkSave(allEntries);

			return Content("Finished");
		}

		/// <summary>
		/// Use for the index page's autocomplete.
		/// </summary>
		public ActionResult GetApplications()
		{
			return Content(JsonConvert.SerializeObject(_configuration.Applications));
		}

		protected override void Dispose(bool disposing)
		{
			_repository.Dispose();
			base.Dispose(disposing);
		}
	}
}
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
using MongoDB.Driver;
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
			_repository = new LogEntryRepository(new MongoClient());
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Today(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			var entries = _repository.GetEntriesToday(applicationName);
			return View(entries);
		}

		public ActionResult ThisWeek(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			var entries = _repository.GetEntriesThisWeek(applicationName);
			return View(entries);
		}

		public ActionResult AllExceptionTypes(string applicationName)
		{
			ViewBag.ApplicationName = applicationName;
			var entries = _repository.GetEntriesThisWeekGroupedByException(applicationName);
			return View(entries);
		}

		public ActionResult ExceptionType(string applicationName, string exceptionType)
		{
			ViewBag.ApplicationName = applicationName;
			ViewBag.ExceptionType = exceptionType;

			var entries = _repository.FindByExceptionType(applicationName, exceptionType);
			return View(entries);
		}

		public ActionResult LoadMessage(Guid id)
		{
			LogEntry entry = _repository.GetEntry(id);
			return Content(entry.Message.Trim());
		}

		/// <summary>
		/// Use for the index page's autocomplete.
		/// </summary>
		public ActionResult GetApplications()
		{
			return Content(JsonConvert.SerializeObject(_configuration.Applications));
		}
	}
}
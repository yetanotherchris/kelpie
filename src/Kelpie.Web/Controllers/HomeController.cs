using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Kelpie.Core.IO;
using Kelpie.Core.Repository;
using Kelpie.Web.Models;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kelpie.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly LogEntryRepository _repository;
		private readonly IConfiguration _configuration;

		public HomeController()
		{
			_configuration = Configuration.Read();
			_repository = new LogEntryRepository(new MongoClient(), _configuration);
		}

		public ActionResult Index()
		{
			if (MemoryCache.Default.Contains("dashboard-list"))
			{
				var list = MemoryCache.Default.Get("dashboard-list") as List<HomepageModel>;
				return View(list);
			}
			else
			{
				HostingEnvironment.QueueBackgroundWorkItem((token) =>
				{
					List<HomepageModel> list = GetDashboardData();
					MemoryCache.Default.Add("dashboard-list", list, DateTimeOffset.UtcNow.AddHours(12));
				});

				return View("CrunchingData");
			}
		}

		private List<HomepageModel> GetDashboardData()
		{
			var list = new List<HomepageModel>();

			foreach (string applicationName in _configuration.Applications.OrderBy(x => x))
			{
				List<LogEntry> entries = _repository.GetEntriesThisWeek(applicationName).ToList();
				var topException = entries.GroupBy(x => x.ExceptionType)
					//.Where(x => !string.IsNullOrWhiteSpace(x.Key))
					.OrderByDescending(x => x.Count());

				string exceptionType = "";
				var topItem = topException.FirstOrDefault();
				if (topItem != null)
					exceptionType = topItem.Key;

				var model = new HomepageModel()
				{
					Application = applicationName,
					TopExceptionType = exceptionType,
					ErrorCount = entries.Count,
					ErrorCountPerServer = entries.Count/_configuration.Servers.Count(),
					ServerCount = _configuration.Servers.Count()
				};

				list.Add(model);
			}

			return list;
		}

		public ActionResult GetCacheDataStatus()
		{
			bool hasData = MemoryCache.Default.Get("dashboard-list") != null;
			return Json(hasData, JsonRequestBehavior.AllowGet);
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
			return Content(HttpUtility.HtmlEncode(entry.Message.Trim()));
		}
	}
}
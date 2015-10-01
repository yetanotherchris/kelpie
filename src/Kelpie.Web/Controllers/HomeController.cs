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
using Environment = Kelpie.Core.Domain.Environment;

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
            Environment selectedEnvironment = GetSelectedEnvironment();
			Response.Cookies.Add(new HttpCookie("environmentName", selectedEnvironment.Name));

			var homepageModel = new HomepageViewModel();
			homepageModel.Environments = _configuration.Environments.Select(x => x.Name);
			homepageModel.CurrentEnvironment = selectedEnvironment.Name;

			string cacheKey = GetCacheKey();

			if (MemoryCache.Default.Contains(cacheKey))
			{
				List<ServerViewModel> list = MemoryCache.Default.Get(cacheKey) as List<ServerViewModel>;
				homepageModel.ServerModels = list;

				return View(homepageModel);
			}
			else
			{
				HostingEnvironment.QueueBackgroundWorkItem((token) =>
				{
					List<ServerViewModel> list = GetDashboardData(selectedEnvironment);
					homepageModel.ServerModels = list;
					MemoryCache.Default.Add(cacheKey, list, DateTimeOffset.MaxValue);
				});

				return View("CrunchingData", homepageModel);
			}
		}

		private string GetCacheKey()
		{
			return string.Format("kelpie.dashboard.{0}", GetSelectedEnvironment().Name);
		}

		private Environment GetSelectedEnvironment()
		{
			string environmentName = Request.QueryString["environmentName"];
				
			if (string.IsNullOrEmpty(environmentName))
				environmentName = Request.Cookies["environmentName"]?.Value;

            Environment selectedEnvironment = null;
			if (!string.IsNullOrEmpty(environmentName))
				selectedEnvironment = _configuration.Environments.First(x => x.Name.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase));

			if (selectedEnvironment == null)
				selectedEnvironment = _configuration.Environments.First();

			return selectedEnvironment;
		}

		private List<ServerViewModel> GetDashboardData(Environment environment)
		{
			var list = new List<ServerViewModel>();

			foreach (string applicationName in _configuration.Applications.OrderBy(x => x))
			{
				List<LogEntry> entries = _repository.GetEntriesThisWeek(environment.Name, applicationName).ToList();
				var topException = entries.GroupBy(x => x.ExceptionType)
					.OrderByDescending(x => x.Count());

				string exceptionType = "";
				var topItem = topException.FirstOrDefault();
				if (topItem != null)
					exceptionType = topItem.Key;

				var model = new ServerViewModel()
				{
					Application = applicationName,
					TopExceptionType = exceptionType,
					ErrorCount = entries.Count,
					ErrorCountPerServer = entries.Count/ environment.Servers.Count(),
					ServerCount = environment.Servers.Count()
				};

				list.Add(model);
			}

			return list;
		}

		public ActionResult GetCacheDataStatus()
		{
			string cacheKey = GetCacheKey();
            bool hasData = MemoryCache.Default.Get(cacheKey) != null;
			return Json(hasData, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Today(string applicationName)
		{
			Environment currentEnvironment = GetSelectedEnvironment();

			ViewBag.EnvironmentName = currentEnvironment.Name;
			ViewBag.ApplicationName = applicationName;

			var entries = _repository.GetEntriesToday(currentEnvironment.Name, applicationName);
			return View(entries);
		}

		public ActionResult ThisWeek(string applicationName)
		{
			Environment currentEnvironment = GetSelectedEnvironment();

			ViewBag.EnvironmentName = currentEnvironment.Name;
			ViewBag.ApplicationName = applicationName;

			var entries = _repository.GetEntriesThisWeek(currentEnvironment.Name, applicationName);
			return View(entries);
		}

		public ActionResult AllExceptionTypes(string applicationName)
		{
			Environment currentEnvironment = GetSelectedEnvironment();

			ViewBag.EnvironmentName = currentEnvironment.Name;
			ViewBag.ApplicationName = applicationName;

			var entries = _repository.GetEntriesThisWeekGroupedByException(currentEnvironment.Name, applicationName);
			return View(entries);
		}

		public ActionResult ExceptionType(string applicationName, string exceptionType)
		{
			Environment currentEnvironment = GetSelectedEnvironment();

			ViewBag.EnvironmentName = currentEnvironment.Name;
			ViewBag.ApplicationName = applicationName;
			ViewBag.ExceptionType = exceptionType;

			var entries = _repository.FindByExceptionType(currentEnvironment.Name, applicationName, exceptionType);
			return View(entries);
		}

		public ActionResult LoadMessage(Guid id)
		{
			LogEntry entry = _repository.GetEntry(id);
			return Content(HttpUtility.HtmlEncode(entry.Message.Trim()));
		}

		public ActionResult ClearCache()
		{
			foreach (KeyValuePair<string, object> keyValuePair in MemoryCache.Default)
			{
				if (keyValuePair.Key.StartsWith("kelpie."))
					MemoryCache.Default.Remove(keyValuePair.Key);
			}

			return Content("All cache keys for Kelpie cleared.");
		}
	}
}
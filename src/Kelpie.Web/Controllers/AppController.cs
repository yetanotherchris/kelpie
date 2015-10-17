using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Kelpie.Core;
using Kelpie.Core.Domain;
using Kelpie.Core.Repository;
using Kelpie.Web.Models;
using MongoDB.Driver;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Web.Controllers
{
    public class AppController : Controller
    {
        private readonly LogEntryRepository _repository;
        private readonly IConfiguration _configuration;

        public AppController()
        {
            _configuration = Configuration.Read();
            _repository = new LogEntryRepository(new MongoClient(), _configuration);
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

        public ActionResult Today(string applicationName, int? page, int? rows)
        {
            Environment currentEnvironment = GetSelectedEnvironment();

            ViewBag.EnvironmentName = currentEnvironment.Name;
            ViewBag.ApplicationName = applicationName;

            var entries = _repository.GetFilterEntriesForApp(new LogEntryFilter()
            {
                Start = DateTime.Now.Date,
                End = DateTime.Now.Date.AddHours(24),
                LogApplication = applicationName,
                Environment = currentEnvironment.Name,
                Page = page,
                Rows = rows
            });
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

		public ActionResult Search(string applicationName, string q)
        {
			Environment currentEnvironment = GetSelectedEnvironment();

			ViewBag.EnvironmentName = currentEnvironment.Name;
			ViewBag.ApplicationName = applicationName;
			ViewBag.SearchQuery = q;

			IEnumerable<LogEntry> results = new LogEntry[0];

			if (_configuration.IsLuceneEnabled)
			{
				var searchRepository = new SearchRepository(_configuration);
				string query = searchRepository.CreateLuceneSearchSyntax(applicationName, currentEnvironment.Name, q);
				results = searchRepository.Search(query);
			}
			else
			{
				// Use MongoDB's slower search.
				results = _repository.Search(currentEnvironment.Name, applicationName, q);
			}

			return View(results);
        }
    }
}
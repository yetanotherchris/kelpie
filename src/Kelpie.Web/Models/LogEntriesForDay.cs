using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kelpie.Core;
using Kelpie.Core.Domain;

namespace Kelpie.Web.Models
{
	public class LogEntriesForDay
	{
		public string DayOfWeek { get; set; }
		public IEnumerable<LogEntry> LogEntries { get; set; }

		public static LogEntriesForDay Create(IEnumerable<LogEntry> entries, DateTime dateTime)
		{
			string dayOfWeek = dateTime.ToString("dddd dd");
			switch (dateTime.Day)
			{
				case 1:
				case 21:
				case 31:
					dayOfWeek += "st";
					break;
				case 2:
				case 22:
					dayOfWeek += "nd";
					break;
				case 3:
				case 23:
					dayOfWeek += "rd";
					break;
				default:
					dayOfWeek += "th";
					break;
			}

			if (dateTime.Date == DateTime.Today.Date)
				dayOfWeek = "Today";
			else if (dateTime.Date == DateTime.Today.AddDays(-1).Date)
				dayOfWeek = "Yesterday";

			return new LogEntriesForDay()
			{
				DayOfWeek = dayOfWeek,
				LogEntries = entries.Where(x => x.DateTime.DayOfWeek == dateTime.DayOfWeek)
			};
		}
	}
}
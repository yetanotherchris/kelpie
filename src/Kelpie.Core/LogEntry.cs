using System;
using Raven.Imports.Newtonsoft.Json;

namespace Kelpie.Core
{
	public class LogEntry
	{
		public DateTime DateTime { get; set; }
		public string Level { get; set; }
		public string Source { get; set; }
		public string Message { get; set; }
		public string ExceptionType { get; set; }
		public string ExceptionMessage { get; set; }

		public string ApplicationName { get; set; }
		public string Server { get; set; }

		[JsonIgnore]
		public long CssId
		{
			get { return DateTime.Ticks; }
		}

		[JsonIgnore]
		public string TruncatedMessage
		{
			get
			{
				if (Message.Length > 30)
					return Message.Substring(0, 30);

				return Message;
			}
		}
	}
}
using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelpie.Core.Domain
{
	public class LogEntry
	{
		public Guid Id { get; set; }
		public DateTime DateTime { get; set; }
		public string Level { get; set; }
		public string Source { get; set; }
		public string Message { get; set; }
		public string ExceptionType { get; set; }
		public string ExceptionMessage { get; set; }

		public string ApplicationName { get; set; }
		public string Server { get; set; }

		[BsonIgnore]
		public string TruncatedMessage
		{
			get
			{
				if (Message.Length > 100)
					return Message.Substring(0, 100) + "...";

				return Message;
			}
		}
	}
}
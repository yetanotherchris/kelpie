using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kelpie.Core
{
    public class LogFileParser
	{
	    private readonly string _filePath;
	    private readonly string _serverName;
	    private readonly string _application;

	    private static readonly Regex _entryRegex = new Regex(@"(?<date>\d{4}-\d{2}-\d{2}\s{1}\d{2}:\d{2}:\d{2}\.\d{4})\|ERROR\|(?<source>\w+?)\|(?<message>.*?)", 
																RegexOptions.Singleline | RegexOptions.Compiled);

		public LogFileParser(string filePath, string serverName, string application)
		{
			_filePath = filePath;
			_serverName = serverName;
			_application = application;
		}

	    public IEnumerable<LogEntry> Read()
	    {
			// No need to stream in the logs, as they are never more than 10mb thanks to the NLog rotate settings.
			var list = new List<LogEntry>();
		
		    string contents = File.ReadAllText(_filePath);		
			MatchCollection matches = _entryRegex.Matches(contents);

			int previousIndex = 0;
			string previousMessage = "";
			var entry = new LogEntry();

			foreach (Match match in matches)
			{
				if (string.IsNullOrEmpty(entry.Message))
				{
					if (previousIndex > 0)
					{
						// Parse the message using the last capture's |source| index, and beginning of this capture
						int end = match.Groups["date"].Index;

						if (previousIndex > 0)
							previousMessage = contents.Substring(previousIndex +1, end - (previousIndex +1));

						entry.Message = previousMessage;
						FillExceptionType(entry);
						list.Add(entry);

						entry = new LogEntry();
					}

					entry.DateTime = DateTime.Parse(match.Groups["date"].Value);
					entry.Source = match.Groups["source"].Value;
					entry.Level = "Error";
					entry.Server = _serverName;
					entry.ApplicationName = _application;

					previousIndex = match.Groups["source"].Index + match.Groups["source"].Length;
				}
			}

		    return list;
	    }

	    private void FillExceptionType(LogEntry entry)
	    {
		    if (string.IsNullOrEmpty(entry.Message))
			    return;

		    string message = entry.Message;

			int start = message.LastIndexOf("Exception Type: ");
		    if (start > -1)
		    {
			    int end = message.IndexOf(Environment.NewLine, start);
			    if (end > -1)
			    {
				    int typeStart = start + "Exception Type: ".Length;
				    string exceptionType = message.Substring(typeStart, end - typeStart);
				    entry.ExceptionType = exceptionType;

				    int messageStart = message.IndexOf("Message: ", start);
				    if (messageStart > -1)
				    {
					    int messageEnd = message.IndexOf(Environment.NewLine, messageStart);
					    if (messageEnd > -1)
					    {
						    messageStart = messageStart + "Message: ".Length;
						    string exceptionMessage = message.Substring(messageStart, messageEnd - messageStart);
						    entry.ExceptionMessage = exceptionMessage;
					    }
				    }
			    }
		    }
	    }
	}
}

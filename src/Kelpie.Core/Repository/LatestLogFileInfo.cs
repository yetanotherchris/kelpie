using System;

namespace Kelpie.Core.Repository
{
	public class LatestLogFileInfo
	{
		public string Id { get; set; }
		public DateTime DateTime { get; set; }

		public static string GenerateId(string environment, string server, string appname)
		{
			return string.Format("{0}/{1}/{2}", environment, server, appname);
		}
	}
}
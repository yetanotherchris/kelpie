using System;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Kelpie.Core.Console
{
	internal class Options
	{
		[Option('c', "copyfiles", Required = false, HelpText = "If true, copies all log files to the temp directory (%TEMP% or $env:TEMP in powershell), inside the 'kelpie' directory.")]
		public bool CopyFiles { get; set; }

		[Option('w', "wipedata", Required = false, HelpText = "If true, wipes all data from the MongoDB database before importing. Use this for performing a fresh import.")]
		public bool WipeData { get; set; }

		[Option('i', "import", Required = false, HelpText = "If true, imports all data into the database (when false, this is usually used in conjunction with --copyfiles).")]
		public bool Import { get; set; }

		[Option('u', "smartupdate", Required = false, HelpText = "Only imports log entries in each application that are newer than any existing ones. If this is not set and you do not specify --wipedata, you will get duplicate log file entries.")]
		public bool SmartUpdate { get; set; }

		[Option('s', "server", Required = false, HelpText = "Specify a single server to import log files from, which should match a server name in the kelpie config file.")]
		public string Server { get; set; }

		[Option('e', "environment", Required = false, HelpText = "Specify a single environment to import log files from, which should match an environment name in the kelpie config file.")]
		public string Environment { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			var help = new HelpText
			{
				Heading = new HeadingInfo("Kelpie import tool", Assembly.GetExecutingAssembly().GetName().Version.ToString()),
				Copyright = new CopyrightInfo("C.Small", DateTime.Now.Year),
				AdditionalNewLineAfterOption = true,
				AddDashesToOption = true
			};

			help.AddOptions(this);

			string exampleUsage = string.Format("{0}Example usage:{0} kelpie.exe --copyfiles --import{0}", System.Environment.NewLine);

			return help + exampleUsage;
		}
	}
}

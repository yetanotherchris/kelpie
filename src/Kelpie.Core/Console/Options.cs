using System;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Kelpie.Core.Console
{
	internal class Options
	{
		[Option('f', "copyfiles", Required = false, HelpText = "Copies all log files to the temp directory (%TEMP% or $env:TEMP in powershell), inside the 'kelpie' directory.")]
		public bool CopyFiles { get; set; }

		[Option('w', "wipedata", Required = false, HelpText = "Wipes all data from the MongoDB database before importing. Use this for performing a fresh import.")]
		public bool WipeData { get; set; }

		[Option('k', "skipimport", Required = false, HelpText = "Doesn't import any data to the database (usually used in conjunction with --copyfiles).")]
		public bool SkipImport { get; set; }

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
			return help;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Kelpie.ConsoleApp
{
	internal class Options
	{
		[Option('f', "copyfiles", Required = false, HelpText = "Copies all log files to the temp directory (%TEMP% or $env:TEMP in powershell), inside the 'kelpie' directory.")]
		public bool CopyFiles { get; set; }

		[Option('w', "wipedata", Required = false, HelpText = "Wipes all data from the MongoDB database before importing. Use this for performing a fresh import.")]
		public bool WipeData { get; set; }

		[Option('k', "skip-import", Required = false, HelpText = "Doesn't import any data to the database (usually used in conjunction with --copyfiles.")]
		public bool SkipImport { get; set; }

		[Option('u', "smartupdate", Required = false, HelpText = "Only imports log entries in each application that are newer than any existing ones. If this is not set and you do not specify --wipedata, you will get duplicate log file entries.")]
		public bool SmartUpdate { get; set; }

		// server
		// environment
		// config location
	}
}

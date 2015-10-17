using System.Collections.Generic;
using Kelpie.Core.Domain;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Core
{
	public interface IConfiguration
	{
		/// <summary>
		/// If set, points to another Kelpie JSON configuration file. This is useful for sharing config files between the import
		/// tool and the website, or multiple instances.
		/// </summary>
		string ConfigFile { get; set; }

		/// <summary>
		/// The list of applications. Each application name usually maps to a directory in the log file directory for each server,
		/// for example D:\ErrorLogs\MyApp1 would be the "MyApp1" application.
		/// </summary>
		List<string> Applications { get; set; }

		/// <summary>
		/// The environments to import and display. This is for multi-environment web servers, e.g. development, UAT, production.
		/// </summary>
		List<Environment> Environments { get; set; }

		/// <summary>
		/// The number of log file entries to hold in memory, before flushing to the database during an import. For small log files 
		/// this figure can be set to a high number, however for large log files with a lot of text this number will need to be set 
		/// to a small number to avoid OutOfMemoryException errors which occur from multiple threads holding log entries.
		/// </summary>
		int ImportBufferSize { get; set; }

		/// <summary>
		/// The maximum number of days to retrieve log files from. Any log files before this number will be ignored.
		/// </summary>
		int MaxAgeDays { get; set; }

		/// <summary>
		/// Whether to use Lucene for searching. Lucene is faster than the native MongoDB-based search, but requires an index to be 
		/// created after each import. For smaller log files MongoDB's search is usually sufficient.
		/// </summary>
		bool IsLuceneEnabled { get; set; }

		/// <summary>
		/// The directory to place the Lucene search index. This directory should be readable by the user that the Kelpie application 
		/// pool runs under, and writeable by the user the import tool runs under.
		/// </summary>
		string LuceneIndexDirectory { get; set; }
	}
}
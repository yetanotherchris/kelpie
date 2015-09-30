using System;
namespace Kelpie.Core.Domain
{
    public class LogEntryFilter
    {
        public string LogApplication { get; set; }

        public int? Page { get; set; }

        public int? Rows { get; set; }
    }
}

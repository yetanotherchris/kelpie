using System.Collections.Generic;

namespace Kelpie.Web.Models
{
    public class SearchViewModel
    {
        public string CurrentEnvironment { get; set; }
        public IEnumerable<string> Environments { get; set; }

        public IEnumerable<string> Applications { get; set; }

        public SearchViewModel()
        {
            Environments = new List<string>();
            Applications = new List<string>();
        }
    }
}
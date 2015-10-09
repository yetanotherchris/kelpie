using System.Collections.Generic;

namespace Kelpie.Web.Models
{
	public class HomepageViewModel
	{
		public string CurrentEnvironment { get; set; }
		public IEnumerable<string> Environments { get; set; }
		public List<ServerViewModel> ServerModels { get; set; }

		public HomepageViewModel()
		{
			ServerModels = new List<ServerViewModel>();
		}
    }
}
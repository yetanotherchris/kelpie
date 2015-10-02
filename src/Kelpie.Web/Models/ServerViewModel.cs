namespace Kelpie.Web.Models
{
	public class ServerViewModel
	{
		public string Application { get; set; }
		public int ErrorCount { get; set; }
		public int ErrorCountPerServer { get; set; }
		public string TopExceptionType { get; set; }
		public int ServerCount { get; set; }
	}
}
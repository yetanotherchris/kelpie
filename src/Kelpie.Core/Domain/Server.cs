namespace Kelpie.Core.Domain
{
	public class Server
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public bool CopyFilesToLocal { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}
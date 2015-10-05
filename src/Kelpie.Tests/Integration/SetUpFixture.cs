using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Kelpie.Tests.Integration
{
	[SetUpFixture]
	public class SetUpFixture
	{
		private Process _mongoDbProcess;

		[SetUp]
		public void TestFixtureSetUp()
		{
			// Check if MongoDb is running
			if (!Process.GetProcessesByName("mongod").Any())
			{
				_mongoDbProcess = Process.Start(@"C:\Program Files\MongoDB\Server\3.0\bin\mongod.exe", @"--dbpath c:\mongodb\data\");
			}
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			if (_mongoDbProcess != null)
			{
				// Only kill mongod if the tests started it
				_mongoDbProcess.Kill();
			}
		}
	}
}
using System;
using System.IO;
using Kelpie.Core.Console;
using Kelpie.Core.Domain;
using Kelpie.Tests.MocksStubs;
using NUnit.Framework;
using Environment = Kelpie.Core.Domain.Environment;

namespace Kelpie.Tests.Integration
{
	public class RunnerTests
	{
		private const int FULL_LOG_ROW_COUNT = 648; // number of log entries in the full.log file

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("--help")]
		public void no_arguments_should_display_help(string args)
		{
			// Arrange
			var repository = new RepositoryMock();
			var config = new ConfigurationMock();
			var runner = new Runner(config, repository);

			// Act
			string output = runner.Run(new string[] { args });

			// Assert
			Assert.That(output, Is.StringContaining("Kelpie import tool"));
			Assert.That(output, Is.StringContaining("--help"));
		}

		[Test]
		public void wipedata_arg_should_clear_the_repository()
		{
			// Arrange
			string[] args = { "--wipedata" };

			var repository = new RepositoryMock();
			repository.LogEntries.Add(new LogEntry());
			repository.LogEntries.Add(new LogEntry());

			var config = new ConfigurationMock();
			var runner = new Runner(config, repository);

			// Act
			string output = runner.Run(args);

			// Assert
			Assert.That(output, Is.StringContaining("done"));
			Assert.That(repository.LogEntries.Count, Is.EqualTo(0));
		}

		[Test]
		public void environment_arg_should_scan_a_single_environment()
		{
			// Arrange
			string[] args = { "--environment=DEV" };

			var server1 = CreateFakeServer("JabberWocky1");
			var server2 = CreateFakeServer("JabberWocky2");

			var environmentProd = new Environment();
			environmentProd.Name = "PRODUCTION";
			environmentProd.Servers.Add(server1);

			var environmentDev = new Environment();
			environmentDev.Name = "DEV";
			environmentDev.Servers.Add(server1);
			environmentDev.Servers.Add(server2);

			var config = new ConfigurationMock();
			config.Environments.Add(environmentProd);
			config.Environments.Add(environmentDev);

			var repository = new RepositoryMock();
			var runner = new Runner(config, repository);

			// Act
			string output = runner.Run(args);

			// Assert
			Assert.That(output, Is.StringContaining("done"));
			Assert.That(repository.LogEntries.Count, Is.EqualTo(FULL_LOG_ROW_COUNT * 2));
		}

		[Test]
		public void server_arg_should_scan_a_single_server()
		{
			// Arrange
			string[] args = { "--server=JabberWocky1" };

			var server1 = CreateFakeServer("JabberWocky1");
			var server2 = CreateFakeServer("JabberWocky2");

			var environment = new Environment();
			environment.Name = "PRODUCTION";
			environment.Servers.Add(server1);
			environment.Servers.Add(server2);

			var config = new ConfigurationMock();
			config.Environments.Add(environment);

			var repository = new RepositoryMock();
			var runner = new Runner(config, repository);

			// Act
			string output = runner.Run(args);

			// Assert
			Assert.That(output, Is.StringContaining("done"));
			Assert.That(repository.LogEntries.Count, Is.EqualTo(FULL_LOG_ROW_COUNT));
		}

		[Test]
		public void skipimport_should_not_persist_data_to_the_repository()
		{
			// Arrange
			string[] args = { "--skipimport" };

			var server1 = CreateFakeServer("JabberWocky1");

			var environment = new Environment();
			environment.Name = "PRODUCTION";
			environment.Servers.Add(server1);

			var config = new ConfigurationMock();
			config.Environments.Add(environment);

			var repository = new RepositoryMock();
			var runner = new Runner(config, repository);

			// Act
			string output = runner.Run(args);

			// Assert
			Assert.That(output, Is.StringContaining("done"));
			Assert.That(repository.LogEntries.Count, Is.EqualTo(0));
		}

		private Server CreateFakeServer(string serverName)
		{
			return new Server()
			{
				CopyFilesToLocal = true,
				Name = serverName,
				Username = "uid",
				Password = "pwd",
				Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExampleLogs")
			};
		}
	}
}

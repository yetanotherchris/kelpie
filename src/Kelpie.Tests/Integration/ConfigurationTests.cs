using System;
using System.Configuration;
using System.IO;
using Kelpie.Core;
using Kelpie.Core.Exceptions;
using NUnit.Framework;

namespace Kelpie.Tests.Integration
{
	public class ConfigurationTests
	{
		[Test]
		public void read_should_deserialize_all_properties_from_config_file_in_current_directory()
		{
			// Arrange + Act
			var configuration = Configuration.Read();

			// Assert
			Assert.That(configuration, Is.Not.Null);
			Assert.That(configuration.LuceneIndexDirectory, Is.EqualTo(@"c:\code\kelpie\kelpie.web\App_Data\Lucene"));
			Assert.That(configuration.Applications.Count, Is.EqualTo(5));
			Assert.That(configuration.ConfigFile, Is.Empty.Or.Null);
			Assert.That(configuration.Environments.Count, Is.EqualTo(2));
			Assert.That(configuration.ImportBufferSize, Is.EqualTo(100));
			Assert.That(configuration.IsLuceneEnabled, Is.True);
			Assert.That(configuration.MaxAgeDays, Is.EqualTo(16));
        }

		[Test]
		public void read_should_deserialize_from_external_config_file_when_set()
		{
			// Arrange
			string configPath = Path.Combine("ExampleConfigs", "external-config1.config");

			// Act
			var configuration = Configuration.Read(configPath);

			// Assert
			Assert.That(configuration, Is.Not.Null);
			Assert.That(configuration.LuceneIndexDirectory, Is.EqualTo(@"external-config-lucene"));
			Assert.That(configuration.Applications.Count, Is.EqualTo(2));
			Assert.That(configuration.ConfigFile, Is.Empty.Or.Null);
			Assert.That(configuration.Environments.Count, Is.EqualTo(2));
			Assert.That(configuration.ImportBufferSize, Is.EqualTo(10));
			Assert.That(configuration.IsLuceneEnabled, Is.True);
			Assert.That(configuration.MaxAgeDays, Is.EqualTo(11));
		}

		[Test]
		public void read_should_throw_exception_when_kelpie_config_not_found()
		{
			// Arrange
			string configPath = "doesnt-exist.config";

			// Act + Assert
			Assert.Throws<InvalidConfigurationFileException>(() => Configuration.Read(configPath));
		}

		[Test]
		public void read_should_throw_exception_when_no_environments_found()
		{
			// Arrange
			string configPath = Path.Combine("ExampleConfigs", "no-environments.config");

			// Act + Assert
			Assert.Throws<InvalidConfigurationFileException>(() => Configuration.Read(configPath));
		}

		[Test]
		public void read_should_throw_exception_when_no_servers_found()
		{
			// Arrange
			string configPath = Path.Combine("ExampleConfigs", "no-servers.config");

			// Act + Assert
			Assert.Throws<InvalidConfigurationFileException>(() => Configuration.Read(configPath));
		}
	}
}

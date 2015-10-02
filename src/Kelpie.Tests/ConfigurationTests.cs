using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelpie.Core;
using NUnit.Framework;

namespace Kelpie.Tests
{
	public class ConfigurationTests
	{
		[Test]
		public void read_should_throw_exception_when_kelpie_config_not_found()
		{
			// Arrange
			var configuration = Configuration.Read();

			// Act

			// Assert
		}

		[Test]
		public void read_should_throw_exception_when_no_environments_found()
		{
			// Arrange
			var configuration = Configuration.Read();

			// Act

			// Assert
		}

		[Test]
		public void read_should_deserialize_from_config_file()
		{
			// Arrange
			var configuration = Configuration.Read();

			// Act

			// Assert
		}

		[Test]
		public void read_should_deserialize_when_external_config_file_is_set()
		{
			// Arrange
			var configuration = Configuration.Read();

			// Act

			// Assert
		}
	}
}

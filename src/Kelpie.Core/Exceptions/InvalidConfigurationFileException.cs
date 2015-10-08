using System;

namespace Kelpie.Core.Exceptions
{
	public class InvalidConfigurationFileException : Exception
	{
		public InvalidConfigurationFileException(string message, params object[] args) : base(string.Format(message, args))
		{
		}
	}
}
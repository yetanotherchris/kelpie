using System;

namespace Kelpie.Core.Exceptions
{
	public class KelpieException : Exception
	{
		public KelpieException(string message) : base(message)
		{
		}
	}
}
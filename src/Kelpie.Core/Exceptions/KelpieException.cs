using System;

namespace Kelpie.Core
{
	public class KelpieException : Exception
	{
		public KelpieException(string message) : base(message)
		{
		}
	}
}
using System;

namespace Kelpie.Core.Repository
{
	public class SearchException : Exception
	{
		public SearchException(Exception exception, string format, params object[] args) : base(string.Format(format, args), exception)
		{
		}
	}
}
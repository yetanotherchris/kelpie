using System;
using Kelpie.Core.Console;

namespace Kelpie.ImportTool
{
	class Program
	{
		static void Main(string[] args)
		{
			var runner = new Runner();
			string output = runner.Run(args);
			Console.WriteLine(output);
		}
	}
}

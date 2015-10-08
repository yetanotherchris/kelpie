using System;
using Kelpie.Core.Console;
using Kelpie.Core.Exceptions;

namespace Kelpie.ImportTool
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var runner = new Runner();
				string output = runner.Run(args);
				Console.WriteLine(output);
            }
			catch (InvalidConfigurationFileException e)
			{
                Console.Error.WriteLine("Error: {0}", e.Message);
				Environment.Exit(1);
			}
		}
	}
}

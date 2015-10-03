using System;
using System.Net;
using CommandLine;
using CommandLine.Text;

namespace Kelpie.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			
            if (Parser.Default.ParseArguments(args, options))
			{
				var runner = new Runner();
				runner.Refresh();
			}
			else
			{
				// Display the default usage information
				Console.WriteLine(HelpText.AutoBuild(options));
			}
		}
	}
}

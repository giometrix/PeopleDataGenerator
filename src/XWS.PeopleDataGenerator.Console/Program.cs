using CommandLine;
using System;
using System.IO;
using System.Linq;
using XWS.PeopleDataGenerator;


namespace XWS.PeopleDataGenerator.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<CommandOptions>(args)

				.WithNotParsed<CommandOptions>(errs =>
				{
					if (errs.Count() > 0)
						return;
				})
				.WithParsed<CommandOptions>(opts =>
				{
					var clg = new ContactListGenerator();
					clg.NumberOfLinesToGenerate = opts.NumberOfRecords;

					File.WriteAllText(opts.OutputFilePath, clg.Generate());
				});
		}
	}

	class CommandOptions
	{
		[Option('o', "output", Default = "output.csv")]
		public string OutputFilePath { get; set; }
		[Option('n', "number of records", Default = 1000)]
		public int NumberOfRecords { get; set; }
	}
}
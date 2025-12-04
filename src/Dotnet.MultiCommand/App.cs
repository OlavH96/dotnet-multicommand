using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand;

public class App(AppConsole _console)
{
	public int Execute(params string[] args)
	{
		var app = new CommandLineApplication();
		app.Description = "Dotnet Multi Command Tool (mc)";

		var gitOnly = app.Option("-g | --git", "Only run command in git directories", CommandOptionType.NoValue);
		var recursive = app.Option("-r | --recursive", "Recursively run commands in subdirectories", CommandOptionType.NoValue);
		var verbose = app.Option("--verbose", "Enable verbose output", CommandOptionType.NoValue);
		var includeFolderFilter = app.Option<string>("-i | --include-folder <TEXT>", "Only run command in directories containing specified text", CommandOptionType.SingleValue);
		var excludeFolderFilter = app.Option<string>("-e | --exclude-folder <TEXT>", "Do not run command in directories containing specified text", CommandOptionType.SingleValue);

		var versionOption = app.VersionOption("-v | --version", GetVersion());
		var helpOption = app.HelpOption("-h | --help");
		app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;

		app.ExtendedHelpText = @"
Examples:
  mc ls
  - Runs 'ls' in all directories in the current folder
  mc -g -r git status 
  - Runs 'git status' in all git repositories recursively
  mc -g -r -i Test -e Example --verbose ls 
  - Runs 'ls' in all git repositories recursively where folder name contains 'Test' but not 'Example' with verbose output
		";

		app.OnExecuteAsync(async cancellationToken =>
		{
			_console.Verbose = verbose.HasValue();
			if(app.RemainingArguments.Count == 0)
			{
				_console.WriteError("No command specified to run.");
				app.ShowHelp();
				return 0xbad;
			}

			var commandToRun = string.Join(" ", app.RemainingArguments);

			var worker = new MultiCommandRunner(_console)
				.WithCommand(commandToRun)
				.WithGitOnly(gitOnly.HasValue())
				.WithFolderInclusionFilter(includeFolderFilter.HasValue() ? includeFolderFilter.ParsedValue : null)
				.WithFolderExclusionFilter(excludeFolderFilter.HasValue() ? excludeFolderFilter.ParsedValue : null)
				.WithRecursive(recursive.HasValue())
			;
			await worker.Run();
			return 0;
		});

		return app.Execute(args);
	}

	private string GetVersion()
	{
		return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
	}
}
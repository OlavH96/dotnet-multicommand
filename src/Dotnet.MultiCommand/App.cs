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
		var hasChanges = app.Option("-hc | --has-changes", "Only run command in directories with uncommitted git changes", CommandOptionType.NoValue);
		var recursive = app.Option("-r | --recursive", "Recursively run commands in subdirectories", CommandOptionType.NoValue);
		var verbose = app.Option("--verbose", "Enable verbose output", CommandOptionType.NoValue);
		var includeFolderFilter = app.Option<string>("-i | --include-folder <TEXT>", "Only run command in directories containing specified text (can be used multiple times)", CommandOptionType.MultipleValue);
		var excludeFolderFilter = app.Option<string>("-e | --exclude-folder <TEXT>", "Do not run command in directories containing specified text (can be used multiple times)", CommandOptionType.MultipleValue);
		var includeFileFilter = app.Option<string>("-if | --include-file <TEXT>", "Only run command in directories containing a file with specified text in the filename (can be used multiple times)", CommandOptionType.MultipleValue);
		var excludeFileFilter = app.Option<string>("-ef | --exclude-file <TEXT>", "Do not run command in directories containing a file with specified text in the filename (can be used multiple times)", CommandOptionType.MultipleValue);

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
  mc -i Folder1 -i Folder2 -e node_modules ls
  - Runs 'ls' in directories containing 'Folder1' OR 'Folder2', but excludes directories containing 'node_modules'
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

			var commandToRun = string.Join(" ", app.RemainingArguments.Select(arg => 
				arg.Contains(' ') ? $"\"{arg}\"" : arg));

			var worker = new MultiCommandRunner(_console)
				.WithCommand(commandToRun)
				.WithGitOnly(gitOnly.HasValue())
				.WithHasChanges(hasChanges.HasValue())
				.WithFolderInclusionFilter(includeFolderFilter.HasValue() ? includeFolderFilter.Values.Where(v => v != null).Select(v => v!) : null)
				.WithFolderExclusionFilter(excludeFolderFilter.HasValue() ? excludeFolderFilter.Values.Where(v => v != null).Select(v => v!) : null)
				.WithFileInclusionFilter(includeFileFilter.HasValue() ? includeFileFilter.Values.Where(v => v != null).Select(v => v!) : null)
				.WithFileExclusionFilter(excludeFileFilter.HasValue() ? excludeFileFilter.Values.Where(v => v != null).Select(v => v!) : null)
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
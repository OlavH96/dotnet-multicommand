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
		app.Description = "Dotnet Multi Command Tool";

		var gitOnly = app.Option("-g | --git", "Only run command in git directories", CommandOptionType.NoValue);
		var recursive = app.Option("-r | --recursive", "Recursively run commands in subdirectories", CommandOptionType.NoValue);
		var verbose = app.Option("--verbose", "Enable verbose output", CommandOptionType.NoValue);

		var versionOption = app.VersionOption("-v | --version", GetVersion());
		var helpOption = app.HelpOption("-h | --help");
		app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;

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
using System.IO;
using CliWrap;
namespace Dotnet.MultiCommand.Core;

public class MultiCommandRunner(AppConsole _console)
{
	private Settings _settings = new Settings();
	public async Task Run()
	{
		_console.WriteNormal($"Running MultiCommand with settings: {_settings}");
		await DoCommand();
	}

	public async Task<bool> DoCommand()
	{
		_console.WriteNormal($"Executing command: {_settings.Command}");
		var baseCommand = _settings.Command.Split(' ')[0];
		_console.WriteNormal($"Base command: {baseCommand}");
		var rest = _settings.Command.Substring(baseCommand.Length).Trim();
		var res = await Cli.Wrap(baseCommand)
			.WithArguments(rest)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => _console.WriteHighlighted(s)))
			.WithStandardErrorPipe(PipeTarget.ToDelegate(s => _console.WriteError(s)))
			.ExecuteAsync();
		return res.ExitCode == 0;
	}
	public MultiCommandRunner WithGitOnly(bool gitOnly)
	{
		_settings = _settings with { GitOnly = gitOnly };
		return this;
	}
	public MultiCommandRunner WithRecursive(bool recursive)
	{
		_settings = _settings with { Recursive = recursive };
		return this;
	}
	public MultiCommandRunner WithCommand(string command)
	{
		_settings = _settings with { Command = command };
		return this;
	}
	
}
using System.IO;
using CliWrap;
namespace Dotnet.MultiCommand.Core;

public class MultiCommandRunner(AppConsole _console)
{
	private Settings _settings = new Settings();
	private Stats _stats = new Stats(0);
	public async Task Run()
	{
		_console.WriteHeader($"Running MultiCommand with settings: {_settings}");
		string currentDir = Directory.GetCurrentDirectory();
		_console.WriteNormal($"Current directory: {currentDir}");
		var dirs = Directory.GetDirectories(currentDir);

		await RunInDirectories(dirs);
		_console.WriteSuccess($"Finished running commands in directories. Total commands ran: {_stats.NumberOfCommandsRan}");
	}
	public async Task<bool> RunInDirectories(string[] directories)
	{
		if(directories.Length == 0)
		{
			return true;
		}
		foreach(var dir in directories)
		{
			if(_settings.Recursive)
			{
				var subDirs = Directory.GetDirectories(dir);
				await RunInDirectories(subDirs);
			}
			if(_settings.GitOnly && !Directory.Exists(Path.Combine(dir, ".git")))
			{
				_console.WriteVerbose($"Skipping non-git directory: {dir}");
				continue;
			}

			await DoCommand(dir);
		}
		return true;
	}

	public async Task<bool> DoCommand(string workingDirectory)
	{
		string? folderName = new DirectoryInfo(workingDirectory).Name;

		if (_settings.FolderExclusionFilter != null && folderName.Contains(_settings.FolderExclusionFilter))
		{
			_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it contains exclusion text '{_settings.FolderExclusionFilter}'.");
			return false;
		}
		if(_settings.FolderInclusionFilter != null && !folderName.Contains(_settings.FolderInclusionFilter))
		{
			_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it does not contain filter text '{_settings.FolderInclusionFilter}'.");
			return false;
		}
		_console.WriteNormal($"Executing command: {_settings.Command} in directory: {workingDirectory}");
		var baseCommand = _settings.Command.Split(' ')[0];
		var rest = _settings.Command.Substring(baseCommand.Length).Trim();
		var res = await Cli.Wrap(baseCommand)
			.WithArguments(rest)
			.WithValidation(CommandResultValidation.None) // todo: arg?
			.WithWorkingDirectory(workingDirectory)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => _console.WriteHighlighted(s)))
			.WithStandardErrorPipe(PipeTarget.ToDelegate(s => _console.WriteError(s)))
			.ExecuteAsync();
		_console.WriteEmptyLine();
		_stats = _stats with { NumberOfCommandsRan = _stats.NumberOfCommandsRan + 1 };
		return res.ExitCode == 0;
	}
	public MultiCommandRunner WithFolderInclusionFilter(string? folderContainsText)
	{
		_settings = _settings with { FolderInclusionFilter = folderContainsText };
		return this;
	}
	public MultiCommandRunner WithFolderExclusionFilter(string? folderExcludesText)
	{
		_settings = _settings with { FolderExclusionFilter = folderExcludesText };
		return this;
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
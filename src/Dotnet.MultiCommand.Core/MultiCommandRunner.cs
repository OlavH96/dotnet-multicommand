using System.IO;
using System.Text;
using CliWrap;
namespace Dotnet.MultiCommand.Core;

public enum CommandResult
{
	Success,
	Failure,
	SkippedBecauseGitOnly,
	SkippedBecauseFolderExclusionFilter,
	SkippedBecauseFolderInclusionFilter,
	SkippedBecauseFileExclusionFilter,
	SkippedBecauseFileInclusionFilter,
	SkippedBecauseNoGitChanges
}
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

			await DoCommand(dir);
		}
		return true;
	}

	public async Task<CommandResult> DoCommand(string workingDirectory)
	{
		if(_settings.GitOnly && !Directory.Exists(Path.Combine(workingDirectory, ".git")))
		{
			_console.WriteVerbose($"Skipping non-git directory: {workingDirectory}");
			return CommandResult.SkippedBecauseGitOnly;
		}
		string? folderName = new DirectoryInfo(workingDirectory).Name;

		if (_settings.FolderExclusionFilter != null && _settings.FolderExclusionFilter.Any(folderName.Contains))
		{
			var matchedFilter = _settings.FolderExclusionFilter.First(folderName.Contains);
			_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it contains exclusion text '{matchedFilter}'.");
			return CommandResult.SkippedBecauseFolderExclusionFilter;
		}
		if(_settings.FolderInclusionFilter != null && !_settings.FolderInclusionFilter.Any(filter => folderName.Contains(filter)))
		{
			_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it does not contain any inclusion filter text.");
			return CommandResult.SkippedBecauseFolderInclusionFilter;
		}
		if(_settings.FileExclusionFilter != null)
		{
			var files = Directory.GetFiles(workingDirectory);
			if(_settings.FileExclusionFilter.Any(filter => files.Any(f => Path.GetFileName(f).Contains(filter))))
			{
				var matchedFilter = _settings.FileExclusionFilter.First(filter => files.Any(f => Path.GetFileName(f).Contains(filter)));
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it contains a file with exclusion text '{matchedFilter}'.");
				return CommandResult.SkippedBecauseFileExclusionFilter;
			}
		}
		if(_settings.FileInclusionFilter != null)
		{
			var files = Directory.GetFiles(workingDirectory);
			if(!_settings.FileInclusionFilter.Any(filter => files.Any(f => Path.GetFileName(f).Contains(filter))))
			{
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it does not contain a file with any inclusion filter text.");
				return CommandResult.SkippedBecauseFileInclusionFilter;
			}
		}
		if(_settings.HasChanges)
		{
			var hasGitChanges = await CheckForGitChanges(workingDirectory);
			if(!hasGitChanges)
			{
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it has no uncommitted git changes.");
				return CommandResult.SkippedBecauseNoGitChanges;
			}
		}
		_console.WriteNormal($"Executing command: {_settings.Command} in directory: {workingDirectory}");
		var baseCommand = _settings.Command.Split(' ')[0];
		var rest = _settings.Command.Substring(baseCommand.Length).Trim();
		var res = await Cli.Wrap(baseCommand)
			.WithArguments(args => args.Add(rest, false)) // false = don't escape, the string is already properly formatted
			.WithValidation(CommandResultValidation.None) // todo: arg?
			.WithWorkingDirectory(workingDirectory)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => _console.WriteHighlighted(s)))
			.WithStandardErrorPipe(PipeTarget.ToDelegate(s => _console.WriteError(s)))
			.ExecuteAsync();
		_console.WriteEmptyLine();
		_stats = _stats with { NumberOfCommandsRan = _stats.NumberOfCommandsRan + 1 };
		return res.ExitCode == 0 ? CommandResult.Success : CommandResult.Failure;
	}
	public MultiCommandRunner WithFolderInclusionFilter(IEnumerable<string>? folderContainsText)
	{
		_settings = _settings with { FolderInclusionFilter = folderContainsText };
		return this;
	}
	public MultiCommandRunner WithFolderExclusionFilter(IEnumerable<string>? folderExcludesText)
	{
		_settings = _settings with { FolderExclusionFilter = folderExcludesText };
		return this;
	}
	private async Task<bool> CheckForGitChanges(string workingDirectory)
	{
		// Check for tracked changes (staged or unstaged, but not untracked files)
		var statusOutput = new List<string>();
		var statusResult = await Cli.Wrap("git")
			.WithArguments("status --porcelain")
			.WithWorkingDirectory(workingDirectory)
			.WithValidation(CommandResultValidation.None)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => statusOutput.Add(s)))
			.WithStandardErrorPipe(PipeTarget.ToDelegate(s => statusOutput.Add(s)))
			.ExecuteAsync();
		
		bool hasTrackedChanges = statusResult.ExitCode == 0 && 
			statusOutput.Any(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("??"));

		if(statusResult.ExitCode != 0)
		{
			Console.Error.WriteLine($"Error checking git status in '{workingDirectory}'. Exit code: {statusResult.ExitCode} Output: {string.Join(Environment.NewLine, statusOutput)}");
			return false;
		}

		// Check for unpushed commits (ahead of remote)
		var logOutput = new StringBuilder();
		var logResult = await Cli.Wrap("git")
			.WithArguments("log @{u}..HEAD --oneline")
			.WithWorkingDirectory(workingDirectory)
			.WithValidation(CommandResultValidation.None)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => logOutput.AppendLine(s)))
			.WithStandardErrorPipe(PipeTarget.ToDelegate(s => logOutput.AppendLine(s)))
			.ExecuteAsync();

		if(logResult.ExitCode != 0)
		{
			Console.Error.WriteLine($"Error checking git log in '{workingDirectory}'. Exit code: {logResult.ExitCode} Output: {logOutput}");
			return false;
		}
		
		bool hasUnpushedCommits = logResult.ExitCode == 0 && logOutput.Length > 0;

		return hasTrackedChanges || hasUnpushedCommits;
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
	public MultiCommandRunner WithFileInclusionFilter(IEnumerable<string>? fileContainsText)
	{
		_settings = _settings with { FileInclusionFilter = fileContainsText };
		return this;
	}
	public MultiCommandRunner WithFileExclusionFilter(IEnumerable<string>? fileExcludesText)
	{
		_settings = _settings with { FileExclusionFilter = fileExcludesText };
		return this;
	}
	public MultiCommandRunner WithHasChanges(bool hasChanges)
	{
		_settings = _settings with { HasChanges = hasChanges };
		return this;
	}
	
}
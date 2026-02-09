using System.IO;
using System.Text;
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
		if(_settings.FileExclusionFilter != null)
		{
			var files = Directory.GetFiles(workingDirectory);
			if(files.Any(f => Path.GetFileName(f).Contains(_settings.FileExclusionFilter)))
			{
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it contains a file with exclusion text '{_settings.FileExclusionFilter}'.");
				return false;
			}
		}
		if(_settings.FileInclusionFilter != null)
		{
			var files = Directory.GetFiles(workingDirectory);
			if(!files.Any(f => Path.GetFileName(f).Contains(_settings.FileInclusionFilter)))
			{
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it does not contain a file with filter text '{_settings.FileInclusionFilter}'.");
				return false;
			}
		}
		if(_settings.HasChanges)
		{
			var hasGitChanges = await CheckForGitChanges(workingDirectory);
			if(!hasGitChanges)
			{
				_console.WriteVerbose($"Skipping directory '{workingDirectory}' as it has no uncommitted git changes.");
				return false;
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
	private async Task<bool> CheckForGitChanges(string workingDirectory)
	{
		// Check for tracked changes (staged or unstaged, but not untracked files)
		var statusOutput = new List<string>();
		var statusResult = await Cli.Wrap("git")
			.WithArguments("status --porcelain")
			.WithWorkingDirectory(workingDirectory)
			.WithValidation(CommandResultValidation.None)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => statusOutput.Add(s)))
			.ExecuteAsync();
		
		bool hasTrackedChanges = statusResult.ExitCode == 0 && 
			statusOutput.Any(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("??"));

		// Check for unpushed commits (ahead of remote)
		var logOutput = new StringBuilder();
		var logResult = await Cli.Wrap("git")
			.WithArguments("log @{u}..HEAD --oneline")
			.WithWorkingDirectory(workingDirectory)
			.WithValidation(CommandResultValidation.None)
			.WithStandardOutputPipe(PipeTarget.ToDelegate(s => logOutput.AppendLine(s)))
			.ExecuteAsync();
		
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
	public MultiCommandRunner WithFileInclusionFilter(string? fileContainsText)
	{
		_settings = _settings with { FileInclusionFilter = fileContainsText };
		return this;
	}
	public MultiCommandRunner WithFileExclusionFilter(string? fileExcludesText)
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
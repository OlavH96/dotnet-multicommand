namespace Dotnet.MultiCommand.Core;
public record Settings(
	bool GitOnly = false,
	bool HasChanges = false,
	bool Recursive = false,
	string Command = "ls",
	IEnumerable<string>? FolderInclusionFilter = null,
	IEnumerable<string>? FolderExclusionFilter = null,
	IEnumerable<string>? FileInclusionFilter = null,
	IEnumerable<string>? FileExclusionFilter = null
);
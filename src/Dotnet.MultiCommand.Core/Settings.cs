namespace Dotnet.MultiCommand.Core;
public record Settings(
	bool GitOnly = false,
	bool HasChanges = false,
	bool Recursive = false,
	string Command = "ls",
	string? FolderInclusionFilter = null,
	string? FolderExclusionFilter = null,
	string? FileInclusionFilter = null,
	string? FileExclusionFilter = null
);
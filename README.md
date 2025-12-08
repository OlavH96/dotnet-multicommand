[![NuGet Version](https://img.shields.io/nuget/v/dotnet-multicommand)](https://www.nuget.org/packages/dotnet-multicommand/)
# Dotnet.MultiCommand (mc)

A .NET global tool for running commands across multiple directories simultaneously.

## Features

- 🚀 **Run any command** across multiple directories
- 📁 **Recursive execution** in subdirectories
- 🔍 **Git-aware** - filter for git repositories only
- 🎯 **Folder filtering** - include/exclude directories by name
- 📄 **File filtering** - include/exclude directories based on files they contain

## Installation

Install as a global .NET tool:

```bash
dotnet tool install --global dotnet-multicommand
```

Or install from a local package:

```bash
dotnet tool install --global --add-source ./src/Dotnet.MultiCommand/nupkg dotnet-multicommand
```

## Usage

### Basic Syntax

```bash
mc [options] <command>
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `-g, --git` | | Only run command in directories containing `.git` folder |
| `-r, --recursive` | | Recursively run commands in all subdirectories |
| `-i, --include-folder <TEXT>` | | Only run in directories containing specified text |
| `-e, --exclude-folder <TEXT>` | | Skip directories containing specified text |
| `-if, --include-file <TEXT>` | | Only run in directories containing a file with specified text |
| `-ef, --exclude-file <TEXT>` | | Skip directories containing a file with specified text |
| `--verbose` | | Enable verbose output for debugging |
| `-v, --version` | | Display version information |
| `-h, --help` | | Display help information |

### Examples

**Run a command in all immediate subdirectories:**
```bash
mc ls
```

**Check git status in all git repositories:**
```bash
mc -g git status
```

**Recursively run git pull in all repositories:**
```bash
mc -g -r git pull
```

**Run commands only in test directories:**
```bash
mc -g -r -i Test ls
```

**Run commands excluding example directories:**
```bash
mc -g -r -e Example git status
```

**Complex filtering with verbose output:**
```bash
mc -g -r -i Test -e Example --verbose npm install
```

**Pull latest changes in all git repos:**
```bash
mc -g -r git pull origin main
```

**Check for uncommitted changes:**
```bash
mc -g -r git status --short
```

**Run build commands across projects:**
```bash
mc -r dotnet build
```

**Run commands only in directories with package.json:**
```bash
mc --include-file package.json npm install
```

**Skip directories containing lock files:**
```bash
mc --exclude-file .lock git status
```

**Run Docker build in directories with Dockerfile:**
```bash
mc -r --include-file Dockerfile docker build -t myapp .
```

**Run tests only in directories with test files:**
```bash
mc -r --include-file test.csproj dotnet test
```

## How It Works

1. **Scans** the current directory for subdirectories
2. **Filters** directories based on your options:
   - Git repository detection (`.git` folder)
   - Folder name inclusion/exclusion patterns
   - File presence inclusion/exclusion patterns
3. **Executes** your command in each matching directory
4. **Displays** output in real-time with color coding
5. **Reports** total number of commands executed

## Use Cases

### Git Operations
```bash
# Check status of all repos
mc -g -r git status

# Pull latest changes
mc -g -r git pull

# Create a branch in all repos
mc -g -r git checkout -b feature/new-feature
```

### Node.js Projects
```bash
# Install dependencies in all Node.js projects
mc -r --include-file package.json npm install

# Run tests in projects with package.json
mc --include-file package.json npm test

# Update dependencies
mc --include-file package.json npm update
```

### .NET Projects
```bash
# Build all .NET projects
mc -r --include-file .csproj dotnet build

# Run tests in all test projects
mc -r --include-file test.csproj dotnet test

# Restore packages
mc --include-file .csproj dotnet restore
```

### Docker Projects
```bash
# Build all Docker images
mc -r --include-file Dockerfile docker build -t myapp .

# Clean up Docker artifacts
mc --include-file docker-compose.yml docker-compose down
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/OlavH96/dotnet-multicommand.git
cd dotnet-multicommand

# Build the project
dotnet build

# Pack as a tool
dotnet pack src/Dotnet.MultiCommand/Dotnet.MultiCommand.csproj

# Install locally
dotnet tool install --global --add-source ./src/Dotnet.MultiCommand/nupkg dotnet-multicommand
```

## Requirements

- .NET 10.0 SDK or later

## Project Structure

```
dotnet-multicommand/
├── src/
│   ├── Dotnet.MultiCommand/          # CLI tool entry point
│   │   ├── Program.cs                # Main entry point
│   │   ├── App.cs                    # Command-line argument parsing
│   │   └── Dotnet.MultiCommand.csproj
│   └── Dotnet.MultiCommand.Core/     # Core logic
│       ├── MultiCommandRunner.cs     # Command execution engine
│       ├── AppConsole.cs             # Console output handling
│       ├── Settings.cs               # Configuration settings
│       ├── Stats.cs                  # Execution statistics
│       └── Dotnet.MultiCommand.Core.csproj
└── test/                             # Test directories
```

## Dependencies

- [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) - Command-line parsing
- [CliWrap](https://github.com/Tyrrrz/CliWrap) - Process execution wrapper

## todo
- Allow strings in the command, like mc -r -g git coomit -m "message"
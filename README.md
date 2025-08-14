# validate-slnf

A .NET tool to validate that Visual Studio Solution Filter (.slnf) files have not fallen out of date.

## Purpose

When working with large codebases, .slnf files are used to create a subset of projects from a larger solution (.sln or .slnx). However, these files can become invalid over time if:

- Projects referenced in the .slnf are removed from the parent solution (.sln or .slnx)
- Project files referenced in the .slnf no longer exist on disk

This tool checks both of these conditions and reports any issues.

## Installation

```
dotnet tool install --global validate-slnf
```

## Usage

Basic usage:
```
validate-slnf mysolution.slnf
```

Multiple files:
```
validate-slnf file1.slnf file2.slnf file3.slnf
```

Check all .slnf files in current directory:
```
validate-slnf
```

Options:
```
--help, -h                Display help information
--verbose, -v             Print detailed information about the validation process
--skip-solution-check, -s Skip checking if projects exist in the parent solution file (.sln or .slnx)
--skip-disk-check, -d     Skip checking if projects exist on disk
```

Exit codes:
- 0: All checks passed (or no .slnf files found)
- 1: One or more checks failed

## Behavior

- With no arguments: Processes all .slnf files in the current directory.
- With file arguments: Processes only the specified .slnf files.
- If no .slnf files are found or provided: Outputs a warning to STDOUT and returns exit code 0.
- With no options: By default, performs both parent solution and disk existence checks.
- In case of success (no issues found): No output is produced.
- In case of failure: All issues are logged to STDERR and the program exits with code 1.
- With `--verbose`: Detailed information is logged to STDOUT regardless of success/failure.

## Example

```
# Validating a single file
> validate-slnf MyProject.slnf --verbose
Processing MyProject.slnf...
Parent solution file: C:\path\to\MyProject.sln
Projects in .slnf file (15):
  Project1\Project1.csproj
  Project2\Project2.csproj
  ...additional projects...

All projects exist in the parent solution
All project files exist on disk
All files validated successfully

# Validating multiple files
> validate-slnf Project1.slnf Project2.slnf --verbose
Processing Project1.slnf...
Parent solution file: C:\path\to\Project1.sln
Projects in .slnf file (2):
  Project1\Project1.csproj
  Project2\Project2.csproj

All projects exist in the parent solution
All project files exist on disk
Processing Project2.slnf...
Parent solution file: C:\path\to\Project2.sln
Projects in .slnf file (3):
  Project1\Project1.csproj
  Project2\Project2.csproj
  Project3\Project3.csproj

Projects missing from parent solution:
  Project3\Project3.csproj
Projects missing from disk:
  Project3\Project3.csproj

# Validating a file with issues (non-verbose)
> validate-slnf InvalidProject.slnf
Validation failed for InvalidProject.slnf:
  Projects missing from parent solution:
    Project3\Project3.csproj
  Projects missing from disk:
    Project3\Project3.csproj

# No .slnf files found
> validate-slnf
Warning: No .slnf files found in the current directory.
```

## License

MIT

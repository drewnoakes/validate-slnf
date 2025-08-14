# Specification: validate-slnf

## Overview

`validate-slnf` is a command-line tool that validates Visual Studio Solution Filter (.slnf) files to ensure they remain valid with respect to their parent solution files (.sln or .slnx) and the project files on disk.

## Requirements

1. The tool must verify that every project in a .slnf file is present in the corresponding parent solution file (.sln or .slnx).
2. The tool must verify that every project file referenced in a .slnf file exists on disk.
3. The tool must support disabling either check via command-line options.
4. The tool must follow the "quiet on success" principle, only producing output on errors or when verbose mode is enabled.
5. The tool must report all issues found, not just the first one.
6. The tool must return exit code 0 for success and 1 for any validation failure.
7. If no .slnf file is provided, the tool must scan all .slnf files in the current directory.
8. The tool must support validating multiple .slnf files when explicitly provided.
9. If no file name is provided, and no .slnf files are found in the current directory, the tool must output a warning to STDOUT and return exit code 0.
10. The tool must provide a help option that displays usage information.

## Input

- Path to one or more .slnf files (optional - if not provided, scans current directory)
- Command-line options (optional):
  - Help options: `--help`, `-h`, `/?`
  - Validation options: `--skip-solution-check`, `--skip-disk-check`, `--verbose`

## Command-line Options

- `--help`, `-h`: Displays help information
- `--skip-solution-check`, `-s`: Disables checking if projects exist in the parent solution file (.sln or .slnx)
- `--skip-disk-check`, `-d`: Disables checking if projects exist on disk
- `--verbose`, `-v`: Enables detailed logging of the validation process

## File Format Details

### SLNF File Format
A .slnf file is a JSON file with the following structure:
```json
{
  "solution": {
    "path": "relative/path/to/solution.sln", // or .slnx
    "projects": [
      "Project1.csproj",
      "Path/To/Project2.csproj"
    ]
  }
}
```

### SLN/SLNX File Format
These are Visual Studio solution files that contain project references. We will use the MSBuild library for parsing these files to extract project references.

## Processing Logic

1. Process command line arguments:
   - If help is requested (`--help`, `-h`, `/?`), display help information and exit with code 0
   - Check for validation options: `--verbose`/`-v`, `--skip-solution-check`/`-s`, `--skip-disk-check`/`-d`
   - Determine file paths to process

2. Determine which .slnf files to process:
   - If specific files are provided as arguments, use those
   - If no files are provided, scan the current directory for .slnf files
   - If no files are found, output a warning and exit with code 0

2. For each .slnf file:
   - Parse the file to extract:
     - The path to the parent solution file (.sln or .slnx)
     - The list of project paths

   - If solution checks are enabled:
     - Locate and parse the referenced parent solution file using the MSBuild library
     - Verify each project in the .slnf exists in the parent solution
     - Collect any missing projects

   - If disk checks are enabled:
     - For each project path in the .slnf:
       - Resolve the full path relative to the parent solution file
       - Check if the file exists on disk
       - Collect any missing files

3. Aggregate all errors across all processed .slnf files

4. Report results:
   - In verbose mode: Log detailed information about the checks
   - In case of failure: Log all issues to STDERR
   - Set appropriate exit code (1 if any errors found, 0 otherwise)

## Error Handling

- Invalid .slnf file format: Error message + exit code 1
- Parent solution file not found: Error message + exit code 1
- Any other unexpected error: Error message + exit code 1

## Dependencies

- MSBuild library for parsing solution files (.sln and .slnx)
- System.Text.Json for parsing .slnf files

## Performance Considerations

- The tool should be efficient, avoiding unnecessary file operations
- For large solutions, parse the parent solution file only once

## Future Enhancements (Not in Initial Version)

- JSON output format for integration with other tools
- Auto-fix option to remove invalid projects from the .slnf file

## Project Structure

The tool has been implemented with the following structure:

```
validate-slnf/
├── ValidateSlnf/                # Main project
│   ├── Program.cs               # Entry point and CLI handling
│   ├── SlnfValidator.cs         # Core validation logic
│   ├── SolutionParser.cs        # Solution file parsing
│   ├── FileSystem/              # File system abstraction
│   │   ├── IFileSystem.cs       # File system interface
│   │   └── RealFileSystem.cs    # Concrete implementation
│   ├── Models/                  # Data models
│   │   ├── SlnfFile.cs          # SLNF file model
│   │   └── ValidationResult.cs  # Validation results
│   └── ValidateSlnf.csproj      # Project file
├── ValidateSlnf.Tests/          # Test project
│   ├── SlnfValidatorTests.cs    # Tests for validation logic
│   ├── ProgramTests.cs          # Tests for CLI functionality
│   ├── TestData/                # Test solutions and slnf files
│   │   ├── ValidSolution/       # Valid test cases
│   │   └── InvalidSolution/     # Invalid test cases
│   └── ValidateSlnf.Tests.csproj
└── validate-slnf.sln            # Solution file
```

Key implementation aspects:
- File system abstraction (IFileSystem) for improved testability
- Solution parser abstraction (ISolutionParser) for decoupling solution parsing
- Command line parsing with support for both long and short-form options
- Well-structured error reporting with clear output formatting
- Comprehensive unit tests with mocked dependencies
- Packaging as a global .NET tool

# Copilot Instructions for validate-slnf

This document outlines the development plan and decisions for the `validate-slnf` tool.

## Project Overview

We're building a .NET tool called `validate-slnf` that validates Visual Studio Solution Filter (.slnf) files by:
1. Checking that all projects in a .slnf exist in the parent solution file (.sln or .slnx)
2. Checking that all projects in a .slnf exist on disk

The tool can validate:
- A single .slnf file specified on the command line
- Multiple .slnf files specified on the command line
- All .slnf files in the current directory if no files are specified

## Technology Decisions

- **Framework**: .NET 8.0 (latest stable)
- **Project Type**: .NET Tool (global tool package)
- **Dependencies**:
  - Microsoft.Build for parsing solution files
  - System.Text.Json for parsing .slnf files (JSON)
- **Testing**: xUnit for unit tests with test data

## Project Structure

```
validate-slnf/
├── ValidateSlnf/                # Main project
│   ├── Program.cs               # Entry point and CLI handling
│   ├── SlnfValidator.cs         # Core validation logic
│   ├── SolutionParser.cs        # Solution file parsing
│   ├── Models/                  # Data models
│   │   ├── SlnfFile.cs          # SLNF file model
│   │   └── ValidationResult.cs  # Validation results
│   └── ValidateSlnf.csproj      # Project file
├── ValidateSlnf.Tests/          # Test project
│   ├── SlnfValidatorTests.cs    # Tests for validation logic
│   ├── TestData/                # Test solutions and slnf files
│   │   ├── ValidSolution/       # Valid test cases
│   │   └── InvalidSolution/     # Invalid test cases
│   └── ValidateSlnf.Tests.csproj
├── validate-slnf.sln            # Solution file
├── README.md                    # Project documentation
└── SPECIFICATION.md             # Detailed requirements
```

## Implementation Plan

1. **Setup Project Structure**
   - Create solution and projects
   - Configure as .NET tool

2. **Implement Core Components**
   - Parse .slnf files
   - Use MSBuild to parse solution files
   - Implement validation logic

3. **Implement CLI**
   - Parse command line arguments
   - Handle verbose mode and option flags
   - Implement proper error reporting

4. **Testing**
   - Create test data with valid and invalid scenarios
   - Write unit tests for all validation logic
   - Test CLI behavior

5. **Packaging**
   - Configure NuGet package
   - Test installation and usage as a .NET tool

## CLI Design

The CLI will be simple with minimal dependencies:
- Optional arguments: Paths to .slnf files (if none provided, scan current directory)
- Optional flags:
  - `--skip-solution-check`: Skip solution validation
  - `--skip-disk-check`: Skip disk validation
  - `--verbose`: Enable verbose output

Exit codes:
- 0: All checks passed or no .slnf files found
- 1: One or more checks failed

## Key Implementation Details

- The validator will be designed as a library separate from CLI concerns
- Error collection will be comprehensive, gathering all errors before reporting
- File path resolution will handle relative paths correctly
- MSBuild's solution parsing capabilities will be used to reliably extract project references

## Future Considerations

- Multi-file validation
- JSON output format
- Auto-fix capabilities

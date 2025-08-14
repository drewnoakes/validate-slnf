using ValidateSlnf.FileSystem;

namespace ValidateSlnf.Tests;

public class ProgramTests : IDisposable
{
    private const string BasePath = @"C:\test";

    private readonly MockFileSystem _fileSystem;
    private readonly MockSolutionParser _solutionParser;

    public ProgramTests()
    {
        // Set up mock environment
        _fileSystem = new MockFileSystem(BasePath);
        _solutionParser = new MockSolutionParser();

        Program._fileSystemOverride = _fileSystem;
        Program._solutionParserOverride = _solutionParser;
    }

    public void Dispose()
    {
        // Cleanup
        Program._fileSystemOverride = null;
        Program._solutionParserOverride = null;
    }
    
    [Fact]
    public void Main_NoFilesSpecified_NoSlnfFilesFound_ReturnsZero()
    {
        var (exitCode, stdout, stderr) = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main([]);
        });

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Warning: No .slnf files found in the current directory.", stdout);
        Assert.Empty(stderr);
    }
    
    [Fact]
    public void Main_ValidFile_NoIssues_ReturnsZero()
    {
        // Add a valid .slnf file
        var slnfContent = """
            {
            "solution": {
                "path": "valid.sln",
                "projects": [
                "Project1\\Project1.csproj",
                "Project2\\Project2.csproj"
                ]
            }
            }
            """;
        _fileSystem.AddFile(@"C:\test\valid.slnf", slnfContent);
        _fileSystem.AddFile(@"C:\test\valid.sln", ""); // Add the solution file
        _fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        _fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");

        // Setup the solution parser
        _solutionParser.AddSolution(@"C:\test\valid.sln",
        [
            @"Project1\Project1.csproj",
            @"Project2\Project2.csproj"
        ]);
        
        var (exitCode, stdout, stderr) = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main([@"C:\test\valid.slnf"]);
        });
        
        // Assert
        Assert.Equal(0, exitCode);
        Assert.Empty(stdout); // No output on success
        Assert.Empty(stderr); // No errors
    }
    
    [Fact]
    public void Main_ValidFile_Verbose_ShowsOutput()
    {
        // Add a valid .slnf file
        var slnfContent = """
            {
            "solution": {
                "path": "valid.sln",
                "projects": [
                "Project1\\Project1.csproj",
                "Project2\\Project2.csproj"
                ]
            }
            }
            """;
        _fileSystem.AddFile(@"C:\test\valid.slnf", slnfContent);
        _fileSystem.AddFile(@"C:\test\valid.sln", ""); // Add the solution file
        _fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        _fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");

        // Setup the solution parser
        _solutionParser.AddSolution(@"C:\test\valid.sln",
        [
            @"Project1\Project1.csproj",
            @"Project2\Project2.csproj"
        ]);
    
        var output = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main([@"C:\test\valid.slnf", "--verbose"]);
        });
        
        // Assert
        Assert.Equal(0, output.exitCode);
        Assert.Contains("Processing", output.stdout); // Verbose output
        Assert.Contains("All projects exist in the parent solution", output.stdout);
        Assert.Contains("All project files exist on disk", output.stdout);
        Assert.Empty(output.stderr); // No errors
    }
    
    [Fact]
    public void Main_InvalidFile_ShowsErrors_ReturnsOne()
    {
        // Arrange - Setup mock environment
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        Program._fileSystemOverride = fileSystem;
        Program._solutionParserOverride = solutionParser;
        
        // Add an invalid .slnf file (missing Project2 from solution)
        var slnfContent = """
            {
                "solution": {
                    "path": "invalid.sln",
                    "projects": [
                        "Project1\\Project1.csproj",
                        "Project2\\Project2.csproj"
                    ]
                }
            }
            """;
        fileSystem.AddFile(@"C:\test\invalid.slnf", slnfContent);
        fileSystem.AddFile(@"C:\test\invalid.sln", ""); // Add the solution file
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        
        // Setup the solution parser with only one project
        solutionParser.AddSolution(@"C:\test\invalid.sln",
        [
            @"Project1\Project1.csproj"
        ]);
    
        var (exitCode, stdout, stderr) = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main([@"C:\test\invalid.slnf"]);
        });
        
        // Assert
        Assert.Equal(1, exitCode);
        Assert.Empty(stdout); // No standard output
        Assert.Contains("Validation failed", stderr); // Error output
        Assert.Contains("Projects missing from parent solution", stderr);
        Assert.Contains("Projects missing from disk", stderr);
    }
    
    [Fact]
    public void Main_InvalidFile_Verbose_ShowsErrorsAndOutput_ReturnsOne()
    {
        // Add an invalid .slnf file (missing Project2 from solution)
        var slnfContent = """
            {
              "solution": {
                "path": "invalid.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        _fileSystem.AddFile(@"C:\test\invalid.slnf", slnfContent);
        _fileSystem.AddFile(@"C:\test\invalid.sln", ""); // Add the solution file
        _fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");

        // Setup the solution parser with only one project
        _solutionParser.AddSolution(@"C:\test\invalid.sln",
        [
            @"Project1\Project1.csproj"
        ]);
    
        var (exitCode, stdout, stderr) = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main([@"C:\test\invalid.slnf", "--verbose"]);
        });
        
        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Processing", stdout); // Verbose output
        Assert.Contains("Projects missing from parent solution", stdout);
        Assert.Contains("Projects missing from disk", stdout);
        Assert.Contains("Validation failed", stderr);
    }
    
    [Fact]
    public void Main_SkipChecks_NoIssues_ReturnsZero()
    {
        // Add an invalid .slnf file (missing Project2 from solution and disk)
        var slnfContent = """
            {
              "solution": {
                "path": "invalid.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        _fileSystem.AddFile(@"C:\test\invalid.slnf", slnfContent);
        _fileSystem.AddFile(@"C:\test\invalid.sln", ""); // Add the solution file
        _fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");

        // Setup the solution parser with only one project
        _solutionParser.AddSolution(@"C:\test\invalid.sln",
        [
            @"Project1\Project1.csproj"
        ]);
    
        var (exitCode, stdout, stderr) = CaptureConsoleOutput(() =>
        {
            // Act
            return Program.Main(
            [
                @"C:\test\invalid.slnf",
                "--skip-solution-check",
                "--skip-disk-check"
            ]);
        });
        
        // Assert
        Assert.Equal(0, exitCode);
        Assert.Empty(stdout); // No output on success
        Assert.Empty(stderr); // No errors
    }
    
    private static (int exitCode, string stdout, string stderr) CaptureConsoleOutput(Func<int> action)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        
        using var outWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        
        Console.SetOut(outWriter);
        Console.SetError(errorWriter);
        
        int exitCode;
        try
        {
            exitCode = action();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
        
        return (exitCode, outWriter.ToString(), errorWriter.ToString());
    }
}

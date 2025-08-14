using ValidateSlnf.FileSystem;

namespace ValidateSlnf.Tests;

public class SlnfValidatorTests
{
    private const string BasePath = @"C:\test";
    private const string SlnfPath = @"C:\test\test.slnf";
    private const string SolutionPath = @"C:\test\test.sln";
    
    [Fact]
    public void ValidateSlnfFile_AllProjectsValid_NoIssues()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Set up the solution with two projects
        solutionParser.AddSolution(SolutionPath,
        [
            @"Project1\Project1.csproj",
            @"Project2\Project2.csproj"
        ]);
        
        // Add the project files
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");
        
        // Set up the .slnf file referencing both projects
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        fileSystem.AddFile(SolutionPath, ""); // Add the solution file
        
        var validator = new SlnfValidator(fileSystem, solutionParser);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.False(result.HasIssues);
        Assert.True(result.SolutionFileExists);
        Assert.Empty(result.ProjectsMissingFromSolution);
        Assert.Empty(result.ProjectsMissingFromDisk);
    }
    
    [Fact]
    public void ValidateSlnfFile_MissingFromSolution_HasIssues()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Set up the solution with one project (Project2 is missing)
        solutionParser.AddSolution(SolutionPath,
        [
            @"Project1\Project1.csproj"
        ]);
        
        // Add the project files
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");
        
        // Set up the .slnf file referencing both projects
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        fileSystem.AddFile(SolutionPath, ""); // Add the solution file
        
        var validator = new SlnfValidator(fileSystem, solutionParser);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.True(result.HasIssues);
        Assert.True(result.SolutionFileExists);
        Assert.Single(result.ProjectsMissingFromSolution);
        Assert.Equal(@"Project2\Project2.csproj", result.ProjectsMissingFromSolution[0]);
        Assert.Empty(result.ProjectsMissingFromDisk);
    }
    
    [Fact]
    public void ValidateSlnfFile_MissingFromDisk_HasIssues()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Set up the solution with two projects
        solutionParser.AddSolution(SolutionPath,
        [
            @"Project1\Project1.csproj",
            @"Project2\Project2.csproj"
        ]);
        
        // Add only one project file (Project2 is missing)
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        
        // Set up the .slnf file referencing both projects
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        fileSystem.AddFile(SolutionPath, ""); // Add the solution file
        
        var validator = new SlnfValidator(fileSystem, solutionParser);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.True(result.HasIssues);
        Assert.True(result.SolutionFileExists);
        Assert.Empty(result.ProjectsMissingFromSolution);
        Assert.Single(result.ProjectsMissingFromDisk);
        Assert.Equal(@"Project2\Project2.csproj", result.ProjectsMissingFromDisk[0]);
    }
    
    [Fact]
    public void ValidateSlnfFile_SolutionMissing_HasIssues()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Solution file is missing from file system
        
        // Add the project files
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");
        
        // Set up the .slnf file
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        
        var validator = new SlnfValidator(fileSystem, solutionParser);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.True(result.HasIssues);
        Assert.False(result.SolutionFileExists);
    }
    
    [Fact]
    public void ValidateSlnfFile_SkipSolutionCheck_OnlyChecksDisk()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Set up the solution with one project (Project2 is missing)
        solutionParser.AddSolution(SolutionPath,
        [
            @"Project1\Project1.csproj"
        ]);
        
        // Add both project files
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        fileSystem.AddFile(@"C:\test\Project2\Project2.csproj", "<Project></Project>");
        
        // Set up the .slnf file referencing both projects
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        fileSystem.AddFile(SolutionPath, ""); // Add the solution file
        
        var validator = new SlnfValidator(fileSystem, solutionParser, skipSolutionCheck: true);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.False(result.HasIssues);
        Assert.True(result.SolutionFileExists);
        Assert.Empty(result.ProjectsMissingFromSolution);
        Assert.Empty(result.ProjectsMissingFromDisk);
    }
    
    [Fact]
    public void ValidateSlnfFile_SkipDiskCheck_OnlyChecksSolution()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        var solutionParser = new MockSolutionParser();
        
        // Set up the solution with two projects
        solutionParser.AddSolution(SolutionPath,
        [
            @"Project1\Project1.csproj",
            @"Project2\Project2.csproj"
        ]);
        
        // Add only one project file (Project2 is missing)
        fileSystem.AddFile(@"C:\test\Project1\Project1.csproj", "<Project></Project>");
        
        // Set up the .slnf file referencing both projects
        var slnfContent = """
            {
              "solution": {
                "path": "test.sln",
                "projects": [
                  "Project1\\Project1.csproj",
                  "Project2\\Project2.csproj"
                ]
              }
            }
            """;
        fileSystem.AddFile(SlnfPath, slnfContent);
        fileSystem.AddFile(SolutionPath, ""); // Add the solution file
        
        var validator = new SlnfValidator(fileSystem, solutionParser, skipDiskCheck: true);
        
        // Act
        var result = validator.ValidateSlnfFile(SlnfPath);
        
        // Assert
        Assert.False(result.HasIssues);
        Assert.True(result.SolutionFileExists);
        Assert.Empty(result.ProjectsMissingFromSolution);
        Assert.Empty(result.ProjectsMissingFromDisk);
    }
    
    [Fact]
    public void FindSlnfFiles_ReturnsAllSlnfFilesInCurrentDirectory()
    {
        // Arrange
        var fileSystem = new MockFileSystem(BasePath);
        
        fileSystem.AddFile(@"C:\test\test1.slnf", "{}");
        fileSystem.AddFile(@"C:\test\test2.slnf", "{}");
        fileSystem.AddFile(@"C:\test\not-a-slnf.txt", "");
        
        var validator = new SlnfValidator(fileSystem);
        
        // Act
        var files = validator.FindSlnfFiles();
        
        // Assert
        Assert.Equal(2, files.Length);
        Assert.Contains(@"C:\test\test1.slnf", files);
        Assert.Contains(@"C:\test\test2.slnf", files);
    }
}

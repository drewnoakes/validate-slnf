using Microsoft.Build.Construction;
using ValidateSlnf.FileSystem;

namespace ValidateSlnf;

/// <summary>
/// Interface for solution parser to facilitate testing
/// </summary>
public interface ISolutionParser
{
    /// <summary>
    /// Gets all project paths in a solution
    /// </summary>
    /// <param name="solutionPath">The path to the solution file</param>
    /// <returns>A list of project paths</returns>
    /// <exception cref="FileNotFoundException">Thrown if the solution file doesn't exist</exception>
    IReadOnlyList<string> GetProjectsInSolution(string solutionPath);
}

/// <summary>
/// Parses solution files and extracts project references
/// </summary>
public class SolutionParser : ISolutionParser
{
    private readonly IFileSystem _fileSystem;
    
    /// <summary>
    /// Creates a new solution parser
    /// </summary>
    /// <param name="fileSystem">The file system to use</param>
    public SolutionParser(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    /// <inheritdoc />
    public IReadOnlyList<string> GetProjectsInSolution(string solutionPath)
    {
        if (!_fileSystem.FileExists(solutionPath))
        {
            throw new FileNotFoundException($"Solution file not found: {solutionPath}", solutionPath);
        }
        
        var solutionFile = SolutionFile.Parse(solutionPath);
        var solutionDirectory = _fileSystem.GetDirectoryName(solutionPath);
        
        return solutionFile.ProjectsInOrder
            .Where(p => !p.ProjectType.Equals(SolutionProjectType.SolutionFolder))
            .Select(p => GetNormalizedProjectPath(p.AbsolutePath, solutionDirectory))
            .ToList();
    }
    
    private string GetNormalizedProjectPath(string absolutePath, string solutionDirectory)
    {
        if (absolutePath.StartsWith(solutionDirectory, StringComparison.OrdinalIgnoreCase))
        {
            // Make the path relative to the solution directory
            return absolutePath.Substring(solutionDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
        }
        
        return absolutePath;
    }
}

using System.Text.Json;
using ValidateSlnf.FileSystem;
using ValidateSlnf.Models;

namespace ValidateSlnf;

/// <summary>
/// Validates .slnf files
/// </summary>
public class SlnfValidator
{
    private readonly IFileSystem _fileSystem;
    private readonly ISolutionParser _solutionParser;
    private readonly bool _skipSolutionCheck;
    private readonly bool _skipDiskCheck;
    
    /// <summary>
    /// Creates a new .slnf validator
    /// </summary>
    /// <param name="fileSystem">The file system to use</param>
    /// <param name="skipSolutionCheck">Whether to skip checking if projects exist in the parent solution</param>
    /// <param name="skipDiskCheck">Whether to skip checking if projects exist on disk</param>
    public SlnfValidator(IFileSystem fileSystem, bool skipSolutionCheck = false, bool skipDiskCheck = false)
        : this(fileSystem, new SolutionParser(fileSystem), skipSolutionCheck, skipDiskCheck)
    {
    }
    
    /// <summary>
    /// Creates a new .slnf validator with a custom solution parser
    /// </summary>
    /// <param name="fileSystem">The file system to use</param>
    /// <param name="solutionParser">The solution parser to use</param>
    /// <param name="skipSolutionCheck">Whether to skip checking if projects exist in the parent solution</param>
    /// <param name="skipDiskCheck">Whether to skip checking if projects exist on disk</param>
    public SlnfValidator(IFileSystem fileSystem, ISolutionParser solutionParser, bool skipSolutionCheck = false, bool skipDiskCheck = false)
    {
        _fileSystem = fileSystem;
        _solutionParser = solutionParser;
        _skipSolutionCheck = skipSolutionCheck;
        _skipDiskCheck = skipDiskCheck;
    }
    
    /// <summary>
    /// Finds all .slnf files in the current directory
    /// </summary>
    /// <returns>An array of .slnf file paths</returns>
    public string[] FindSlnfFiles()
    {
        var currentDirectory = _fileSystem.GetCurrentDirectory();
        return _fileSystem.GetFiles(currentDirectory, "*.slnf");
    }
    
    /// <summary>
    /// Validates a .slnf file
    /// </summary>
    /// <param name="slnfFilePath">The path to the .slnf file</param>
    /// <returns>The validation result</returns>
    public ValidationResult ValidateSlnfFile(string slnfFilePath)
    {
        if (!_fileSystem.FileExists(slnfFilePath))
        {
            throw new FileNotFoundException($"SLNF file not found: {slnfFilePath}", slnfFilePath);
        }
        
        var slnfJson = _fileSystem.ReadAllText(slnfFilePath);
        var slnf = JsonSerializer.Deserialize<SlnfFile>(slnfJson) ?? 
            throw new InvalidOperationException($"Failed to parse SLNF file: {slnfFilePath}");
        
        var slnfDirectory = _fileSystem.GetDirectoryName(slnfFilePath);
        var solutionPath = _fileSystem.Combine(slnfDirectory, slnf.Solution.Path);
        solutionPath = _fileSystem.GetFullPath(solutionPath);
        
        var result = new ValidationResult(slnfFilePath, solutionPath, slnf);
        
        // Check if the solution file exists
        if (!_fileSystem.FileExists(solutionPath))
        {
            result.SolutionFileExists = false;
            return result;
        }
        
        // Check if projects in .slnf exist in the parent solution
        if (!_skipSolutionCheck)
        {
            ValidateProjectsExistInSolution(slnf, solutionPath, result);
        }
        
        // Check if project files exist on disk
        if (!_skipDiskCheck)
        {
            ValidateProjectsExistOnDisk(slnf, solutionPath, result);
        }
        
        return result;
    }
    
    private void ValidateProjectsExistInSolution(SlnfFile slnf, string solutionPath, ValidationResult result)
    {
        try
        {
            var solutionProjects = _solutionParser.GetProjectsInSolution(solutionPath);
            var solutionDirectory = _fileSystem.GetDirectoryName(solutionPath);
            
            foreach (var project in slnf.Solution.Projects)
            {
                var normalizedProject = NormalizeProjectPath(project);
                
                if (!solutionProjects.Any(sp => string.Equals(NormalizeProjectPath(sp), normalizedProject, StringComparison.OrdinalIgnoreCase)))
                {
                    result.ProjectsMissingFromSolution.Add(project);
                }
            }
        }
        catch (Exception ex)
        {
            // If we can't parse the solution, consider it as not existing
            result.SolutionFileExists = false;
            Console.Error.WriteLine($"Error parsing solution file: {ex.Message}");
        }
    }
    
    private void ValidateProjectsExistOnDisk(SlnfFile slnf, string solutionPath, ValidationResult result)
    {
        var solutionDirectory = _fileSystem.GetDirectoryName(solutionPath);
        
        foreach (var project in slnf.Solution.Projects)
        {
            var projectPath = _fileSystem.Combine(solutionDirectory, project);
            if (!_fileSystem.FileExists(projectPath))
            {
                result.ProjectsMissingFromDisk.Add(project);
            }
        }
    }
    
    private string NormalizeProjectPath(string projectPath)
    {
        // Normalize path separators
        return projectPath.Replace('/', Path.DirectorySeparatorChar)
                         .Replace('\\', Path.DirectorySeparatorChar)
                         .TrimStart(Path.DirectorySeparatorChar);
    }
}

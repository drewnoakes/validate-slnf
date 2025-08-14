namespace ValidateSlnf.Models;

/// <summary>
/// Represents the result of validating a .slnf file
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// The path to the .slnf file that was validated
    /// </summary>
    public string SlnfFilePath { get; }
    
    /// <summary>
    /// The path to the parent solution file
    /// </summary>
    public string SolutionFilePath { get; }
    
    /// <summary>
    /// The parsed .slnf file object
    /// </summary>
    public SlnfFile SlnfFile { get; }
    
    /// <summary>
    /// List of projects that exist in the .slnf but not in the parent solution
    /// </summary>
    public List<string> ProjectsMissingFromSolution { get; } = new();
    
    /// <summary>
    /// List of projects that exist in the .slnf but not on disk
    /// </summary>
    public List<string> ProjectsMissingFromDisk { get; } = new();
    
    /// <summary>
    /// Indicates if the solution file was found
    /// </summary>
    public bool SolutionFileExists { get; set; } = true;
    
    /// <summary>
    /// Creates a new validation result
    /// </summary>
    /// <param name="slnfFilePath">The path to the .slnf file</param>
    /// <param name="solutionFilePath">The path to the parent solution file</param>
    /// <param name="slnfFile">The parsed .slnf file object</param>
    public ValidationResult(string slnfFilePath, string solutionFilePath, SlnfFile slnfFile)
    {
        SlnfFilePath = slnfFilePath;
        SolutionFilePath = solutionFilePath;
        SlnfFile = slnfFile;
    }
    
    /// <summary>
    /// Indicates if any issues were found during validation
    /// </summary>
    public bool HasIssues => !SolutionFileExists || 
                            ProjectsMissingFromSolution.Count > 0 || 
                            ProjectsMissingFromDisk.Count > 0;
}

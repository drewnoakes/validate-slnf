namespace ValidateSlnf.Tests;

/// <summary>
/// Mock solution parser for testing
/// </summary>
public class MockSolutionParser : ISolutionParser
{
    private readonly Dictionary<string, List<string>> _solutionProjects = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _existingSolutions = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Adds a solution with projects to the mock
    /// </summary>
    /// <param name="solutionPath">The path to the solution</param>
    /// <param name="projectPaths">The project paths in the solution</param>
    public void AddSolution(string solutionPath, IEnumerable<string> projectPaths)
    {
        _solutionProjects[solutionPath] = projectPaths.ToList();
        _existingSolutions.Add(solutionPath);
    }
    
    /// <inheritdoc />
    public IReadOnlyList<string> GetProjectsInSolution(string solutionPath)
    {
        if (!_existingSolutions.Contains(solutionPath))
        {
            throw new FileNotFoundException($"Solution file not found: {solutionPath}", solutionPath);
        }
        
        return _solutionProjects[solutionPath];
    }
}

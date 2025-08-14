namespace ValidateSlnf.FileSystem;

/// <summary>
/// Abstraction for file system operations to facilitate testing
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Checks if a file exists at the specified path
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    bool FileExists(string path);
    
    /// <summary>
    /// Reads all text from a file
    /// </summary>
    /// <param name="path">The path of the file to read</param>
    /// <returns>The contents of the file</returns>
    string ReadAllText(string path);
    
    /// <summary>
    /// Gets all files matching a search pattern in a directory
    /// </summary>
    /// <param name="directory">The directory to search</param>
    /// <param name="searchPattern">The search pattern to use</param>
    /// <returns>An array of file paths</returns>
    string[] GetFiles(string directory, string searchPattern);
    
    /// <summary>
    /// Gets the directory name from a path
    /// </summary>
    /// <param name="path">The path to get the directory from</param>
    /// <returns>The directory name</returns>
    string GetDirectoryName(string path);
    
    /// <summary>
    /// Combines multiple path parts into a single path
    /// </summary>
    /// <param name="paths">The path parts to combine</param>
    /// <returns>The combined path</returns>
    string Combine(params string[] paths);
    
    /// <summary>
    /// Gets the full path for a relative path
    /// </summary>
    /// <param name="path">The relative path</param>
    /// <returns>The full path</returns>
    string GetFullPath(string path);
    
    /// <summary>
    /// Gets the current directory
    /// </summary>
    /// <returns>The current directory</returns>
    string GetCurrentDirectory();
}

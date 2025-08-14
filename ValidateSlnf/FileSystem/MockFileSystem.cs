namespace ValidateSlnf.FileSystem;

/// <summary>
/// Mock file system for testing
/// </summary>
public class MockFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _fileContents = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _existingFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _currentDirectory;
    
    /// <summary>
    /// Creates a new instance of the mock file system
    /// </summary>
    /// <param name="currentDirectory">The current directory to use</param>
    public MockFileSystem(string currentDirectory = @"c:\test")
    {
        _currentDirectory = currentDirectory;
    }
    
    /// <summary>
    /// Adds a file to the mock file system
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="contents">The contents of the file</param>
    public void AddFile(string path, string contents)
    {
        var fullPath = GetFullPath(path);
        _fileContents[fullPath] = contents;
        _existingFiles.Add(fullPath);
    }
    
    /// <inheritdoc />
    public bool FileExists(string path)
    {
        var fullPath = GetFullPath(path);
        return _existingFiles.Contains(fullPath);
    }

    /// <inheritdoc />
    public string ReadAllText(string path)
    {
        var fullPath = GetFullPath(path);
        if (!_fileContents.TryGetValue(fullPath, out var contents))
        {
            throw new FileNotFoundException($"File not found: {path}", path);
        }
        
        return contents;
    }

    /// <inheritdoc />
    public string[] GetFiles(string directory, string searchPattern)
    {
        var fullDirectory = GetFullPath(directory);
        
        // Very simple implementation that only supports *.extension style patterns
        if (searchPattern.StartsWith("*"))
        {
            var extension = searchPattern.Substring(1);
            return _existingFiles
                .Where(f => f.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase) && 
                           f.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        
        return _existingFiles
            .Where(f => f.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase) && 
                       Path.GetFileName(f).Equals(searchPattern, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <inheritdoc />
    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path) ?? string.Empty;
    }

    /// <inheritdoc />
    public string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    /// <inheritdoc />
    public string GetFullPath(string path)
    {
        if (Path.IsPathFullyQualified(path))
        {
            return path;
        }
        
        return Path.GetFullPath(path, _currentDirectory);
    }

    /// <inheritdoc />
    public string GetCurrentDirectory()
    {
        return _currentDirectory;
    }
}

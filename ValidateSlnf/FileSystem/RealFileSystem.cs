namespace ValidateSlnf.FileSystem;

/// <summary>
/// Real file system implementation
/// </summary>
public class RealFileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <inheritdoc />
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <inheritdoc />
    public string[] GetFiles(string directory, string searchPattern)
    {
        return Directory.GetFiles(directory, searchPattern);
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
        return Path.GetFullPath(path);
    }

    /// <inheritdoc />
    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }
}

using ValidateSlnf.FileSystem;
using ValidateSlnf.Models;

namespace ValidateSlnf;

public class Program
{
    private static bool _verbose;
    private static bool _skipSolutionCheck;
    private static bool _skipDiskCheck;
    
    // For testing
    public static IFileSystem? _fileSystemOverride;
    public static ISolutionParser? _solutionParserOverride;

    public static int Main(string[] args)
    {
        // Parse command line arguments
        var fileList = ParseArguments(args);
        
        // Use the real file system by default, but allow override for testing
        var fileSystem = _fileSystemOverride ?? new RealFileSystem();
        
        // Create a solution parser or use the override for testing
        var solutionParser = _solutionParserOverride ?? new SolutionParser(fileSystem);
        
        var validator = new SlnfValidator(fileSystem, solutionParser, _skipSolutionCheck, _skipDiskCheck);
        
        // If no files are specified, use all .slnf files in the current directory
        if (fileList.Count == 0)
        {
            var slnfFiles = validator.FindSlnfFiles();
            
            if (slnfFiles.Length == 0)
            {
                Console.WriteLine("Warning: No .slnf files found in the current directory.");
                return 0;
            }
            
            if (_verbose)
            {
                Console.WriteLine($"Found {slnfFiles.Length} .slnf files in current directory");
            }
            
            fileList.AddRange(slnfFiles);
        }
        
        // Validate each file
        var hasIssues = false;
        var results = new List<ValidationResult>();
        
        foreach (var file in fileList)
        {
            try
            {
                if (_verbose)
                {
                    Console.WriteLine($"Processing {file}...");
                }
                
                var result = validator.ValidateSlnfFile(file);
                results.Add(result);
                
                if (result.HasIssues)
                {
                    hasIssues = true;
                }
                
                if (_verbose)
                {
                    ReportVerboseResults(result);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error validating {file}: {ex.Message}");
                hasIssues = true;
            }
        }
        
        // Output results if there are issues
        if (hasIssues && !_verbose)
        {
            ReportIssues(results);
            return 1;
        }
        
        if (_verbose && !hasIssues)
        {
            Console.WriteLine("All files validated successfully");
        }
        
        return hasIssues ? 1 : 0;
    }
    
    private static List<string> ParseArguments(string[] args)
    {
        var fileList = new List<string>();
        
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            switch (arg.ToLower())
            {
                case "--help":
                case "-h":
                case "/?":
                    DisplayHelp();
                    Environment.Exit(0);
                    break;
                
                case "--verbose":
                case "-v":
                    _verbose = true;
                    break;
                
                case "--skip-solution-check":
                case "-s":
                    _skipSolutionCheck = true;
                    break;
                
                case "--skip-disk-check":
                case "-d":
                    _skipDiskCheck = true;
                    break;
                
                default:
                    // If not a flag, treat as a file path
                    if (!arg.StartsWith("--"))
                    {
                        fileList.Add(arg);
                    }
                    else
                    {
                        Console.Error.WriteLine($"Unknown option: {arg}");
                    }
                    break;
            }
        }
        
        return fileList;
    }
    
    private static void DisplayHelp()
    {
        Console.WriteLine("""
            validate-slnf: A tool to validate that Visual Studio Solution Filter (.slnf) files have not fallen out of date

            Usage:
              validate-slnf [options] [file1.slnf file2.slnf ...]

            If no files are specified, all .slnf files in the current directory will be validated.

            Options:
              --help, -h              Display this help message
              --verbose, -v           Print detailed information about the validation process
              --skip-solution-check, -s  Skip checking if projects exist in the parent solution file (.sln or .slnx)
              --skip-disk-check, -d   Skip checking if projects exist on disk

            Exit codes:
              0: All checks passed (or no .slnf files found)
              1: One or more checks failed
            """);
    }
    
    private static void ReportVerboseResults(ValidationResult result)
    {
        if (!result.SolutionFileExists)
        {
            Console.WriteLine($"Parent solution file not found: {result.SolutionFilePath}");
            return;
        }
        
        Console.WriteLine($"Parent solution file: {result.SolutionFilePath}");
        
        // Use the SlnfFile directly from the ValidationResult
        Console.WriteLine($"Projects in .slnf file ({result.SlnfFile.Solution.Projects.Count}):");
        foreach (var project in result.SlnfFile.Solution.Projects)
        {
            Console.WriteLine($"  {project}");
        }
        Console.WriteLine();
        
        if (!_skipSolutionCheck)
        {
            if (result.ProjectsMissingFromSolution.Count > 0)
            {
                Console.WriteLine("Projects missing from parent solution:");
                foreach (var project in result.ProjectsMissingFromSolution)
                {
                    Console.WriteLine($"  {project}");
                }
            }
            else
            {
                Console.WriteLine("All projects exist in the parent solution");
            }
        }
        
        if (!_skipDiskCheck)
        {
            if (result.ProjectsMissingFromDisk.Count > 0)
            {
                Console.WriteLine("Projects missing from disk:");
                foreach (var project in result.ProjectsMissingFromDisk)
                {
                    Console.WriteLine($"  {project}");
                }
            }
            else
            {
                Console.WriteLine("All project files exist on disk");
            }
        }
    }
    
    private static void ReportIssues(List<ValidationResult> results)
    {
        foreach (var result in results.Where(r => r.HasIssues))
        {
            Console.Error.WriteLine($"Validation failed for {result.SlnfFilePath}:");
            
            if (!result.SolutionFileExists)
            {
                Console.Error.WriteLine($"  Parent solution file not found: {result.SolutionFilePath}");
                continue;
            }
            
            if (result.ProjectsMissingFromSolution.Count > 0)
            {
                Console.Error.WriteLine("  Projects missing from parent solution:");
                foreach (var project in result.ProjectsMissingFromSolution)
                {
                    Console.Error.WriteLine($"    {project}");
                }
            }
            
            if (result.ProjectsMissingFromDisk.Count > 0)
            {
                Console.Error.WriteLine("  Projects missing from disk:");
                foreach (var project in result.ProjectsMissingFromDisk)
                {
                    Console.Error.WriteLine($"    {project}");
                }
            }
            
            Console.Error.WriteLine();
        }
    }
}

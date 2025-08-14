using System.Text.Json.Serialization;

namespace ValidateSlnf.Models;

/// <summary>
/// Represents a Solution Filter file (.slnf)
/// </summary>
public class SlnfFile
{
    /// <summary>
    /// The solution information contained in the .slnf file
    /// </summary>
    [JsonPropertyName("solution")]
    public SlnfSolution Solution { get; set; } = new();

    /// <summary>
    /// Represents the solution section of the .slnf file
    /// </summary>
    public class SlnfSolution
    {
        /// <summary>
        /// The relative path to the parent solution file (.sln or .slnx)
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The list of project paths included in the solution filter
        /// </summary>
        [JsonPropertyName("projects")]
        public List<string> Projects { get; set; } = new();
    }
}

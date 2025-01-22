using System.Text.Json.Serialization;

namespace MyProject.Domain.OAuths;

public class EmailGithub
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("primary")]
    public bool Primary { get; set; }
    
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }
    
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
}
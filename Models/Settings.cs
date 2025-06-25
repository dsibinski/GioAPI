namespace GioAPI.Models;

public class Settings
{
    public required string OpenAiApiKey { get; set; }
    public required string BearerToken { get; set; }
    public required string DefaultModel { get; set; }
} 
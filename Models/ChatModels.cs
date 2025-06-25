namespace GioAPI.Models;

public class Message
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}

public class ChatRequest
{
    public required List<Message> Messages { get; set; }
    public string? Model { get; set; }
}

public class ChatResponse
{
    public required string Content { get; set; }
} 
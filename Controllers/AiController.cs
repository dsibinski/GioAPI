using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using GioAPI.Models;

namespace GioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AiController : ControllerBase
{
    private readonly ChatClient _chatClient;

    public AiController(IOptions<Settings> settings)
    {
        var settingsValue = settings.Value;
        _chatClient = new ChatClient(settingsValue.DefaultModel, settingsValue.OpenAiApiKey);
    }

    [HttpPost("Chat")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var chatMessages = new List<ChatMessage>();
            
            foreach (var message in request.Messages)
            {
                var role = message.Role.ToLowerInvariant();
                if (role == "system")
                {
                    chatMessages.Add(new SystemChatMessage(message.Content));
                }
                else if (role == "assistant")
                {
                    chatMessages.Add(new AssistantChatMessage(message.Content));
                }
                else
                {
                    chatMessages.Add(new UserChatMessage(message.Content));
                }
            }

            ChatCompletion completion = await _chatClient.CompleteChatAsync(chatMessages.ToArray());
            return Ok(new ChatResponse { Content = completion.Content[0].Text });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 
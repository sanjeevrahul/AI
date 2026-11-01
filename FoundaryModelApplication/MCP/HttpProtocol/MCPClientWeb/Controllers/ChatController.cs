using Microsoft.AspNetCore.Mvc;
using McpWebClient.Services;

namespace McpWebClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(
        IChatService chatService)
    {
        _chatService = chatService;
    }


    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.Message))
        {
            return BadRequest(
                "Message cannot be empty.");
        }


        var conversationId =
            string.IsNullOrWhiteSpace(
                request.ConversationId)
            ? Guid.NewGuid().ToString()
            : request.ConversationId;


        var response =
            await _chatService.SendMessageAsync(
                conversationId,
                request.Message,
                cancellationToken);


        return Ok(response);
    }
}
public sealed record ChatRequest(
    string? ConversationId,
    string Message);
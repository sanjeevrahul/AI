namespace McpWebClient.Services;

public interface IChatService
{
Task InitialiseAsync(
        CancellationToken cancellationToken = default);
    Task<ChatResponse> SendMessageAsync(
        string conversationId,
        string message,
        CancellationToken cancellationToken = default);
}

public record ChatResponse(
    string ConversationId,
    string Message);
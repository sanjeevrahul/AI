using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace LamaModelConsoleApp;

public sealed class OllamaHttpChatClient : IChatClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaHttpChatClient(
        HttpClient httpClient,
        string model)
    {
        _httpClient = httpClient;
        _model = model;
    }

    public ChatClientMetadata Metadata =>
        new("ollama", new Uri(_httpClient.BaseAddress!, "/"));

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaRequest
        {
            Model = _model,
            Stream = false,
            Messages = messages
                .Select(m => new OllamaMessage
                {
                    Role = MapRole(m.Role),
                    Content = GetText(m)
                })
                .ToList()
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/chat",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaResponse>(
                    cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException(
                "Ollama returned an empty response.");
        }

        var assistantMessage =
            new ChatMessage(
                ChatRole.Assistant,
                result.Message?.Content ?? string.Empty);

        return new ChatResponse(assistantMessage);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaRequest
        {
            Model = _model,
            Stream = true,
            Messages = messages
                .Select(m => new OllamaMessage
                {
                    Role = MapRole(m.Role),
                    Content = GetText(m)
                })
                .ToList()
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/chat");

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var reader =
            new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line =
                await reader.ReadLineAsync(
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var chunk =
                JsonSerializer.Deserialize<OllamaResponse>(
                    line);

            if (chunk?.Message?.Content == null)
                continue;

            yield return new ChatResponseUpdate
            {
                Contents =
                [
                    new TextContent(
                        chunk.Message.Content)
                ]
            };

            if (chunk.Done)
                yield break;
        }
    }

    public object? GetService(
        Type serviceType,
        object? serviceKey = null)
    {
        if (serviceType == typeof(OllamaHttpChatClient))
            return this;

        return null;
    }

    public void Dispose()
    {
        // HttpClient is owned by the application.
    }

    private static string MapRole(
        ChatRole role)
    {
        if (role == ChatRole.System)
            return "system";

        if (role == ChatRole.User)
            return "user";

        if (role == ChatRole.Assistant)
            return "assistant";

        return "user";
    }

    private static string GetText(
        ChatMessage message)
    {
        return string.Join(
            "",
            message.Contents
                .OfType<TextContent>()
                .Select(x => x.Text));
    }

    private sealed class OllamaRequest
    {
        public string Model { get; set; } = "";

        public bool Stream { get; set; }

        public List<OllamaMessage> Messages { get; set; } = [];
    }

    private sealed class OllamaMessage
    {
        public string Role { get; set; } = "";

        public string Content { get; set; } = "";
    }

    private sealed class OllamaResponse
    {
        public OllamaMessage? Message { get; set; }

        public bool Done { get; set; }
    }
}
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI;
using OpenAI.Responses;
using System.ClientModel;

var builder = WebApplication.CreateSlimBuilder(args);

// JSON source generation
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0,
        AppJsonSerializerContext.Default);
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

#pragma warning disable OPENAI001

// ============================================================
// Azure AI Foundry / OpenAI
// ============================================================

const string deploymentName = "gpt-5.4-mini";

var endpoint = builder.Configuration["FOUNDRY_ENDPOINT"];
if (string.IsNullOrWhiteSpace(endpoint))
{
    throw new InvalidOperationException(
        "Required environment variable FOUNDRY_ENDPOINT is not set.");
}

var apiKey = builder.Configuration["AZURE_OPENAI_API_KEY"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        "Required environment variable FOUNDRY_API_KEY is not set.");
}

ResponsesClient client = new(
    credential: new ApiKeyCredential(apiKey),
    options: new ResponsesClientOptions
    {
        Endpoint = new Uri(endpoint)
    });


// ============================================================
// Azure AI Foundry API
// ============================================================

var foundaryAPI = app.MapGroup("/AI");


// GET /AI
foundaryAPI.MapGet("/", () =>
{
    string request = "about ireland";

    CreateResponseOptions options = new()
    {
        Model = deploymentName,
        InputItems =
        {
            ResponseItem.CreateUserMessageItem(request)
        }
    };

    var response = client.CreateResponse(options);

    return response.Value.GetOutputText();
})
.WithName("GetDefaultResponse");


// GET /AI/{request}
//
// Example:
// http://localhost:5122/AI/about%20ireland
//
foundaryAPI.MapGet("/{request}", (string request) =>
{
    CreateResponseOptions options = new()
    {
        Model = deploymentName,
        InputItems =
        {
            ResponseItem.CreateUserMessageItem(request)
        }
    };

    var response = client.CreateResponse(options);

    return response.Value.GetOutputText();
})
.WithName("AskAI");


// POST /AI
//
// Body:
// {
//     "message": "Tell me about Ireland"
// }
//
foundaryAPI.MapPost("/", (ChatRequest request) =>
{
    CreateResponseOptions options = new()
    {
        Model = deploymentName,
        InputItems =
        {
            ResponseItem.CreateUserMessageItem(request.Message)
        }
    };

    var response = client.CreateResponse(options);

    return response.Value.GetOutputText();
})
.WithName("AskAIPost");


// POST /AI/{request}
//
// Example:
// POST /AI/about%20ireland
//
foundaryAPI.MapPost("/{request}", (string request) =>
{
    CreateResponseOptions options = new()
    {
        Model = deploymentName,
        InputItems =
        {
            ResponseItem.CreateUserMessageItem(request)
        }
    };

    var response = client.CreateResponse(options);

    return response.Value.GetOutputText();
})
.WithName("AskAIPostRoute");


// ============================================================
// Azure Translator
// ============================================================

const string endpointTranslation =
    "https://api.cognitive.microsofttranslator.com";

const string apiVersion = "2025-10-01-preview";

const string region = "westeurope";
var translationApi = app.MapGroup("/translator");


// POST /translator
//
// Request:
//
// {
//     "text": "Doctor is available next Monday.",
//     "targetLanguage": "es"
// }
//
translationApi.MapPost("/", async (TranslationRequest request) =>
{
    var subscriptionKey = builder.Configuration["AZURE_OPENAI_API_KEY"];
    if (string.IsNullOrWhiteSpace(subscriptionKey))
    {
        throw new InvalidOperationException(
            "Required environment variable TRANSLATOR_SUBSCRIPTION_KEY is not set.");
    }

    // Build the Azure Translator request
    var translatorRequest = new TranslatorRequest(
        new[]
        {
            new TranslatorInput(
                request.Text,
                "en",
                new[]
                {
                    new TranslatorTarget(
                        request.TargetLanguage)
                })
        });


    // Azure Translator URL
    var url =
        $"{endpointTranslation}/translate?api-version={apiVersion}";


    using var clientTranslation = new HttpClient();


    // Authentication
    clientTranslation.DefaultRequestHeaders.Add(
        "Ocp-Apim-Subscription-Key",
        subscriptionKey);

    clientTranslation.DefaultRequestHeaders.Add(
        "Ocp-Apim-Subscription-Region",
        region);


    // IMPORTANT:
    // Use source-generated JSON metadata.
    //
    var jsonRequest = JsonSerializer.Serialize(
        translatorRequest,
        AppJsonSerializerContext.Default.TranslatorRequest);


    Console.WriteLine("Translator Request:");
    Console.WriteLine(jsonRequest);


    using var content = new StringContent(
        jsonRequest,
        Encoding.UTF8,
        "application/json");


    // Call Azure Translator
    var responseTranslation =
        await clientTranslation.PostAsync(
            url,
            content);


    var responseJson =
        await responseTranslation.Content.ReadAsStringAsync();


    Console.WriteLine(
        $"Translator Status: {responseTranslation.StatusCode}");

    Console.WriteLine(
        $"Translator Response: {responseJson}");


    // Return Translator's JSON directly
    return Results.Content(
        responseJson,
        "application/json");
})
.WithName("TranslateText");


app.Run();


// ============================================================
// Request models
// ============================================================

public record ChatRequest(
    string Message);


public record TranslationRequest(
    string Text,
    string TargetLanguage);


// ============================================================
// Azure Translator models
// ============================================================

public record TranslatorRequest(
    [property: JsonPropertyName("inputs")]
    TranslatorInput[] Inputs);


public record TranslatorInput(
    [property: JsonPropertyName("text")]
    string Text,

    [property: JsonPropertyName("language")]
    string Language,

    [property: JsonPropertyName("targets")]
    TranslatorTarget[] Targets);


public record TranslatorTarget(
    [property: JsonPropertyName("language")]
    string Language);


// ============================================================
// JSON Source Generation
// ============================================================

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatRequest[]))]

[JsonSerializable(typeof(TranslationRequest))]

[JsonSerializable(typeof(TranslatorRequest))]
[JsonSerializable(typeof(TranslatorInput))]
[JsonSerializable(typeof(TranslatorInput[]))]
[JsonSerializable(typeof(TranslatorTarget))]
[JsonSerializable(typeof(TranslatorTarget[]))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
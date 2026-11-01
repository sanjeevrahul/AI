# FoundaryModelUsingWebAPI

Configure the Azure AI Foundry connection with environment variables before starting the application:

```powershell
$env:FOUNDRY_ENDPOINT = "https://<resource>.services.ai.azure.com/openai/v1"
$env:FOUNDRY_API_KEY = "<your-foundry-api-key>"
dotnet run
```

The endpoint should be the OpenAI-compatible endpoint for your Foundry resource. The deployment name (`gpt-5.4-mini`) is configured in `Program.cs`.

The `/translator` endpoint additionally requires `TRANSLATOR_SUBSCRIPTION_KEY`; set it to your Azure Translator key before calling that endpoint:

```powershell
$env:TRANSLATOR_SUBSCRIPTION_KEY = "<your-translator-key>"
```

Do not commit real API keys to source control.

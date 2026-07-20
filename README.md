# Cyber-Chan

My discord bot.

## Development

### Building

This project uses .NET 8.0. To build:

```bash
dotnet restore
dotnet build --configuration Release
```

### CI/CD

The project uses GitHub Actions for continuous integration and deployment:

- **CI Workflow** (`ci.yml`): Runs on pushes to `master` and `test` branches, and on pull requests to `master`
- **Deploy Workflow** (`deploy.yml`): Handles deployment to test and production environments
- **Code Quality** (`code-quality.yml`): Runs code quality checks on pull requests

### Dependencies

Dependencies are automatically updated weekly via Dependabot.

### OpenAI model configuration

Chat and auxiliary model selection is configurable through `appSettings` values (for example in `CyberChan.dll.config`):

- `OpenAIModelGPT3Prompt` (default: `gpt-4.1-mini`)
- `OpenAIModelChatPrompt` (default: `gpt-4.1-mini`)
- `OpenAIModelGPT4Prompt` (default: `gpt-4.1`)
- `OpenAIModelGPT4PreviewPrompt` (default: `gpt-4.1-mini`)
- `OpenAIModelGPT4OmniPrompt` (default: `gpt-4o-mini`)
- `OpenAIModelGPTO1Prompt` (default: `o1-mini`)
- `OpenAIModelO4MiniPrompt` (default: `gpt-4o-mini`)
- `OpenAIModelGPT41NanoPrompt` (default: `gpt-4.1-mini`)
- `OpenAIModelGPT41Prompt` (default: `gpt-4.1`)
- `OpenAIModelO3Prompt` (default: `o3`)
- `OpenAIModelGPT52Prompt` (default: `gpt-5.2`)
- `OpenAIModelVisionPrompt` (default: `gpt-4o-mini`)
- `OpenAIModelImageGeneration` (default: `gpt-image-1`)
- `OpenAIModelImageEdit` (default: `gpt-image-1.5`)

Deprecated values such as `text-davinci-003`, `gpt-3.5-turbo-16k`, `gpt-4`, and `gpt-4-turbo-preview` are automatically mapped to supported models.
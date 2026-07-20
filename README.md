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

Model selection is configurable through `appSettings` values (for example in `CyberChan.dll.config`).

#### Chat and reasoning use-cases

- **General chat**
  - `OpenAIModelGPT3Prompt` (default: `gpt-5.2`)
  - `OpenAIModelChatPrompt` (default: `gpt-5.2`)
  - `OpenAIModelGPT4PreviewPrompt` (default: `gpt-5.2`)
  - `OpenAIModelGPT41NanoPrompt` (default: `gpt-5.2`)
- **High-context / quality chat**
  - `OpenAIModelGPT4Prompt` (default: `gpt-5.2`)
  - `OpenAIModelGPT41Prompt` (default: `gpt-5.2`)
  - `OpenAIModelGPT52Prompt` (default: `gpt-5.2`)
- **Multimodal and reasoning**
  - `OpenAIModelGPT4OmniPrompt` (default: `gpt-5.2`)
  - `OpenAIModelO4MiniPrompt` (default: `gpt-5.2`)
  - `OpenAIModelGPTO1Prompt` (default: `o3`)
  - `OpenAIModelO3Prompt` (default: `o3`)

#### Vision and image use-cases

- `OpenAIModelVisionPrompt` (default: `gpt-4o`)
- `OpenAIModelImageGeneration` (default: `gpt-image-1.5`)
- `OpenAIModelImageEdit` (default: `gpt-image-1.5`)

Deprecated values such as `text-davinci-003`, `gpt-3.5-turbo`, `gpt-3.5-turbo-16k`, `gpt-4`, `gpt-4-turbo-preview`, and `gpt-4o-mini` are automatically mapped to supported models.
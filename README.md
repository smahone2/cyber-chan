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

- **Simple / fast chat** (no seed support)
  - `OpenAIModelSimplePrompt` (default: `gpt-4.1-nano`)
- **General chat**
  - `OpenAIModelFastPrompt` (default: `gpt-4.1-mini`)
  - `OpenAIModelBalancedPrompt` (default: `gpt-4.1`)
  - `OpenAIModelNanoPrompt` (default: `gpt-4.1-nano`)
- **High-context / quality chat**
  - `OpenAIModelDeepContextPrompt` (default: `gpt-4.1`)
  - `OpenAIModelHighQualityPrompt` (default: `gpt-4.1`)
  - `OpenAIModelFrontierPrompt` (default: `gpt-4.1`)
- **Multimodal**
  - `OpenAIModelMultimodalPrompt` (default: `gpt-4o`)
- **Reasoning**
  - `OpenAIModelFastReasoningPrompt` (default: `o4-mini-2025-04-16`)
  - `OpenAIModelReasoningPrompt` (default: `o3-2025-04-16`)
  - `OpenAIModelDeepReasoningPrompt` (default: `o3-2025-04-16`)

#### Vision and image use-cases

- `OpenAIModelVision` (default: `gpt-4o`)
- `OpenAIModelImageGeneration` (default: `gpt-image-1-mini`)
- `OpenAIModelImageEdit` (default: `gpt-image-1`)
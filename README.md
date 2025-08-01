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
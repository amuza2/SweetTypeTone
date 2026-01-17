# Contributing to SweetTypeTone

Thank you for your interest in contributing to SweetTypeTone! 🎉

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Guidelines](#coding-guidelines)
- [Commit Messages](#commit-messages)
- [Pull Request Process](#pull-request-process)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/amuza2/SweetTypeTone.git`
3. Create a new branch: `git checkout -b feature/your-feature-name`

## Development Setup

### Prerequisites

- .NET 10.0 SDK or later
- Linux (primary development platform)
- OpenAL libraries (for audio)
- Input device access (for keyboard monitoring)

### Building

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/SweetTypeTone.csproj
```

### Project Structure

```
SweetTypeTone/
├── src/
│   ├── SweetTypeTone/           # Main Avalonia UI application
│   └── SweetTypeTone.Core/      # Core business logic and services
├── .github/                      # GitHub workflows and templates
└── README.md
```

## How to Contribute

### Reporting Bugs

Use the [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.yml) to report bugs. Please include:

- Clear description of the issue
- Steps to reproduce
- Expected vs actual behavior
- System information (OS, .NET version)
- Logs or error messages

### Suggesting Features

Use the [Feature Request template](.github/ISSUE_TEMPLATE/feature_request.yml) to suggest new features. Please include:

- Problem statement
- Proposed solution
- Use case
- Any mockups or examples

### Contributing Code

1. **Find an issue** to work on or create a new one
2. **Comment** on the issue to let others know you're working on it
3. **Fork and clone** the repository
4. **Create a branch** for your changes
5. **Make your changes** following our coding guidelines
6. **Test thoroughly** on your platform
7. **Submit a pull request** using our template

## Coding Guidelines

### C# Style

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for I/O operations

### Code Organization

- Keep business logic in `SweetTypeTone.Core`
- Keep UI code in `SweetTypeTone`
- Use dependency injection for services
- Follow MVVM pattern for UI code

### Example

```csharp
/// <summary>
/// Loads a sound pack asynchronously
/// </summary>
/// <param name="soundPack">The sound pack to load</param>
public async Task LoadSoundPackAsync(SoundPack soundPack)
{
    // Implementation
}
```

## Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(audio): add parallel loading for sound packs

Implement parallel loading using Parallel.ForEach to improve
loading performance for file-based sound packs.

Closes #123
```

```
fix(ui): prevent selection of unsupported sound packs

Disable ComboBoxItems for unsupported MP3 packs to prevent
users from selecting them.

Fixes #456
```

## Pull Request Process

1. **Update documentation** if you've changed APIs or added features
2. **Add tests** if applicable
3. **Ensure all tests pass** and the project builds successfully
4. **Update CHANGELOG.md** with your changes
5. **Fill out the PR template** completely
6. **Request review** from maintainers
7. **Address feedback** promptly

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings introduced
- [ ] Tested on target platform(s)
- [ ] CHANGELOG.md updated

## Testing

### Manual Testing

Test your changes on:
- Different sound packs (OGG, WAV)
- Different keyboard layouts
- System tray functionality
- Settings persistence

### Platform Testing

If possible, test on:
- Ubuntu/Debian
- Fedora/RHEL
- Arch Linux
- Other Linux distributions

## Questions?

Feel free to:
- Ask in an existing issue
- Reach out to maintainers

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to SweetTypeTone! 🎹✨

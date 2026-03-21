# AGENTS.md - AI Test Project

## Project Overview

- **Project Type**: Godot Game (C#) + Core Library
- **Language**: C# 12
- **Target Framework**: net8.0
- **Projects**: AiTest.Core (logic), AiTest.Godot (game UI)

## Build Commands

```bash
# Build all projects
dotnet build

# Build Godot project specifically
dotnet build src/GodotGame/AiTest.Godot.csproj

# Build in Release mode
dotnet build -c Release

# Clean and rebuild
dotnet clean && dotnet build

# Restore dependencies
dotnet restore
```

## Code Analysis

Code analysis is configured via:
- `.editorconfig` - code style and formatting rules
- `Directory.Build.props` - analyzer configuration

### Primary Analysis: dotnet build
**Always run `dotnet build` first** - it shows all errors, warnings, and analyzer diagnostics.

### Enabled Analyzers
- **Microsoft.CodeAnalysis.NetAnalyzers** - built into .NET SDK
- **Roslynator.Analyzers** - code style and best practices

### Key Rules
- Code style warnings are **NOT** treated as errors (`EnforceCodeStyleInBuild=false`)
- Nullable reference annotations (`CS8632`) are suppressed
- Warnings don't block builds

### Format Checking
```bash
# Check code formatting (requires dotnet format)
dotnet format --verify-no-changes src/GodotGame/AiTest.Godot.csproj
```

### LSP Configuration
- **csharp-ls** installed via `dotnet tool install --global csharp-ls`
- Configuration in `opencode.json` uses `dotnet tool run csharp-ls`
- LSP may have slow initialization (3-4 seconds) on C# projects
- For real-time diagnostics, use VS Code with C# extension

## Test Commands

```bash
# Run all tests
dotnet test

# Run a single test (by fully qualified name)
dotnet test --filter "FullyQualifiedName~GameEngineTests.MovePlayer_ShouldUpdatePosition"

# Run tests with verbose output
dotnet test -v n

# Run tests and show output immediately
dotnet test --logger "console;verbosity=detailed"
```

## Code Style Guidelines

### General Conventions
- **Framework**: Use standard .NET 10 libraries, avoid third-party dependencies unless necessary
- **Nullable**: All reference types must use nullable reference types (`string?`, `List<T>?`)
- **Implicit Usings**: Enabled - do not add explicit `using` statements for System namespaces

### Naming Conventions
- **Classes/Methods/Properties**: PascalCase (`Player`, `MovePlayer`, `CurrentPosition`)
- **Private fields**: camelCase with underscore prefix (`_player`, `_gameState`)
- **Constants**: PascalCase (`MaxPlayers`, `DefaultTimeout`)
- **Interfaces**: Start with `I` (`IPlayer`, `IGameEngine`)
- **Files**: Match class name (`Player.cs`, `GameEngine.cs`)

### Code Organization
- One public class per file
- Files grouped by feature/namespace
- Place classes in appropriate namespaces: `AiTest.Core.{Feature}`

### Types and Variables
- Use `var` for local variables when type is obvious
- Prefer explicit types for public API members
- Use primitive types (`int`, `string`) over boxed types
- Avoid `dynamic` unless absolutely necessary

### Error Handling
- Use exceptions for exceptional conditions
- Provide meaningful exception messages
- Catch specific exceptions, not base `Exception`
- Use `ArgumentNullException.ThrowIfNull()` for null validation
- Validate input parameters at public API boundaries

### Properties and Methods
- Use expression-bodied members for simple getters: `public int Score => _score;`
- Use auto-properties when no backing field logic needed
- Keep methods focused (single responsibility)
- Use records for immutable data transfer objects

### Comments and Documentation
- Add XML documentation for public APIs (`/// <summary>`)
- Document nullability contracts in docs
- No comments for obvious code; comment the "why", not the "what"

### Formatting (enforced by dotnet format)
- 4 spaces for indentation (no tabs)
- Opening brace on new line for namespaces/classes, same line for methods
- Use file-scoped namespaces: `namespace AiTest.Core.Game;`
- One blank line between top-level elements

## Project Structure

```
AiTest.Core/
├── AiTest.Core.csproj
├── Game/              # Game logic components
└── Models/            # Data models and DTOs

src/GodotGame/
├── scripts/           # Game scripts (Robot, Tile, MapRenderer, etc.)
├── Tiles/             # Tile scenes and scripts
├── Animations/        # Animation classes
├── Services/          # Game services
└── project.godot      # Godot project config
```

## Testing

- Use xUnit for unit tests (recommended for .NET)
- Test file naming: `{ClassName}Tests.cs`
- Test method naming: `{MethodName}_{Scenario}_{ExpectedResult}`
- Place tests in separate test project: `AiTest.Core.Tests`

## Common Tasks

### Add a new class
1. Create new .cs file in appropriate folder
2. Add namespace matching folder structure
3. Make class `public` if part of API

### Run a specific test
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

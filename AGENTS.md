# AGENTS.md - AI Test Project

## Project Overview

- **Project Type**: Godot Game (C#) + Core Library
- **Language**: C# 12
- **Target Framework**: net8.0
- **Projects**: AiTest.Core (logic), AiTest.Godot (game UI)

## Documentation First

**Перед тем как писать код или принимать решения - ЗАДУМАЙСЯ о документации!**

Godot имеет обширную документацию в `C:\projects\godot\docs`. Для большинства задач там уже есть ответы:

- Нужна UI кнопка или контейнер? → `tutorials/ui/gui_containers.html`, `class_button.html`
- Не понимаешь как работает сигнал? → `tutorials/scripting/c_sharp/c_sharp_signals.html`
- Сложности с архитектурой? → `tutorials/best_practices/`
- Проблема с вводом? → `tutorials/inputs/inputevent.html`
- Не знаешь как сделать модальное окно? → `class_popup.html`, `class_window.html`

Всегда сначала проверяй документацию, особенно в папках:
- `tutorials/scripting/c_sharp/` - C# в Godot
- `tutorials/best_practices/` - лучшие практики
- `tutorials/ui/` - UI системы
- `classes/` - API классов Godot

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

# Run tests with Godot runtime support (for GdUnit4Net tests)
# Windows:
set GODOT_BIN=C:\Program Files (x86)\godot\Godot_v4.6.1-stable_mono_win64.exe
dotnet test

# Linux/macOS:
export GODOT_BIN=/path/to/Godot_v4.x-stable_mono_linux.x86_64
dotnet test
```

## Code Style Guidelines

### General Conventions (based on Godot C# Style Guide)
- **Framework**: Use standard .NET 10 libraries, avoid third-party dependencies unless necessary
- **Nullable**: All reference types must use nullable reference types (`string?`, `List<T>?`)
- **Implicit Usings**: Enabled - do not add explicit `using` statements for System namespaces

### Formatting (Allman Style)
- Use **4 spaces** for indentation (soft tabs), not tabs
- Use **LF** line endings, not CRLF
- Use **UTF-8** without BOM
- Consider breaking line if longer than **100 characters**
- **Braces on new line** for namespaces, classes, control statements:
  ```csharp
  if (x > 0)
  {
      DoSomething();
  }
  ```
- **Blank line** after `using` statements
- **Blank line** between methods, properties, inner type declarations
- **No blank line** after `{`, before `}`, after comments, adjacent to another blank line

### Naming Conventions (PascalCase)
- **Classes/Methods/Properties/Constants/Events**: PascalCase (`Player`, `MovePlayer`, `DefaultSpeed`)
- **Private fields**: camelCase with underscore prefix (`_player`, `_gameState`)
- **Local variables/Method arguments**: camelCase (`attackStrength`, `targetFound`)
- **Interfaces**: Start with `I` (`IPlayer`, `IGameEngine`)
- **Two-letter acronyms**: uppercase in PascalCase (`UIManager`), lowercase otherwise (`uiManager`)
- **`Id`** not `ID`
- **Files**: Match class name (`Player.cs`, `GameEngine.cs`)
- Avoid type prefix (`string strText` → `string text`)

### Types and Variables
- Use `var` when type is **obvious from right side** of assignment:
  ```csharp
  var direction = new Vector2(1, 0);  // OK
  var value = GetValue();             // Avoid - type not obvious
  ```
- Declare local variables **as close to first use** as possible
- Use explicit types for **public API members** and **numeric values** (especially with `real_t`)
- Prefer **properties** over non-private fields

### Error Handling
- Use exceptions for exceptional conditions
- Provide meaningful exception messages
- Catch specific exceptions, not base `Exception`
- Use `ArgumentNullException.ThrowIfNull()` for null validation
- Validate input parameters at public API boundaries
- Use **safe cast** (`as`) when possibility of different type, **direct cast** (`(Type)`) otherwise

### Properties and Methods
- Use expression-bodied members for simple getters: `public int Score => _score;`
- Use auto-properties when no backing field logic needed
- Keep methods focused (single responsibility)
- Use records for immutable data transfer objects
- Use explicit access modifiers (don't omit `public/private`)

### Comments and Documentation
- Add XML documentation for public APIs (`/// <summary>`)
- Document nullability contracts in docs
- No comments for obvious code; comment the "why", not the "what"

### Modifiers Order
`public`/`protected`/`private`/`internal` → `virtual` → `override` → `abstract` → `new` → `static` → `readonly`

### Other
- Avoid using fully-qualified names or `this.` prefix when not necessary
- Remove unused `using` statements and unnecessary parentheses
- Consider using null-conditional operators (`?.`) for cleaner code

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

### Test Projects Structure
```
Tests/
├── AiTest.Core.Tests/        # Pure logic tests (xUnit)
└── AiTest.Godot.Tests/       # Godot-specific tests
    ├── ExecutionPhase/       # Execution phase tests
    ├── GameFlow/             # Game flow integration tests
    ├── Messages/             # Message dispatcher tests
    ├── Phases/               # Phase tests
    ├── GdUnit/               # GdUnit4Net tests (RequireGodotRuntime)
    └── GlobalUsings.cs       # Common usings (FluentAssertions, Godot, etc.)
```

### Testing Frameworks
- **xUnit** - for unit tests
- **FluentAssertions** - for readable assertions (`.Should().Be()`)
- **GdUnit4Net** - for Godot runtime integration tests

### Running Tests
```bash
# Run all tests
dotnet test

# Run Godot tests only
dotnet test Tests/AiTest.Godot.Tests/

# Run tests with GODOT_BIN (for GdUnit4Net runtime tests)
export GODOT_BIN="/path/to/Godot_v4.x-stable_mono_win64.exe"  # Linux/macOS
set GODOT_BIN=C:\path\to\Godot_v4.x-stable_mono_win64.exe    # Windows
dotnet test
```

### Writing Tests
```csharp
// xUnit + FluentAssertions
[Fact]
public void ExecutionPhase_ExecuteRegister_PublishesEvent()
{
    var phase = new ExecutionPhase();
    phase.Initialize(runner, dispatcher, _ => {});
    
    phase.ExecuteRegister();
    
    events.Should().HaveCount(1);
}

// GdUnit4Net with Godot runtime
[TestSuite]
public class RobotNodeTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void Robot_Move_UpdatesPosition()
    {
        var robot = new Robot();
        robot.Move(new Position(1, 0));
        AssertThat(robot.Position.X).IsEqual(1);
    }
}
```

### Test Guidelines
- **Unit tests** (`AiTest.Core.Tests`): Test pure game logic without Godot dependencies
- **Integration tests** (`AiTest.Godot.Tests`): Test classes that use Godot APIs
- **Godot Runtime tests** (`GdUnit/`): Use `[RequireGodotRuntime]` attribute, requires `GODOT_BIN` environment variable
- Test file naming: `{ClassName}Tests.cs`
- Test method naming: `{MethodName}_{Scenario}_{ExpectedResult}`

## Godot Documentation

- **Location**: `C:\projects\godot\docs`
- **Usage**: Reference for Godot API, UI components, node types, signals, etc.
- **Format**: HTML docs in `classes/` folder for each Godot class

### Useful Godot Docs
- `class_control.html` - Base UI control node
- `class_button.html` - Button control
- `class_label.html` - Text display
- `class_panelcontainer.html` - Panel containers
- `class_hboxcontainer.html` / `class_vboxcontainer.html` - Layout containers
- `class_popup.html` / `class_window.html` - Modal windows
- `class_viewport.html` - Viewport and scaling

## Common Tasks

### Godot Documentation Usage
- Documentation is located at `C:\projects\godot\docs\classes\`
- Use for: UI components, Control nodes, Container layouts, Viewport settings
- Key docs: Control, Button, Label, Panel, HBoxContainer, VBoxContainer, Viewport, Window

### Add a new class
1. Create new .cs file in appropriate folder
2. Add namespace matching folder structure
3. Make class `public` if part of API

### Finding Godot API Information
For specific Godot API questions, use:
- **codesearch** tool - searches for code examples, documentation, and API references (web)
- **websearch** tool - can search for recent Godot 4 documentation and examples (web)
- **Read tool** - for local docs at `C:\projects\godot\docs\classes\` or `C:\projects\godot\docs\tutorials\`

### Godot Documentation - Key Resources

#### Best Practices Tutorials
- `tutorials/best_practices/project_organization.html` - как организовать проект
- `tutorials/best_practices/scene_organization.html` - организация сцен
- `tutorials/best_practices/autoloads_versus_regular_nodes.html` - автолоады vs обычные ноды
- `tutorials/best_practices/godot_interfaces.html` - интерфейсы в Godot
- `tutorials/best_practices/data_preferences.html` - работа с данными
- `tutorials/best_practices/what_are_godot_classes.html` - ООП подход в Godot

#### C# Best Practices (scripting/c_sharp/)
- `c_sharp_style_guide.html` - руководство по стилю кода C# в Godot
- `c_sharp_signals.html` - работа с сигналами через C# events
- `c_sharp_basics.html` - основы C# в Godot
- `c_sharp_features.html` - особенности C# в Godot
- `c_sharp_differences.html` - отличия от GDScript
- `c_sharp_collections.html` - работа с коллекциями
- `diagnostics/index.html` - список диагностик (GD0101-GD0402)

#### UI Tutorials
- `tutorials/ui/gui_containers.html` - контейнеры (HBox, VBox, Grid, Margin, Panel, Scroll, etc.)
- `tutorials/ui/size_and_anchors.html` - размеры и позиционирование (anchors)
- `tutorials/ui/control_node_gallery.html` - обзор Control нод
- `tutorials/ui/gui_skinning.html` - стилизация UI (themes)
- `tutorials/ui/gui_navigation.html` - навигация с клавиатурой/контроллером

#### Best Practices
- `tutorials/best_practices/node_alternatives.html` - когда использовать Object/RefCounted/Resource вместо Node
- `tutorials/best_practices/data_preferences.html` - работа с данными
- `tutorials/best_practices/autoloads_versus_regular_nodes.html` - автолоады vs обычные ноды
- `tutorials/best_practices/scenes_versus_scripts.html` - сцена vs скрипт

#### Scripting
- `tutorials/scripting/resources.html` - работа с Resources (данные, сериализация)
- `tutorials/scripting/nodes_and_scene_instances.html` - инстансирование сцен
- `tutorials/scripting/groups.html` - группы нод
- `tutorials/scripting/singletons_autoload.html` - автолоады (singletons)

#### Debugging
- `tutorials/scripting/debug/overview_of_debugging_tools.html` - обзор отладочных инструментов
- `tutorials/scripting/debug/the_profiler.html` - профайлер
- `tutorials/scripting/debug/output_panel.html` - output логи

#### Input
- `tutorials/inputs/inputevent.html` - обработка ввода (InputEvent)
- `tutorials/inputs/input_examples.html` - примеры ввода

#### Common Scenarios
1. **Нужна UI кнопка** → `class_button.html` или `tutorials/ui/control_node_gallery.html`
2. **Проблемы с раскладкой (layout)** → `tutorials/ui/gui_containers.html`
3. **Не понимаю как сделать модальное окно** → `class_popup.html`, `class_window.html`
4. **Как организовать код** → `tutorials/best_practices/logic_preferences.html`
5. **Сцена vs скрипт** → `tutorials/best_practices/scenes_versus_scripts.html`
6. **Когда использовать автолоад** → `tutorials/best_practices/autoloads_versus_regular_nodes.html`

### Run a specific test
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

## Godot Docs Search Tool

Для semantic search по Godot документации используй RAG tool:

- **Location**: `C:/projects/godot/godot-docs-rag-tool`
- **Docker**: `docker-compose up -d` (запускает на localhost:5001)

### API
```bash
# Search
curl "http://localhost:5001/search?q=signals+in+csharp"

# MCP tool call
curl -X POST http://localhost:5001/mcp/call \
  -H "Content-Type: application/json" \
  -d '{"tool": "search_godot_docs", "arguments": {"query": "signals csharp"}}'

# Static docs
curl "http://localhost:5001/docs/tutorials/ui/gui_containers.html"

# Health
curl "http://localhost:5001/health"
```

### MCP Tool Definition
```json
{
  "name": "search_godot_docs",
  "description": "Search Godot Engine documentation",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query": {"type": "string"},
      "top_k": {"type": "integer", "default": 3}
    },
    "required": ["query"]
  }
}
```

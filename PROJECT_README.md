# RoboRally Project

## Структура проекта

```
AiTest/
├── AiTest.Core.csproj    # Игровой движок (C# .NET 8)
├── Tests/                 # Юнит-тесты
├── GodotGame/           # Godot визуализация (4.6 Mono)
│   ├── project.godot
│   ├── AiTest RoboRally.csproj
│   └── scripts/
│       └── GameManager.cs
└── .gitignore
```

## Запуск и сборка

### Godot проект

1. Открыть папку `GodotGame/` в Godot 4.6 Mono
2. Нажать F5 для запуска

### Сборка из командной строки

```bash
# Сборка Godot проекта
cd GodotGame && dotnet build

# Сборка движка
dotnet build AiTest.Core.csproj

# Запуск тестов
dotnet test
```

## Важные особенности

### Godot csproj

```xml
<Project Sdk="Godot.NET.Sdk/4.6.1">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../AiTest.Core.csproj" />
  </ItemGroup>
</Project>
```

**Важно:** 
- Использовать `Godot.NET.Sdk` а не стандартный Microsoft.NET.Sdk
- Не добавлять PackageReference на GodotSharp - он идёт с SDK
- После перемещения проекта удалить папки `obj` и `.godot`

### Целевая версия .NET

AiTest.Core и Godot проекты должны использовать **net8.0** (не net10.0) для совместимости с Godot Mono.

### Ошибки LSP в IDE

Красные подчёркивания в IDE - это нормально. Godot.NET.Sdk не понимается стандартными IDE. Сборка при этом работает.

### Управление в игре

- **1-9** - выбрать карту для регистра (фаза программирования)
- **SPACE** - перейти к выполнению / выполнить регистр
- **R** - перезапуск игры

## Конфигурация Godot

### Input mappings (project.godot)

Добавлены mappings для клавиш 1-9:
```
ui_1, ui_2, ..., ui_9
```

## Зависимости

- .NET 8.0 SDK
- Godot 4.6 Mono
- GodotSharp 4.6.1 (встроен в Godot)

## Тесты

```bash
# Запуск всех тестов
dotnet test

# Запуск конкретного теста
dotnet test --filter "FullyQualifiedName~GameEngineTests.MovePlayer"
```

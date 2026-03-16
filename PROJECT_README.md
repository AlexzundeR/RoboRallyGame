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

## Сборка и проверка

### Полная проверка (рекомендуется)

```bash
# 1. Сборка движка
dotnet build

# 2. Очистка и сборка Godot проекта
rm -rf GodotGame/.godot
godot --headless --path GodotGame --build-solutions
```

### Только dotnet (быстрая проверка C#)
```bash
cd GodotGame && dotnet build
```

### Запуск в Godot
Открыть папку `GodotGame/` в Godot 4.6 Mono и нажать F5

### Запуск тестов
```bash
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
- Godot 4.6 Mono (C:\Program Files (x86)\godot\Godot_v4.6.1-stable_mono_win64.exe)

## Тесты

```bash
# Запуск всех тестов
dotnet test

# Запуск конкретного теста
dotnet test --filter "FullyQualifiedName~GameEngineTests.MovePlayer"
```

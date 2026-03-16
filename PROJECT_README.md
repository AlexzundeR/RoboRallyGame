# RoboRally Project

## Основные правила игры

### Фазы игры

1. **Programming (Программирование)** - игроки выбирают 5 карт для регистров
2. **Activation (Выполнение)** - роботы выполняют регистры по очереди (от наивысшего приоритета)
3. **Cleanup (Очистка)** - конец раунда

### Карты программирования

| Тип | Обозначение | Описание |
|-----|-------------|----------|
| Move | M1, M2, M3 | Перемещение на 1-3 клетки |
| Turn Left | ↺ | Поворот налево (90°) |
| Turn Right | ↻ | Поворот направо (90°) |
| U-Turn | ⟲ | Разворот на 180° |

### Типы плиток (Tiles)

| TileType | Обозначение | Описание |
|----------|-------------|----------|
| Floor | · | Обычный пол |
| Wall | █ | Стена - непроходимо |
| Pit | (пусто) | Яма - робот уничтожен |
| Start | S | Стартовая позиция |
| Checkpoint | 1-4 | Контрольная точка (нужно посетить все 4) |
| ConveyorCW/CCW | ↻/↺ | Конвейер (1 клетка/ход) |
| ExpressConveyor | ⤴/⤵ | Экспресс-конвейер (2 клетки/ход) |
| GearCW/CCW | ↺/↻ | Шестерёнка - поворот робота |
| LaserH/LaserV | ━/┃ | Лазер на плитке |

### Приоритет карт

- Карты выполняются в порядке убывания приоритета (чем больше число, тем раньше)
- При одинаковом приоритете - слева направо по карте

### Повреждения

- Стандартный лазер: 1 HP
- Робот уничтожается при 10 HP
- 4 чекпоинта = победа

## Соответствие сущностей

### Модель → Правила

| Класс | Описание |
|-------|----------|
| `GameEngine` | Основной движок игры |
| `GameState` | Состояние игры (раунд, фаза, игроки) |
| `GamePhase` | Setup → Programming → Activation → Cleanup → GameOver |
| `GameMap` | Игровое поле |
| `Tile` / `TileType` | Плитки поля |
| `Robot` | Робот игрока |
| `Player` | Игрок (владеет Robot, Hand, Registers) |
| `ProgramCard` | Базовый класс карты |
| `MoveCard` | Карта движения (M1, M2, M3) |
| `TurnCard` | Карта поворота (↺, ↻) |
| `UTurnCard` | Карта разворота (⟲) |

### Визуализация (GodotGame)

| Godot сущность | Соответствие в модели |
|----------------|---------------------|
| `GameManager.cs` | Управление игрой, UI |
| `ColorRect` (карта) | Tile |
| `ColorRect` (робот) | Robot |
| UI Labels | GameState, Player status |

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

# План реализации системы опций (апгрейдов) RoboRally

## Обзор

Реализация системы **Upgrades** (апгрейдов/опций) на основе оригинальных правил (rules__.md) с элементами Permanent/Temporary из новой версии для удобства.

---

## Анализ правил

### Получение Upgrades (оригинал)
- **Repair Site (1 ключ)** - снять 1 повреждение
- **Repair Site (ключ + молоток)** - снять 1 повреждение + взять 1 карту опций

### Защита от урона (оригинал)
- Игрок может сбросить карту upgrade чтобы предотвратить 1 повреждение
- Обрабатывается в порядке от позиции #1

### 26 Upgrades (оригинал)

| ID | Название | Type | Effect | Uses |
|----|----------|------|--------|------|
| ablative_coat | Ablative Coat | Permanent | Блокирует 3 урона | 3 |
| abort_switch | Abort Switch | Temporary | Заменить карту регистра | 1/раунд |
| brakes | Brakes | Permanent | Move 1 можно не двигать | - |
| circuit_breaker | Circuit Breaker | Permanent | При 3 урона - авто power down | - |
| conditional_program | Conditional Program | Temporary | Отложить карту, заменить позже | 1/раунд |
| crab_legs | Crab Legs | Permanent | Move 1 + поворот = движение вбок | - |
| double_barrel_laser | Double-Barreled Laser | Permanent | +1 выстрел | - |
| dual_processor | Dual Processor | Permanent | Move + поворот в одном регистре | - |
| extra_memory | Extra Memory | Permanent | +1 карта за раунд | - |
| fire_control | Fire Control | Permanent | Вместо урона - заклинить регистр | - |
| flywheel | Flywheel | Temporary | Сохранить карту для следующего раунда | - |
| fourth_gear | Fourth Gear | Permanent | Move 3 = Move 4 | - |
| gyroscopic_stabilizer | Gyroscopic Stabilizer | Permanent | Игнорировать шестерни/конвейеры | 1/раунд |
| high_power_laser | High-power Laser | Permanent | Лазер сквозь стены/роботов | - |
| mechanical_arm | Mechanical Arm | Permanent | Reach до флагов/ремонта через 1 клетку | - |
| mini_howitzer | Mini Howitzer | Temporary | Выстрел + толчок | 5 |
| power_down_shield | Power-down Shield | Permanent | -1 урон когда выключен | - |
| pressor_beam | Pressor Beam | Permanent | Вместо урона - толчок | - |
| radio_control | Radio Control | Permanent | Управлять роботом врага | - |
| ramming_gear | Ramming Gear | Permanent | +1 урон при толчке | - |
| rear_firing_laser | Rear-Firing Laser | Permanent | Стрелять назад | - |
| recompile | Recompile | Temporary | Новая рука (урон 1) | 1/раунд |
| reverse_gear | Reverse Gear | Permanent | Back Up = 2 клетки | - |
| scrambler | Scrambler | Permanent | Заменить карту регистра врага | - |
| superior_archive | Superior Archive | Permanent | Рестарт без +2 урона | - |
| tractor_beam | Tractor Beam | Permanent | Притянуть робота на 1 клетку | - |

---

## Архитектура решения

### 1. Модель данных (Core)

```
RoboRallyGame.Core/Upgrades/
├── UpgradeCard.cs          # Модель upgrade
├── UpgradeCatalog.cs       # Каталог всех 26 upgrades
├── IOptionEffect.cs       # Интерфейс эффекта
└── Effects/
    ├── LaserEffect.cs     # Лазерные модификации
    ├── MovementEffect.cs  # Модификации движения
    ├── DamageEffect.cs    # Модификации урона
    └── SpecialEffect.cs   # Специальные эффекты
```

### 2. Интерфейсы стратегий (расширяемость)

```csharp
public interface IUpgradeEffect
{
    string UpgradeId { get; }
    void Apply(/* context */);
}

public interface ILaserModifier
{
    int ModifyShotCount(int baseCount);
    bool CanPierceThroughWalls();
    bool CanPierceThroughRobots();
    Direction? GetAdditionalFireDirection();
}

public interface IMovementModifier
{
    int ModifyMoveDistance(int baseDistance);
    bool CanSkipMove();           // Brakes
    bool CanMoveSideways();       // Crab Legs
}

public interface IDamageModifier
{
    int BlockDamage(int incomingDamage);
    bool AddDamageOnPush();        // Ramming Gear
}
```

### 3. Интеграция с GameEngine

**Файл:** `GameEngine.cs`

```csharp
// Коллекция upgrades игрока
public IReadOnlyList<UpgradeCard> Upgrades { get; }

// Методы для работы с upgrades
public void GrantUpgrade(Player player, UpgradeCard upgrade);
public bool TryBlockDamage(Player player, int damage);
public void ProcessUpgradeAtRepairSite(Player player, RepairSiteType type);

// Хуки для эффектов
void OnLaserFire(LaserEvent e, Player shooter);
void OnMovement(MoveEvent e, Player player);
void OnDamage(DamageEvent e, Player target);
```

### 4. Интеграция с фазами

**ProgrammingPhase.cs:**
- Conditional Program, Flywheel - отложенные карты
- Extra Memory - модификация количества карт
- Recompile - новая рука

**ExecutionPhase.cs:**
- Abort Switch - замена карты регистра
- Gyroscopic Stabilizer - активация перед регистром
- Все эффекты при выполнении регистров

---

## Этапы реализации

### Этап 1: Модель и каталог

1. **UpgradeCard.cs** - модель с Id, Name, Description, MaxUses, IsPermanent
2. **UpgradeCatalog.cs** - статический каталог 26 upgrades
3. **Unit тесты** - все upgrades загружаются корректно

### Этап 2: Базовая интеграция

1. Добавить `List<UpgradeCard> Upgrades` в Player
2. Методы `GrantUpgrade`, `HasUpgrade`, `RemoveUpgrade`
3. Интеграция с repair site (ключ+молоток) в GameEngine
4. **Unit тесты** - получение upgrades на repair site

### Этап 3: Простые эффекты

| Upgrade | Реализация |
|---------|-----------|
| Rear-Firing Laser | +1 направление выстрела |
| Double-Barreled Laser | +1 выстрел |
| Extra Memory | +1 карта в TotalCardCount |
| Brakes | Skip Move 1 |
| Gyroscopic Stabilizer | Игнор шестерен/конвейеров |
| Fourth Gear | Move 3 → Move 4 |
| Reverse Gear | Back Up = 2 клетки |

### Этап 4: Сложные эффекты

| Upgrade | Реализация |
|---------|-----------|
| Ablative Coat | DamageModifier с 3 зарядами |
| Mini Howitzer | SpecialEffect: выстрел + толчок |
| Pressor Beam | SpecialEffect: толчок без урона |
| Ramming Gear | +1 урон при толчке |
| Fire Control | Вместо урона - LockRegister |
| High-power Laser | Pierce через walls/robots |
| Mechanical Arm | Reach для флагов/ремонта через 1 клетку |
| Superior Archive | Reboot без +2 damage |

### Этап 5: Временные Upgrades

| Upgrade | Реализация |
|---------|-----------|
| Abort Switch | Замена карты регистра |
| Conditional Program | Отложить карту для замены позже |
| Flywheel | Сохранить карту для след. раунда |
| Recompile | Новая рука + 1 урон |

### Этап 6: UI (Godot)

```
src/GodotGame/Upgrades/
├── UpgradeCard.tscn        # Карточка upgrade
├── UpgradeCard.cs
├── UpgradesPanel.tscn      # Панель upgrades игрока
├── UpgradesPanel.cs
├── UpgradeHelpModal.tscn   # Справка по всем
└── UpgradeHelpModal.cs
```

**Components:**
- `UpgradeCard` - иконка, название, оставшиеся использования
- `UpgradesPanel` - горизонтальный скролл всех upgrades игрока
- `UpgradeHelpModal` - модальное окно со всеми 26 upgrades

### Этап 7: Events

```csharp
public sealed record OptionAcquiredEvent(int PlayerId, string OptionId) : IEvent;
public sealed record OptionUsedEvent(int PlayerId, string OptionId, int RemainingUses) : IEvent;
public sealed record OptionDiscardedEvent(int PlayerId, string OptionId) : IEvent;
```

---

## Детальный план

### Неделя 1: Модель и каталог

**День 1-2:**
- [ ] OptionCard.cs с полями: Id, Name, Description, MaxUses (-1 = unlimited), IsPermanent
- [ ] OptionCatalog.cs - все 26 опций с описаниями
- [ ] Тесты: каталог содержит все 26 опций

**День 3-4:**
- [ ] Добавить `List<OptionCard> Options` в Player
- [ ] Методы: `GrantOption()`, `HasOption()`, `RemoveOption()`
- [ ] Тесты: добавление/удаление опций

### Неделя 2: Интеграция с GameEngine

**День 5-6:**
- [ ] Интеграция с repair site (ключ+молоток → GrantOption)
- [ ] Базовые опции: Rear Laser, Double Barrel
- [ ] Тесты: робот получает опцию на repair site

**День 7:**
- [ ] Интеграция с LaserResolver
- [ ] Тесты: опции влияют на выстрелы

### Неделя 3: Движение и эффекты

**День 8-10:**
- [ ] Brakes, Fourth Gear, Reverse Gear
- [ ] Extra Memory (модификация TotalCardCount)
- [ ] Gyroscopic Stabilizer (игнор шестерен/конвейеров)
- [ ] Crab Legs, Dual Processor

**День 11-12:**
- [ ] Ablative Coat, Mini Howitzer, Pressor Beam
- [ ] Ramming Gear, Fire Control
- [ ] Тесты всех эффектов

### Неделя 4: Сложные опции и UI

**День 13-14:**
- [ ] High-power Laser, Mechanical Arm, Superior Archive
- [ ] Scrambler, Radio Control, Conditional Program
- [ ] Flywheel, Recompile, Abort Switch

**День 15-16:**
- [ ] Godot UI: OptionCard, OptionsPanel
- [ ] Godot UI: OptionHelpModal
- [ ] Интеграция с GameManager

### Неделя 5: Полировка

**День 17-18:**
- [ ] Анимации для эффектов опций
- [ ] Обработка edge cases
- [ ] Интеграционные тесты

**День 19-20:**
- [ ] Документация
- [ ] Финальное тестирование

---

## Принципы реализации

1. **Композиция > наследование** - эффекты через интерфейсы
2. **Стратегии** - ILaserModifier, IMovementModifier, IDamageModifier
3. **Не изменять существующую логику** - только расширять через хуки
4. **Тесты сначала** - каждый эффект покрыт тестами
5. **Отдельные сцены Godot** - компонентный подход

---

## Пример интеграции

```csharp
// GameEngine.cs - хук при выстреле
public void OnLaserFired(Player shooter, List<LaserShot> shots)
{
    foreach (var option in shooter.Options)
    {
        var effect = OptionCatalog.GetEffect(option.Id);
        if (effect is ILaserModifier laser)
        {
            laser.ModifyShots(shooter, shots);
        }
    }
}
```

---

## Статус

- [x] Этап 1: Модель и каталог
- [x] Этап 2: Базовая интеграция
- [x] Этап 3: Простые эффекты
- [x] Этап 4: Сложные эффекты
- [x] Этап 5: Временные опции
- [x] Этап 6: UI (Godot)
- [x] Этап 7: Events

### Выполнено (30.03.2026):
1. **UpgradeCatalog.cs** - 26 определений upgrades с описаниями
2. **UpgradeEffects.cs** - все 26 реализаций (Permanent + Temporary)
3. **RepairSite integration** - RepairSitePlus выдает случайный upgrade
4. **PlayerExtensions** - GetLaser/Movement/Damage/ConveyorStrategy
5. **Godot UI** - UpgradeCard, UpgradesPanel, UpgradeHelpModal
6. **Тесты** - 234 Core + 71 Godot тестов проходят
7. **GameEngine.GrantRandomUpgrade()** - метод для выдачи upgrade
8. **OnUpgradeAcquired event** - событие для UI
9. **ActionLog integration** - желтое сообщение при получении upgrade
10. **PlayerPanel status** - показывает количество ⚙ upgrades
11. **Секретные клавиши** - U (выдать upgrade), I (дать 1 урон), D (дать урон)

### Выполнено (31.03.2026):
1. **UI Grid рефакторинг** - переработана структура контейнеров в GameUI.tscn:
   - MainContainer (MarginContainer, 10px margins)
   - MainHBox (HBoxContainer)
     - LeftContent (VBox, fills remaining space)
       - TopSection (TopPanel + spacer)
       - MiddleSpacer (flexible)
       - BottomSection (Player panels)
     - ActionLog (fixed 270px, full height)
   - ModalContainer (CenterContainer для модалов)
2. **Responsive stretch** - обновлены настройки в project.godot:
   - `stretch/mode="canvas_items_stretch"`
   - `stretch/aspect="expand"`
   - `stretch/scale_mode="fractional"`
3. **Fixed gyroscopic_stabilizer** - исправлен тип с Temporary на Permanent

### Следующие шаги:
1. Интеграция Fourth Gear (Move 3 → Move 4)
2. Интеграция Reverse Gear (Back Up 2)
3. Интеграция Extra Memory (+1 карта за раунд)
4. Интеграция Dual Processor (move + rotate в одном регистре)
5. Интеграция Crab Legs (боковое движение)
6. Ablative Coat - использование зарядов при уроне
7. Circuit Breaker - авто power down при 3 уроне
8. Superior Archive - reboot без +2 damage

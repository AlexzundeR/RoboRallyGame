namespace RoboRallyGame.Core.Upgrades;

public sealed record UpgradeDefinition(
    string Id,
    string Name,
    string Description,
    UpgradeType Type,
    int MaxUses
);

public static class UpgradeCatalog
{
    public static UpgradeDefinition AblativeCoat { get; } = new(
        Id: "ablative_coat",
        Name: "Ablative Coat",
        Description: "Защитный экран предотвращает 3 пункта повреждения робота, принимая удар на себя.",
        Type: UpgradeType.Permanent,
        MaxUses: 3
    );

    public static UpgradeDefinition AbortSwitch { get; } = new(
        Id: "abort_switch",
        Name: "Abort Switch",
        Description: "Один раз в течение каждого раунда игрок может заменить открытую программную карту текущего регистра на верхнюю карту из общей стопки.",
        Type: UpgradeType.Temporary,
        MaxUses: 1
    );

    public static UpgradeDefinition Brakes { get; } = new(
        Id: "brakes",
        Name: "Brakes",
        Description: "Когда игрок использует программную карту «Move 1», он может не передвигать робота вовсе.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition CircuitBreaker { get; } = new(
        Id: "circuit_breaker",
        Name: "Circuit Breaker",
        Description: "Если к концу раунда робот игрока имеет 3 жетона повреждений, то он автоматически выключается.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition ConditionalProgram { get; } = new(
        Id: "conditional_program",
        Name: "Conditional Program",
        Description: "После программирования игрок может отложить одну программную карту и заменить ей карту в регистре позже.",
        Type: UpgradeType.Temporary,
        MaxUses: 1
    );

    public static UpgradeDefinition CrabLegs { get; } = new(
        Id: "crab_legs",
        Name: "Crab Legs",
        Description: "Move 1 + Rotate Left/Right позволяет двигаться вбок без поворота.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition DoubleBarrelLaser { get; } = new(
        Id: "double_barrel_laser",
        Name: "Double-Barreled Laser",
        Description: "Бортовой лазер совершает два выстрела вместо одного.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition DualProcessor { get; } = new(
        Id: "dual_processor",
        Name: "Dual Processor",
        Description: "В один регистр можно положить карту движения и карту поворота. Сначала движение (на 1 меньше), потом поворот.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition ExtraMemory { get; } = new(
        Id: "extra_memory",
        Name: "Extra Memory",
        Description: "Игрок получает одну дополнительную программную карту в начале каждого раунда.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition FireControl { get; } = new(
        Id: "fire_control",
        Name: "Fire Control",
        Description: "Вместо нанесения повреждения можно заклинить один регистр или удалить карту опций целевого робота.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition Flywheel { get; } = new(
        Id: "flywheel",
        Name: "Flywheel",
        Description: "После программирования можно отложить одну карту и добавить её к руке в следующем раунде.",
        Type: UpgradeType.Temporary,
        MaxUses: -1
    );

    public static UpgradeDefinition FourthGear { get; } = new(
        Id: "fourth_gear",
        Name: "Fourth Gear",
        Description: "При использовании карты «Move 3» можно передвинуть робота на 4 клетки.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition GyroscopicStabilizer { get; } = new(
        Id: "gyroscopic_stabilizer",
        Name: "Gyroscopic Stabilizer",
        Description: "Один раз за раунд можно активировать перед открытием карт 1 регистра. Робот не поворачивается на шестернях и конвейерах.",
        Type: UpgradeType.Temporary,
        MaxUses: 1
    );

    public static UpgradeDefinition HighPowerLaser { get; } = new(
        Id: "high_power_laser",
        Name: "High-power Laser",
        Description: "Лазер проходит сквозь одну стену и через робота (оба получают повреждения).",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition MechanicalArm { get; } = new(
        Id: "mechanical_arm",
        Name: "Mechanical Arm",
        Description: "Робот может дотянуться до флага или пункта ремонта с соседней клетки (если нет стены между ними).",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition MiniHowitzer { get; } = new(
        Id: "mini_howitzer",
        Name: "Mini Howitzer",
        Description: "Вместо лазера можно выстрелить из гаубицы: цель отталкивается на 1 клетку и получает 1 повреждение. 5 зарядов.",
        Type: UpgradeType.Temporary,
        MaxUses: 5
    );

    public static UpgradeDefinition PowerDownShield { get; } = new(
        Id: "power_down_shield",
        Name: "Power-down Shield",
        Description: "Пока робот выключен, защитный колокол предотвращает одно повреждение с каждого направления.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition PressorBeam { get; } = new(
        Id: "pressor_beam",
        Name: "Pressor Beam",
        Description: "Вместо лазера можно использовать толкатель: цель отталкивается на 1 клетку без повреждения.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition RadioControl { get; } = new(
        Id: "radio_control",
        Name: "Radio Control",
        Description: "Вместо лазера целевой робот до конца раунда выполняет программу атаковавшего робота.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition RammingGear { get; } = new(
        Id: "ramming_gear",
        Name: "Ramming Gear",
        Description: "Когда робот толкает или врезается в другого робота, тот получает одно повреждение.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition RearFiringLaser { get; } = new(
        Id: "rear_firing_laser",
        Name: "Rear-Firing Laser",
        Description: "Гамма-Лазер стреляет назад, дополняя бортовой лазер.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition Recompile { get; } = new(
        Id: "recompile",
        Name: "Recompile",
        Description: "Один раз за раунд можно сбросить все карты из руки и взять новые. Робот получает 1 повреждение.",
        Type: UpgradeType.Temporary,
        MaxUses: 1
    );

    public static UpgradeDefinition ReverseGear { get; } = new(
        Id: "reverse_gear",
        Name: "Reverse Gear",
        Description: "При использовании карты «Back Up» можно передвинуть робота на 2 клетки назад.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition Scrambler { get; } = new(
        Id: "scrambler",
        Name: "Scrambler",
        Description: "Вместо лазера можно заменить программную карту следующего регистра целевого робота на верхнюю карту из стопки.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition SuperiorArchive { get; } = new(
        Id: "superior_archive",
        Name: "Superior Archive",
        Description: "Когда робот восстанавливается после уничтожения, он не получает 2 жетона повреждений.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static UpgradeDefinition TractorBeam { get; } = new(
        Id: "tractor_beam",
        Name: "Tractor Beam",
        Description: "Вместо лазера можно притянуть целевого робота на 1 клетку по направлению к себе.",
        Type: UpgradeType.Permanent,
        MaxUses: -1
    );

    public static IReadOnlyList<UpgradeDefinition> AllUpgrades { get; } = new[]
    {
        AblativeCoat,
        AbortSwitch,
        Brakes,
        CircuitBreaker,
        ConditionalProgram,
        CrabLegs,
        DoubleBarrelLaser,
        DualProcessor,
        ExtraMemory,
        FireControl,
        Flywheel,
        FourthGear,
        GyroscopicStabilizer,
        HighPowerLaser,
        MechanicalArm,
        MiniHowitzer,
        PowerDownShield,
        PressorBeam,
        RadioControl,
        RammingGear,
        RearFiringLaser,
        Recompile,
        ReverseGear,
        Scrambler,
        SuperiorArchive,
        TractorBeam
    };

    public static UpgradeDefinition? Get(string id) =>
        AllUpgrades.FirstOrDefault(u => u.Id == id);

    public static UpgradeEffect CreateEffect(UpgradeDefinition definition) =>
        definition.Type switch
        {
            UpgradeType.Permanent => CreatePermanentEffect(definition.Id),
            UpgradeType.Temporary => CreateTemporaryEffect(definition.Id),
            _ => throw new ArgumentException($"Unknown upgrade type: {definition.Type}")
        };

    private static UpgradeEffect CreatePermanentEffect(string id) => id switch
    {
        "brakes" => new Permanent.Brakes(),
        "double_barrel_laser" => new Permanent.DoubleBarrelLaser(),
        "rear_firing_laser" => new Permanent.RearFiringLaser(),
        "fourth_gear" => new Permanent.FourthGear(),
        "reverse_gear" => new Permanent.ReverseGear(),
        "crab_legs" => new Permanent.CrabLegs(),
        "dual_processor" => new Permanent.DualProcessor(),
        "extra_memory" => new Permanent.ExtraMemory(),
        "gyroscopic_stabilizer" => new Permanent.GyroscopicStabilizer(),
        "high_power_laser" => new Permanent.HighPowerLaser(),
        "fire_control" => new Permanent.FireControl(),
        "mechanical_arm" => new Permanent.MechanicalArm(),
        "power_down_shield" => new Permanent.PowerDownShield(),
        "pressor_beam" => new Permanent.PressorBeam(),
        "ramming_gear" => new Permanent.RammingGear(),
        "radio_control" => new Permanent.RadioControl(),
        "scrambler" => new Permanent.Scrambler(),
        "superior_archive" => new Permanent.SuperiorArchive(),
        "tractor_beam" => new Permanent.TractorBeam(),
        "circuit_breaker" => new Permanent.CircuitBreaker(),
        "ablative_coat" => new Permanent.AblativeCoat(),
        _ => throw new ArgumentException($"Unknown permanent upgrade: {id}")
    };

    private static UpgradeEffect CreateTemporaryEffect(string id) => id switch
    {
        "abort_switch" => new Temporary.AbortSwitch(),
        "conditional_program" => new Temporary.ConditionalProgram(),
        "flywheel" => new Temporary.Flywheel(),
        "recompile" => new Temporary.Recompile(),
        "mini_howitzer" => new Temporary.MiniHowitzer(),
        _ => throw new ArgumentException($"Unknown temporary upgrade: {id}")
    };
}

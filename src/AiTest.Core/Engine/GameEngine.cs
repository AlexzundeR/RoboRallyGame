namespace AiTest.Core.Engine;

using AiTest.Core.Cards;
using AiTest.Core.Entities;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;
using AiTest.Core.Upgrades;

public interface IDealer
{
    void Deal(IEnumerable<Player> players, IReadOnlyList<ProgramCard> deck);
}

public static class CardDealerHelper
{
    public static int GetCardCountForDamage(int damage) => damage switch
    {
        0 => 9,
        1 => 8,
        2 => 7,
        3 => 6,
        4 => 5,
        5 => 4,
        6 => 3,
        7 => 2,
        8 => 1,
        9 => 0,
        _ => 0
    };
}

public sealed class RandomDealer : IDealer
{
    private readonly Random _random = new();
    
    public void Deal(IEnumerable<Player> players, IReadOnlyList<ProgramCard> deck)
    {
        foreach (var player in players)
        {
            var cardCount = CardDealerHelper.GetCardCountForDamage(player.Robot.Damage);
            var hand = deck.OrderBy(_ => _random.Next()).Take(cardCount).ToList();
            player.DealCards(hand);
        }
    }
}

public sealed class FixedDealer : IDealer
{
    private readonly IReadOnlyList<IReadOnlyList<ProgramCard>> _hands;
    private int _currentHand;
    
    public FixedDealer(params IReadOnlyList<ProgramCard>[] hands)
    {
        _hands = hands;
        _currentHand = 0;
    }
    
    public void Deal(IEnumerable<Player> players, IReadOnlyList<ProgramCard> deck)
    {
        foreach (var player in players)
        {
            var cardCount = CardDealerHelper.GetCardCountForDamage(player.Robot.Damage);
            var hand = _currentHand < _hands.Count 
                ? _hands[_currentHand].Take(cardCount).ToList() 
                : deck.Take(cardCount).ToList();
            player.DealCards(hand);
            _currentHand++;
        }
    }
}

public enum GamePhase
{
    Setup,
    Programming,
    Activation,
    Cleanup,
    GameOver
}

public sealed class GameState
{
    private readonly List<Player> _players;

    public GameMap Map { get; set; } = null!;
    public IReadOnlyList<Player> Players => _players;
    public int CurrentPlayerIndex { get; set; }
    public int CurrentRegister { get; set; }
    public GamePhase Phase { get; set; } = GamePhase.Setup;
    public int Round { get; set; }
    public Player? Winner { get; set; }

    public GameState() : this(new List<Player>()) { }

    public GameState(List<Player> players)
    {
        _players = players;
        Map = new GameMap(10, 10);
    }

    public GameState(List<Player> players, GameMap map)
    {
        _players = players;
        Map = map;
        Phase = GamePhase.Programming;
    }

    public Player AddPlayer(string name)
    {
        var player = new Player
        {
            Id = _players.Count + 1,
            Name = name,
            Robot = new Robot
            {
                Id = _players.Count + 1,
                Name = name
            }
        };
        _players.Add(player);
        return player;
    }

    public Player? GetCurrentPlayer()
    {
        return CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count
            ? _players[CurrentPlayerIndex]
            : null;
    }

    public void Clear()
    {
        _players.Clear();
        CurrentPlayerIndex = 0;
        CurrentRegister = 0;
        Round = 0;
        Phase = GamePhase.Setup;
        Winner = null;
    }
}

public interface IGameEngine
{
    GameState State { get; }
    void Initialize(GameMap map);
    void AddPlayer(string name);
    void StartGame();
    void BeginRegisterExecution(int registerIndex);
    StepResult TryExecuteNextStep();
    void NextRegister();
    void NextRound();
    bool IsGameOver();
    IReadOnlyList<Player> GetWinners();
}

public sealed class GameEngine : IGameEngine
{
    private readonly Random _random = new();
    private readonly CollisionResolver _collisionResolver = new();
    private readonly LaserResolver _laserResolver = new();
    private IDealer _dealer;

    private int _currentRegister = -1;
    private int _currentStep = -1;
    private List<Player> _alivePlayers = new();
    private int _movementStepIndex = -1;
    private bool _conveyorExecuted = false;
    private bool _laserExecuted = false;

    public event Action<MoveEvent>? OnMove;
    public event Action<RotateEvent>? OnRotate;
    public event Action<LaserFireEvent>? OnLaserFire;
    public event Action<DamageEvent>? OnDamage;
    public event Action<ConveyorMoveEvent>? OnConveyorMove;
    public event Action<CheckpointEvent>? OnCheckpoint;
    public event Action<RobotDestroyedEvent>? OnRobotDestroyed;
    public event Action<RobotRebootEvent>? OnRobotReboot;
    public event Action<RobotRepairedEvent>? OnRobotRepaired;
    public event Action<LifeTokenLostEvent>? OnLifeTokenLost;
    public event Action<MovementPhaseStartedEvent>? OnMovementPhaseStarted;
    public event Action<ConveyorPhaseStartedEvent>? OnConveyorPhaseStarted;
    public event Action<LaserPhaseStartedEvent>? OnLaserPhaseStarted;
    public event Action<PhaseExecutionCompleteEvent>? OnPhaseExecutionComplete;
    public event Action? OnAllRegistersComplete;
    public event Action<GameOverEvent>? OnGameOver;

    public GameEngine() : this(new RandomDealer(), new GameState()) { }
    
    public GameEngine(IDealer dealer) : this(dealer, new GameState()) { }

    public GameEngine(IDealer dealer, GameState state)
    {
        _dealer = dealer;
        State = state;
    }

    public IDealer Dealer
    {
        get => _dealer;
        set => _dealer = value ?? throw new ArgumentNullException(nameof(value));
    }

    public GameState State { get; }

    public void Initialize(GameMap map)
    {
        State.Map = map;
        State.Phase = GamePhase.Programming;
    }

    public void AddPlayer(string name)
    {
        var player = State.AddPlayer(name);
        var startTiles = FindStartTiles();
        if (startTiles.Count > player.Id - 1)
        {
            var startPos = startTiles[player.Id - 1];
            player.Robot.SetPosition(startPos);
            player.Robot.SetStartPosition(startPos);
        }
    }

    public void StartGame()
    {
        State.Phase = GamePhase.Programming;
        State.Round = 1;
        DealCardsToAllPlayers();
    }

    public void BeginRegisterExecution(int registerIndex)
    {
        _currentRegister = registerIndex;
        _currentStep = 0;
        _alivePlayers = State.Players.Where(p => p.Robot.IsAlive).ToList();
        _movementStepIndex = 0;
        _conveyorExecuted = false;
        _laserExecuted = false;
    }

    public StepResult TryExecuteNextStep()
    {
        var totalSteps = GetTotalSteps();
        if (_currentStep >= totalSteps)
        {
            OnPhaseExecutionComplete?.Invoke(new PhaseExecutionCompleteEvent(_currentRegister));
            return StepResult.Completed;
        }

        var movementSteps = _alivePlayers.Count;

        if (_currentStep < movementSteps)
        {
            ExecuteCards(_currentRegister);
            _currentStep = movementSteps;
        }
        else if (_currentStep == movementSteps && !_conveyorExecuted)
        {
            ApplyNonConveyorEffects();
            ProcessConveyorPhase();
            CheckGameOver();
            _conveyorExecuted = true;
            _currentStep++;
        }
        else if (_currentStep > movementSteps && !_laserExecuted)
        {
            FireLasers();
            _laserExecuted = true;
            _currentStep++;
        }
        else
        {
            _currentStep++;
        }

        return StepResult.Executed;
    }

    private int GetTotalSteps()
    {
        return _alivePlayers.Count + 3;
    }

    private void FireLasers()
    {
        var shots = _laserResolver.PrepareShots(State.Map, State.Players);
        
        foreach (var shot in shots)
        {
            var hitPlayerId = State.Players
                .FirstOrDefault(p => p.Robot.Id != shot.Source.Id && 
                    p.Robot.IsAlive && 
                    shot.Path.Contains(p.Robot.Position))
                ?.Robot.Id;
            
            var damage = hitPlayerId.HasValue ? 1 : 0;
            OnLaserFire?.Invoke(new LaserFireEvent(shot.Source.Id, shot.Source.Position, shot.Direction, shot.Path, hitPlayerId, damage));
        }
        
        _laserResolver.Fire(shots, State.Players);
    }

    private Dictionary<int, (Position Pos, Direction Dir, int Damage)> CaptureStates()
    {
        var states = new Dictionary<int, (Position, Direction, int)>();
        foreach (var player in State.Players)
        {
            states[player.Robot.Id] = (player.Robot.Position, player.Robot.Direction, player.Robot.Damage);
        }
        return states;
    }

    private Dictionary<int, int> CaptureDamageStates()
    {
        return State.Players.ToDictionary(p => p.Robot.Id, p => p.Robot.Damage);
    }

    private void CheckDamageOrDestroy(Dictionary<int, int> damageBefore)
    {
        foreach (var player in State.Players)
        {
            var id = player.Robot.Id;
            if (damageBefore.TryGetValue(id, out var before))
            {
                var current = player.Robot.Damage;
                if (current != before)
                {
                    OnDamage?.Invoke(new DamageEvent(id, current - before, current));
                    if (current >= 10)
                        OnRobotDestroyed?.Invoke(new RobotDestroyedEvent(id, player.Robot.Position));
                }
            }
        }
    }
    
    private void FireEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after,
        bool fireMove,
        bool fireRotate)
    {
        foreach (var player in State.Players)
        {
            var id = player.Robot.Id;
            if (before.TryGetValue(id, out var b) && after.TryGetValue(id, out var a))
            {
                if (fireMove && b.Pos != a.Pos)
                {
                    OnMove?.Invoke(new MoveEvent(id, b.Pos, a.Pos));
                }
                if (fireRotate && b.Dir != a.Dir)
                {
                    OnRotate?.Invoke(new RotateEvent(id, b.Dir, a.Dir));
                }
            }
        }
    }

    private void FireMoveAndRotateEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after)
    {
        FireEvents(before, after, fireMove: true, fireRotate: true);
    }
    
    private void ExecuteCards(int registerIndex)
    {
        var before = CaptureStates();
        var moveIntents = new List<MoveIntent>();
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive || player.Robot.NeedsReboot)
                continue;

            if (registerIndex < player.Robot.LockedRegisters)
                continue;

            var cardExecutionStrategy = GetCardExecutionStrategy(player);
            var movementStrategy = GetMovementStrategy(player);
            
            var card = player.GetRegister(registerIndex);
            if (card == null)
                continue;

            if (card.Card is TurnCard turnCard)
            {
                if (turnCard.IsLeft)
                    player.Robot.RotateLeft();
                else
                    player.Robot.RotateRight();
                
                cardExecutionStrategy.ExecuteRotation(player.Robot);
                continue;
            }
            
            if (card.Card is UTurnCard)
            {
                player.Robot.Rotate180();
                cardExecutionStrategy.ExecuteRotation(player.Robot);
                continue;
            }

            if (card.Card is BackUpCard)
            {
                var oppositeDir = player.Robot.Direction.TurnAround();
                var currentTile = State.Map.GetTile(player.Robot.Position);
                var targetPos = player.Robot.Position.Move(oppositeDir);
                var targetTile = State.Map.GetTile(targetPos);
                
                if (State.Map.IsInBounds(targetPos) && 
                    targetTile.CanEnterFrom(oppositeDir) && 
                    currentTile.CanExitTo(oppositeDir))
                {
                    moveIntents.Add(new MoveIntent
                    {
                        Robot = player.Robot,
                        From = player.Robot.Position,
                        To = targetPos,
                        Direction = oppositeDir,
                        Priority = card.Card.Priority,
                        RegisterIndex = registerIndex,
                        Type = MoveType.Normal
                    });
                }
                continue;
            }

            if (card.Card is MoveCard moveCard)
            {
                var extraSpaces = movementStrategy.GetExtraMoveSpaces(player.Robot) 
                    + cardExecutionStrategy.GetExtraMoveSpaces(player.Robot, moveCard.Spaces);
                var targetPos = CalculateMoveTarget(player.Robot, moveCard.Spaces + extraSpaces, movementStrategy);
                
                moveIntents.Add(new MoveIntent
                {
                    Robot = player.Robot,
                    From = player.Robot.Position,
                    To = targetPos,
                    Direction = player.Robot.Direction,
                    Priority = moveCard.Priority,
                    RegisterIndex = registerIndex,
                    Type = MoveType.Normal
                });
            }
        }
        
        if (moveIntents.Count > 0)
        {
            var resolver = new CollisionResolver();
            foreach (var intent in moveIntents)
                resolver.AddIntent(intent);
            
            resolver.Resolve(State.Map, State.Players.Select(p => p.Robot).ToList());
        }
        
        var after = CaptureStates();
        FireMoveAndRotateEvents(before, after);
        CheckDamageOrDestroy(before.ToDictionary(x => x.Key, x => x.Value.Damage));
    }

    private Position CalculateMoveTarget(Robot robot, int spaces)
    {
        return CalculateMoveTarget(robot, spaces, DefaultMovementStrategy.Instance);
    }

    private Position CalculateMoveTarget(Robot robot, int spaces, IMovementStrategy movementStrategy)
    {
        var pos = robot.Position;
        for (int i = 0; i < spaces; i++)
        {
            var currentTile = State.Map.GetTile(pos);
            var next = pos.Move(robot.Direction);
            if (State.Map.IsInBounds(next))
            {
                var nextTile = State.Map.GetTile(next);
                if (!nextTile.CanEnterFrom(robot.Direction))
                    break;
                if (!currentTile.CanExitTo(robot.Direction))
                    break;
                if (!movementStrategy.CanEnterTile(robot, State.Map, next))
                    break;
                pos = next;
            }
        }
        return movementStrategy.CalculateMove(robot, State.Map, pos);
    }

    private ICardExecutionStrategy GetCardExecutionStrategy(Player player)
    {
        var strategies = new List<ICardExecutionStrategy> { DefaultCardExecutionStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetCardExecutionStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeCardExecutionStrategy(strategies);
    }

    private IMovementStrategy GetMovementStrategy(Player player)
    {
        var strategies = new List<IMovementStrategy> { DefaultMovementStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetMovementStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeMovementStrategy(strategies);
    }

    private void ApplyNonConveyorEffects()
    {
        var damageBefore = CaptureDamageStates();
        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var tile = State.Map.GetTile(player.Robot.Position);
            
            if (tile.GetComponent<ConveyorBehaviorComponent>() != null)
                continue;
            
            var result = tile.GetComponent<ITileBehaviorComponent>()?.ApplyPostMovement(player, State.Map);
            
            switch (result)
            {
                case TileActionResult.CheckpointReached:
                    OnCheckpoint?.Invoke(new CheckpointEvent(player.Robot.Id, player.Robot.CheckpointsReached, totalCheckpoints));
                    break;
                case TileActionResult.FellIntoPit:
                    var spawnPos = player.Robot.LastCheckpointPosition ?? player.Robot.StartPosition;
                    player.Robot.Reboot(spawnPos);
                    OnLifeTokenLost?.Invoke(new LifeTokenLostEvent(player.Robot.Id, player.Robot.LifeTokens));
                    OnRobotReboot?.Invoke(new RobotRebootEvent(player.Robot.Id, spawnPos));
                    break;
            }
        }
        
        CheckDamageOrDestroy(damageBefore);
    }

    private void ProcessConveyorPhase()
    {
        var conveyorIntents = new List<MoveIntent>();
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;
            
            var tile = State.Map.GetTile(player.Robot.Position);
            var component = tile.GetComponent<ConveyorBehaviorComponent>();
            
            if (component == null)
                continue;
                
            if (player.GetConveyorStrategy().ShouldIgnoreConveyors(player.Robot))
                continue;
            
            var intent = component.TryCreateIntent(player.Robot, State.Map);
            if (intent != null)
                conveyorIntents.Add(intent);
        }
        
        if (conveyorIntents.Count == 0)
            return;
        
        var damageBefore = CaptureDamageStates();
        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        
        var resolver = new CollisionResolver();
        foreach (var intent in conveyorIntents)
            resolver.AddIntent(intent);
        
        var result = resolver.Resolve(State.Map, State.Players.Select(p => p.Robot).ToList());
        
        foreach (var movement in result.Movements)
        {
            OnConveyorMove?.Invoke(new ConveyorMoveEvent(movement.RobotId, movement.From, movement.To));
        }
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var tile = State.Map.GetTile(player.Robot.Position);
            var postResult = tile.GetComponent<ITileBehaviorComponent>()?.ApplyPostMovement(player, State.Map);
            
            switch (postResult)
            {
                case TileActionResult.CheckpointReached:
                    OnCheckpoint?.Invoke(new CheckpointEvent(player.Robot.Id, player.Robot.CheckpointsReached, totalCheckpoints));
                    break;
                case TileActionResult.FellIntoPit:
                    var spawnPos = player.Robot.LastCheckpointPosition ?? player.Robot.StartPosition;
                    player.Robot.Reboot(spawnPos);
                    OnLifeTokenLost?.Invoke(new LifeTokenLostEvent(player.Robot.Id, player.Robot.LifeTokens));
                    OnRobotReboot?.Invoke(new RobotRebootEvent(player.Robot.Id, spawnPos));
                    break;
            }
        }
        
        CheckDamageOrDestroy(damageBefore);
    }

    private void CheckGameOver()
    {
        if (IsGameOver() && State.Phase != GamePhase.GameOver)
        {
            State.Phase = GamePhase.GameOver;
            var winners = GetWinners();
            OnGameOver?.Invoke(new GameOverEvent(
                winners.Select(w => w.Id).ToList(),
                winners.Count > 1));
        }
    }

    public void NextRegister()
    {
        State.CurrentRegister++;
        if (State.CurrentRegister >= 5)
        {
            State.CurrentRegister = 0;
            OnAllRegistersComplete?.Invoke();
            NextRound();
        }
    }

    public void NextRound()
    {
        State.Round++;
        State.CurrentRegister = 0;
        State.CurrentPlayerIndex = 0;
        State.Phase = GamePhase.Programming;
        
        foreach (var player in State.Players)
        {
            if (player.Robot.Status == RobotStatus.PowerDown)
            {
                player.Robot.WakeUp();
                player.Robot.Repair(player.Robot.Damage);
                OnRobotRepaired?.Invoke(new RobotRepairedEvent(player.Robot.Id, player.Robot.Damage, 0));
            }
            else
            {
                player.ClearRegisters();
            }
            
            if (player.Robot.Status == RobotStatus.Rebooting)
            {
                var spawnPos = player.Robot.LastCheckpointPosition ?? new Position(0, 0);
                player.Robot.Reboot(spawnPos);
                OnRobotReboot?.Invoke(new RobotRebootEvent(player.Robot.Id, spawnPos));
            }

            var tile = State.Map.GetTile(player.Robot.Position);
            var repairComponent = tile.GetComponent<IEndOfRoundComponent>();
            if (repairComponent != null)
            {
                var result = repairComponent.ApplyEndOfRound(player);
                if (result == TileActionResult.Repaired)
                {
                    OnRobotRepaired?.Invoke(new RobotRepairedEvent(player.Robot.Id, 1, player.Robot.Damage));
                }
            }
        }

        DealCardsToAllPlayers();
    }

    public bool IsGameOver()
    {
        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        return State.Players.Any(p => p.Robot.CheckpointsReached >= totalCheckpoints);
    }

    public IReadOnlyList<Player> GetWinners()
    {
        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        return State.Players
            .Where(p => p.Robot.CheckpointsReached >= totalCheckpoints)
            .ToList();
    }

    private List<Position> FindStartTiles()
    {
        var starts = new List<Position>();
        for (int x = 0; x < State.Map.Width; x++)
        {
            for (int y = 0; y < State.Map.Height; y++)
            {
                if (State.Map.GetTile(x, y).Type == TileType.Start)
                    starts.Add(new Position(x, y));
            }
        }
        return starts;
    }

    private void DealCardsToAllPlayers()
    {
        var deck = CreateDeck();
        _dealer.Deal(State.Players, deck);
    }

    private List<ProgramCard> CreateDeck()
    {
        var deck = new List<ProgramCard>();

        for (int i = 0; i < 18; i++)
            deck.Add(new MoveCard { Priority = 100 + i, Spaces = 1 });
        
        for (int i = 0; i < 12; i++)
            deck.Add(new MoveCard { Priority = 200 + i, Spaces = 2 });
        
        for (int i = 0; i < 6; i++)
            deck.Add(new MoveCard { Priority = 300 + i, Spaces = 3 });

        for (int i = 0; i < 18; i++)
            deck.Add(new TurnCard { Priority = 50 + i * 10, IsLeft = true });
        
        for (int i = 0; i < 18; i++)
            deck.Add(new TurnCard { Priority = 60 + i * 10, IsLeft = false });

        for (int i = 0; i < 6; i++)
            deck.Add(new UTurnCard { Priority = 400 + i });

        for (int i = 0; i < 6; i++)
            deck.Add(new BackUpCard { Priority = 500 + i });

        return deck;
    }
}

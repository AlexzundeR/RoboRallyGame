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
    void NextRound();
    bool IsGameOver();
    Player? GetWinner();
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
    public event Action<MovementPhaseStartedEvent>? OnMovementPhaseStarted;
    public event Action<ConveyorPhaseStartedEvent>? OnConveyorPhaseStarted;
    public event Action<LaserPhaseStartedEvent>? OnLaserPhaseStarted;
    public event Action<PhaseExecutionCompleteEvent>? OnPhaseExecutionComplete;

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
            player.Robot.SetPosition(startTiles[player.Id - 1]);
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
            ApplyBoardElements();
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

    private Dictionary<int, (Position Pos, Direction Dir, int Damage)> CaptureRobotStates()
    {
        var states = new Dictionary<int, (Position, Direction, int)>();
        foreach (var player in State.Players)
        {
            states[player.Robot.Id] = (player.Robot.Position, player.Robot.Direction, player.Robot.Damage);
        }
        return states;
    }
    
    private void FireEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after,
        bool fireMove,
        bool fireRotate,
        bool fireDamage)
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
                if (fireDamage && b.Damage != a.Damage)
                {
                    OnDamage?.Invoke(new DamageEvent(id, a.Damage - b.Damage, a.Damage));
                    
                    if (a.Damage >= 10)
                    {
                        OnRobotDestroyed?.Invoke(new RobotDestroyedEvent(id, player.Robot.Position));
                    }
                }
            }
        }
    }

    private void FireMoveAndRotateEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after)
    {
        FireEvents(before, after, fireMove: true, fireRotate: true, fireDamage: false);
    }
    
    private void FireMoveEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after)
    {
        foreach (var player in State.Players)
        {
            var id = player.Robot.Id;
            if (before.TryGetValue(id, out var b) && after.TryGetValue(id, out var a))
            {
                if (b.Pos != a.Pos)
                {
                    OnConveyorMove?.Invoke(new ConveyorMoveEvent(id, b.Pos, a.Pos));
                }
            }
        }
    }
    
    private void FireDamageEvents(
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> before,
        Dictionary<int, (Position Pos, Direction Dir, int Damage)> after)
    {
        FireEvents(before, after, fireMove: false, fireRotate: false, fireDamage: true);
    }

    private void ExecuteCards(int registerIndex)
    {
        var before = CaptureRobotStates();
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
                        Direction = player.Robot.Direction,
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
        
        var after = CaptureRobotStates();
        FireMoveAndRotateEvents(before, after);
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

    private void ApplyBoardElements()
    {
        var before = CaptureRobotStates();
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var ignorePits = player.GetConveyorStrategy().ShouldIgnorePits(player.Robot);
            var tile = State.Map.GetTile(player.Robot.Position);

            switch (tile.Type)
            {
                case TileType.Pit when !ignorePits:
                    HandlePit(player);
                    break;
                case TileType.Gear:
                case TileType.Conveyor:
                    tile.GetComponent<ITileBehaviorComponent>()?.ApplyPostMovement(player, State.Map);
                    break;
                case TileType.Checkpoint:
                    HandleCheckpoint(player, tile.CheckpointNumber ?? 0);
                    break;
            }
        }
        
        var after = CaptureRobotStates();
        FireMoveEvents(before, after);
        FireDamageEvents(before, after);
    }

    private void HandlePit(Player player)
    {
        player.Robot.LoseLifeToken();
        
        if (player.Robot.LastCheckpointPosition.HasValue)
        {
            player.Robot.Reboot(player.Robot.LastCheckpointPosition.Value);
        }
    }

    private void HandleCheckpoint(Player player, int checkpointNum)
    {
        if (checkpointNum == player.Robot.CheckpointsReached + 1)
        {
            for (int i = 0; i < checkpointNum; i++)
                player.Robot.AddCheckpoint(player.Robot.Position);
            OnCheckpoint?.Invoke(new CheckpointEvent(player.Robot.Id, checkpointNum));
        }
    }

    public void NextRegister()
    {
        State.CurrentRegister++;
        if (State.CurrentRegister >= 5)
        {
            State.CurrentRegister = 0;
            NextRound();
        }
    }

    public void NextRound()
    {
        State.Round++;
        State.CurrentRegister = 0;
        State.Phase = GamePhase.Programming;
        
        foreach (var player in State.Players)
        {
            if (player.Robot.Status == RobotStatus.PowerDown)
            {
                player.Robot.WakeUp();
                player.Robot.Repair(player.Robot.Damage);
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
        }

        DealCardsToAllPlayers();
    }

    public bool IsGameOver()
    {
        return State.Players.Any(p => p.Robot.CheckpointsReached >= 4);
    }

    public Player? GetWinner()
    {
        return State.Players.FirstOrDefault(p => p.Robot.CheckpointsReached >= 4);
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

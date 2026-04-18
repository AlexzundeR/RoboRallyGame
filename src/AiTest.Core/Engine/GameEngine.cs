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

    public GameMap Map { get; private set; } = null!;
    public IReadOnlyList<Player> Players => _players;
    public int CurrentPlayerIndex { get; private set; }
    public int CurrentRegister { get; private set; }
    public GamePhase Phase { get; private set; } = GamePhase.Setup;
    public int Round { get; private set; }
    public Player? Winner { get; private set; }

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
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public void InitializeMap(GameMap map)
    {
        Map = map;
    }

    public Player? GetCurrentPlayer()
    {
        return CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count
            ? _players[CurrentPlayerIndex]
            : null;
    }

    public Player? GetPlayer(int playerId)
    {
        return _players.FirstOrDefault(p => p.Id == playerId);
    }

    public void SetCurrentRegister(int register)
    {
        CurrentRegister = register;
    }

    public void SetCurrentPlayerIndex(int index)
    {
        CurrentPlayerIndex = index;
    }

    public void SetPhase(GamePhase phase)
    {
        Phase = phase;
    }

    public void IncrementRound()
    {
        Round++;
    }

    public void StartGame()
    {
        Phase = GamePhase.Programming;
        Round = 1;
    }

    public void SetWinner(Player? winner)
    {
        Winner = winner;
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
    bool IsGameOver();
    IReadOnlyList<Player> GetWinners();

    void Initialize(GameMap map);
    void AddPlayer(string name);
    void StartGame();
    void GrantDamage(int playerId, int amount);
    void GrantRandomUpgrade(int playerId);
    void SetCurrentRegister(int register);
    void SetCurrentPlayerIndex(int index);
    void SetPhase(GamePhase phase);
    StepResult TryExecuteNextStep();
    void NextRegister();
    void NextRound();
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
    public event Action<RegisterExecutedEvent>? OnRegisterExecuted;
    public event Action<PhaseExecutionCompleteEvent>? OnPhaseExecutionComplete;
    public event Action? OnAllRegistersComplete;
    public event Action<GameOverEvent>? OnGameOver;
    public event Action<UpgradeAcquiredEvent>? OnUpgradeAcquired;

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

    public int CurrentPlayerIndex => State.CurrentPlayerIndex;
    public int CurrentRegister => State.CurrentRegister;
    public int Round => State.Round;
    public GamePhase Phase => State.Phase;
    public IReadOnlyList<Player> Players => State.Players;
    public GameMap Map => State.Map;

    public Player? GetCurrentPlayer()
    {
        return State.GetCurrentPlayer();
    }

    public Player? GetPlayer(int playerId)
    {
        return State.Players.FirstOrDefault(p => p.Id == playerId);
    }

    public void GrantDamage(int playerId, int amount)
    {
        var player = GetPlayer(playerId);
        if (player == null) return;
        player.Robot.TakeDamage(amount);
    }

    public void Initialize(GameMap map)
    {
        State.InitializeMap(map);
        State.SetPhase(GamePhase.Programming);
    }

    public void AddPlayer(string name)
    {
        var player = new Player
        {
            Id = State.Players.Count + 1,
            Name = name,
            Robot = new Robot
            {
                Id = State.Players.Count + 1,
                Name = name
            }
        };
        State.AddPlayer(player);
        
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
        State.StartGame();
        DealCardsToAllPlayers();
    }

    public void GrantUpgrade(int playerId, string upgradeId)
    {
        var player = State.GetPlayer(playerId);
        if (player == null)
            return;

        var definition = UpgradeCatalog.Get(upgradeId);
        if (definition == null)
            return;

        var effect = UpgradeCatalog.CreateEffect(definition);
        player.AddUpgrade(effect);
        OnUpgradeAcquired?.Invoke(new UpgradeAcquiredEvent(playerId, upgradeId));
    }

    public void GrantRandomUpgrade(int playerId)
    {
        var upgrade = UpgradeCatalog.AllUpgrades
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (upgrade != null)
        {
            GrantUpgrade(playerId, upgrade.Id);
        }
    }

    public void SetCurrentRegister(int register)
    {
        State.SetCurrentRegister(register);
        _currentStep = -1;
    }

    public void SetCurrentPlayerIndex(int index)
    {
        State.SetCurrentPlayerIndex(index);
    }

    public void IncrementCurrentPlayerIndex()
    {
        var nextIndex = State.CurrentPlayerIndex + 1;
        if (nextIndex >= State.Players.Count)
            nextIndex = 0;
        State.SetCurrentPlayerIndex(nextIndex);
    }

    public void SetPhase(GamePhase phase)
    {
        State.SetPhase(phase);
    }

    private void BeginRegisterExecution()
    {
        var registerIndex = State.CurrentRegister;
        
        _currentRegister = registerIndex;
        _alivePlayers = State.Players.Where(p => p.Robot.IsAlive).ToList();
        _movementStepIndex = 0;
        _conveyorExecuted = false;
        _laserExecuted = false;
        _currentStep = 0;
    }

    public StepResult TryExecuteNextStep()
    {
        if (_currentStep < 0)
        {
            BeginRegisterExecution();
        }
        
        var totalSteps = GetTotalSteps();
        if (_currentStep >= totalSteps)
        {
            OnPhaseExecutionComplete?.Invoke(new PhaseExecutionCompleteEvent(_currentRegister));
            CheckGameOver();
            return StepResult.Completed;
        }

        var movementSteps = _alivePlayers.Count;

        if (_currentStep < movementSteps)
        {
            if (_currentStep == 0)
                OnMovementPhaseStarted?.Invoke(new MovementPhaseStartedEvent(_currentRegister));
            ExecuteCards(_currentRegister);
            _currentStep = movementSteps;
        }
        else if (_currentStep == movementSteps && !_conveyorExecuted)
        {
            OnConveyorPhaseStarted?.Invoke(new ConveyorPhaseStartedEvent(_currentRegister));
            ApplyAllTileEffects();
            _conveyorExecuted = true;
            _currentStep++;
        }
        else if (_currentStep > movementSteps && !_laserExecuted)
        {
            OnLaserPhaseStarted?.Invoke(new LaserPhaseStartedEvent(_currentRegister));
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
        var before = CaptureRobotStates();
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
        CheckDamageOrDestroy(before);
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

    private Dictionary<int, (RobotStatus Status, int Damage, int LifeTokens)> CaptureRobotStates()
    {
        var states = new Dictionary<int, (RobotStatus, int, int)>();
        foreach (var player in State.Players)
        {
            states[player.Robot.Id] = (player.Robot.Status, player.Robot.Damage, player.Robot.LifeTokens);
        }
        return states;
    }

    private Dictionary<int, int> CaptureDamageStates()
    {
        return State.Players.ToDictionary(p => p.Robot.Id, p => p.Robot.Damage);
    }

    private void CheckDamageOrDestroy(Dictionary<int, (RobotStatus Status, int Damage, int LifeTokens)> before)
    {
        foreach (var player in State.Players)
        {
            var id = player.Robot.Id;
            if (before.TryGetValue(id, out var prev))
            {
                var currentDamage = player.Robot.Damage;
                
                if (currentDamage != prev.Damage)
                {
                    OnDamage?.Invoke(new DamageEvent(id, currentDamage - prev.Damage, currentDamage));
                }
                
                if (prev.LifeTokens > player.Robot.LifeTokens)
                {
                    if (player.Robot.LifeTokens == 0)
                    {
                        OnRobotDestroyed?.Invoke(new RobotDestroyedEvent(id, player.Robot.Position));
                    }
                    else
                    {
                        OnRobotReboot?.Invoke(new RobotRebootEvent(id, player.Robot.Position));
                    }
                    OnLifeTokenLost?.Invoke(new LifeTokenLostEvent(player.Robot.Id, player.Robot.LifeTokens));
                }
            }
        }
    }
    
    private void FireMoveAndRotateEvents(
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
                    OnMove?.Invoke(new MoveEvent(id, b.Pos, a.Pos));
                }
                if (b.Dir != a.Dir)
                {
                    OnRotate?.Invoke(new RotateEvent(id, b.Dir, a.Dir));
                }
            }
        }
    }
    
    private void ExecuteCards(int registerIndex)
    {
        var before = CaptureStates();
        var statusBefore = CaptureRobotStates();
        var moveIntents = new List<MoveIntent>();
        
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive || player.Robot.NeedsReboot)
                continue;

            if (registerIndex < player.Robot.LockedRegisters)
                continue;

            var card = player.GetRegister(registerIndex);
            if (card == null)
                continue;

            var intent = card.Card.CreateMoveIntent(player.Robot, State.Map, registerIndex);
            if (intent != null)
            {
                moveIntents.Add(intent);
            }
        }
        
        if (moveIntents.Count > 0)
        {
            var resolver = new CollisionResolver();
            foreach (var intent in moveIntents)
                resolver.AddIntent(intent);
            
            resolver.Resolve(State.Map, State.Players.Select(p => p.Robot).ToList());
        }
        
        for (int i = 0; i < State.Players.Count; i++)
        {
            var player = State.Players[i];
            var card = player.GetRegister(registerIndex);
            if (card != null && player.Robot.IsAlive)
            {
                OnRegisterExecuted?.Invoke(new RegisterExecutedEvent(registerIndex, i));
            }
        }
        
        var after = CaptureStates();
        FireMoveAndRotateEvents(before, after);
        CheckDamageOrDestroy(statusBefore);
    }

    private void ApplyAllTileEffects()
    {
        var statusBefore = CaptureRobotStates();
        var directionsBefore = State.Players.ToDictionary(p => p.Robot.Id, p => p.Robot.Direction);
        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        var mapMovementIntents = new List<MoveIntent>();

        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var tile = State.Map.GetTile(player.Robot.Position);
            var effect = tile.GetComponent<ITileBehaviorComponent>()?.ApplyEffect(player, State.Map);

            if (!effect.HasValue)
                continue;

            if (effect?.Intent != null)
            {
                mapMovementIntents.Add(effect.Value.Intent);
            }

            switch (effect?.ResultKind)
            {
                case TileActionResult.CheckpointReached:
                    OnCheckpoint?.Invoke(new CheckpointEvent(player.Robot.Id, player.Robot.CheckpointsReached, totalCheckpoints));
                    break;
                case TileActionResult.FellIntoPit:
                    break;
            }
        }

        if (mapMovementIntents.Count > 0)
        {
            var resolver = new CollisionResolver();
            foreach (var intent in mapMovementIntents)
                resolver.AddIntent(intent);

            var result = resolver.Resolve(State.Map, State.Players.Select(p => p.Robot).ToList());

            foreach (var movement in result.Movements)
            {
                if (movement.Result != CollisionResult.Blocked)
                    OnConveyorMove?.Invoke(new ConveyorMoveEvent(movement.RobotId, movement.From, movement.To));
            }
        }

        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            if (directionsBefore.TryGetValue(player.Robot.Id, out var beforeDir) && beforeDir != player.Robot.Direction)
            {
                OnRotate?.Invoke(new RotateEvent(player.Robot.Id, beforeDir, player.Robot.Direction));
            }
        }

        CheckDamageOrDestroy(statusBefore);
    }

    private void CheckGameOver()
    {
        if (IsGameOver() && State.Phase != GamePhase.GameOver)
        {
            State.SetPhase(GamePhase.GameOver);
            var winners = GetWinners();
            OnGameOver?.Invoke(new GameOverEvent(
                winners.Select(w => w.Id).ToList(),
                winners.Count > 1));
        }
    }

    public void NextRegister()
    {
        var nextRegister = State.CurrentRegister + 1;
        if (nextRegister >= 5)
        {
            nextRegister = 0;
        }
        State.SetCurrentRegister(nextRegister);
        _currentStep = -1;
        if (nextRegister == 0)
        {
            OnAllRegistersComplete?.Invoke();
            NextRound();
        }
    }

    public void NextRound()
    {
        State.IncrementRound();
        State.SetCurrentRegister(0);
        State.SetCurrentPlayerIndex(0);
        State.SetPhase(GamePhase.Programming);
        
        foreach (var player in State.Players)
        {
            //TODO: вынести в компоненты поведения робота, сделать реакцию на начало раунда
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
                var spawnPos = player.Robot.LastCheckpointPosition ?? player.Robot.StartPosition;
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
        var isOnlyOneRemainsOrAllDead = State.Players.Count(p=>p.Robot.IsAlive) <= 1;
        if (isOnlyOneRemainsOrAllDead) return true;

        var totalCheckpoints = State.Map.GetTotalCheckpoints();
        return State.Players.Any(p => p.Robot.CheckpointsReached >= totalCheckpoints);
    }

    public IReadOnlyList<Player> GetWinners()
    {
        var livePlayers = State.Players.Where(p=>p.Robot.IsAlive).ToArray();
        if (livePlayers.Length == 1)
        {
            return [livePlayers[0]];
        }

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

    private static List<ProgramCard> CreateDeck()
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

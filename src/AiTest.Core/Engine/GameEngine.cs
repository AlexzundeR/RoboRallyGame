namespace AiTest.Core.Engine;

using AiTest.Core.Entities;
using AiTest.Core.Models;
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
    public GameMap Map { get; set; } = null!;
    public List<Player> Players { get; } = new();
    public int CurrentPlayerIndex { get; set; }
    public int CurrentRegister { get; set; }
    public GamePhase Phase { get; set; } = GamePhase.Setup;
    public int Round { get; set; }
    public Player? Winner { get; set; }

    public Player AddPlayer(string name)
    {
        var player = new Player
        {
            Id = Players.Count + 1,
            Name = name,
            Robot = new Robot
            {
                Id = Players.Count + 1,
                Name = name
            }
        };
        Players.Add(player);
        return player;
    }

    public Player? GetCurrentPlayer()
    {
        return CurrentPlayerIndex >= 0 && CurrentPlayerIndex < Players.Count
            ? Players[CurrentPlayerIndex]
            : null;
    }

    public void Clear()
    {
        Players.Clear();
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
    void ExecuteRegister(int registerIndex);
    void ProcessBoardElements();
    void FireLasers();
    void NextRegister();
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

    public GameEngine() : this(new RandomDealer()) { }
    
    public GameEngine(IDealer dealer)
    {
        _dealer = dealer;
    }

    public IDealer Dealer
    {
        get => _dealer;
        set => _dealer = value ?? throw new ArgumentNullException(nameof(value));
    }

    public GameState State { get; } = new();

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
            player.Robot.Position = startTiles[player.Id - 1];
        }
    }

    public void StartGame()
    {
        State.Phase = GamePhase.Programming;
        State.Round = 1;
        DealCardsToAllPlayers();
    }

    public void ExecuteRegister(int registerIndex)
    {
        Phase1_CardExecution(registerIndex);
        Phase2_BoardElements();
        Phase3_Lasers();
    }

    private void Phase1_CardExecution(int registerIndex)
    {
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

            if (card is TurnCard turnCard)
            {
                if (turnCard.IsLeft)
                    player.Robot.RotateLeft();
                else
                    player.Robot.RotateRight();
                
                cardExecutionStrategy.ExecuteRotation(player.Robot);
                continue;
            }
            
            if (card is UTurnCard)
            {
                player.Robot.Rotate180();
                cardExecutionStrategy.ExecuteRotation(player.Robot);
                continue;
            }

            if (card is BackUpCard)
            {
                var targetPos = player.Robot.Position.Move(player.Robot.Direction.TurnAround());
                if (State.Map.IsInBounds(targetPos) && State.Map.GetTile(targetPos).Type != TileType.Wall)
                {
                    moveIntents.Add(new MoveIntent
                    {
                        Robot = player.Robot,
                        From = player.Robot.Position,
                        To = targetPos,
                        Direction = player.Robot.Direction,
                        Priority = card.Priority,
                        RegisterIndex = registerIndex,
                        Type = MoveType.Normal
                    });
                }
                continue;
            }

            if (card is MoveCard moveCard)
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

    private Position CalculateCardTarget(Robot robot, ProgramCard card)
    {
        return card switch
        {
            MoveCard m => CalculateMoveTarget(robot, m.Spaces),
            TurnCard t => robot.Position,
            UTurnCard => robot.Position,
            _ => robot.Position
        };
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
            var next = pos.Move(robot.Direction);
            if (State.Map.IsInBounds(next))
            {
                var tile = State.Map.GetTile(next);
                if (tile.Type == TileType.Wall || tile.Type == TileType.Pit)
                    break;
                if (!movementStrategy.CanEnterTile(robot, State.Map, next))
                    break;
                pos = next;
            }
        }
        return movementStrategy.CalculateMove(robot, State.Map, pos);
    }

    public void ProcessBoardElements()
    {
        Phase2_BoardElements();
    }

    private void Phase2_BoardElements()
    {
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var conveyorStrategy = GetConveyorStrategy(player);
            var ignoreConveyors = conveyorStrategy.ShouldIgnoreConveyors(player.Robot);
            var ignorePits = conveyorStrategy.ShouldIgnorePits(player.Robot);
            
            var tile = State.Map.GetTile(player.Robot.Position);

            switch (tile.Type)
            {
                case TileType.Pit when !ignorePits:
                    HandlePit(player);
                    break;
                case TileType.GearCW:
                    player.Robot.RotateRight();
                    break;
                case TileType.GearCCW:
                    player.Robot.RotateLeft();
                    break;
                case TileType.ConveyorCW when !ignoreConveyors:
                case TileType.ExpressConveyorCW when !ignoreConveyors:
                    MoveOnConveyor(player, tile.ConveyorDir!.Value, tile.Type == TileType.ExpressConveyorCW ? 2 : 1);
                    break;
                case TileType.ConveyorCCW when !ignoreConveyors:
                case TileType.ExpressConveyorCCW when !ignoreConveyors:
                    MoveOnConveyor(player, tile.ConveyorDir!.Value, tile.Type == TileType.ExpressConveyorCCW ? 2 : 1);
                    break;
                case TileType.Checkpoint:
                    HandleCheckpoint(player, tile.CheckpointNumber ?? 0);
                    break;
            }
        }
    }

    private IConveyorStrategy GetConveyorStrategy(Player player)
    {
        var strategies = new List<IConveyorStrategy> { DefaultConveyorStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetConveyorStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeConveyorStrategy(strategies);
    }

    public void FireLasers()
    {
        Phase3_Lasers();
    }

    private void Phase3_Lasers()
    {
        var shots = _laserResolver.PrepareShots(State.Map, State.Players);
        _laserResolver.Fire(shots, State.Players);
    }

    private void HandlePit(Player player)
    {
        player.Robot.Status = RobotStatus.Rebooting;
    }

    private void HandleCheckpoint(Player player, int checkpointNum)
    {
        if (checkpointNum == player.Robot.CheckpointsReached + 1)
        {
            player.Robot.CheckpointsReached = checkpointNum;
        }
    }

    private void MoveOnConveyor(Player player, ConveyorDirection dir, int spaces)
    {
        var direction = dir switch
        {
            ConveyorDirection.North => Direction.North,
            ConveyorDirection.East => Direction.East,
            ConveyorDirection.South => Direction.South,
            ConveyorDirection.West => Direction.West,
            _ => Direction.North
        };

        for (int i = 0; i < spaces; i++)
        {
            var newPos = player.Robot.Position.Move(direction);
            if (State.Map.IsInBounds(newPos))
            {
                var targetTile = State.Map.GetTile(newPos);
                if (targetTile.Type != TileType.Wall && targetTile.Type != TileType.Pit)
                {
                    player.Robot.Position = newPos;
                }
                else
                {
                    break;
                }
            }
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
                player.Robot.Damage = 0;
            }
            player.ClearRegisters();
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

namespace RoboRallyGame.Core.Models.Components;

using RoboRallyGame.Core.Engine;

public enum TileActionResult
{
    None,
    Moved,
    CheckpointReached,
    Repaired,
    FellIntoPit,
    ReadyToFall
}

public readonly record struct TileEffect(MoveIntent? Intent, TileActionResult ResultKind);

public abstract class TileComponentBase : ITileComponent
{
    public Tile Tile { get; private set; }
    public Position Position => Tile.Position;

    protected TileComponentBase() { }

    public void SetTile(Tile tile) => Tile = tile;
}

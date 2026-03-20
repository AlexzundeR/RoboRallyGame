namespace AiTest.Godot.Tiles;

public partial class WallTile : Tile
{
    protected override Color BaseColor => new(0.4f, 0.4f, 0.45f);
    protected override Color BorderColor => new(0.5f, 0.5f, 0.55f);
}

namespace AiTest.Godot.Animations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public partial class AnimationService : Node, IAnimationService
{
    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    public async Task PlayAndWait(IEnumerable<Animation> animations)
    {
        var list = animations.ToList();
        if (list.Count == 0) return;

        var tasks = list.Select(a => a.PlayAsync(this)).ToList();
        await Task.WhenAll(tasks);
    }

    public static Vector2 PositionToVector(Position pos)
    {
        return new Vector2(pos.X * TileSize + 10, pos.Y * TileSize + MapOffsetY + 10);
    }

    public static Vector2 PositionToVectorCentered(Position pos)
    {
        return new Vector2(pos.X * TileSize + TileSize / 2, pos.Y * TileSize + MapOffsetY + TileSize / 2);
    }

    public static float DirectionToAngle(Direction dir) => dir switch
    {
        Direction.North => 0f,
        Direction.East => 90f,
        Direction.South => 180f,
        Direction.West => 270f,
        _ => 0f
    };
}

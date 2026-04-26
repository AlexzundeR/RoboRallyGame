namespace RoboRallyGame.Godot.Animations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

public partial class AnimationService : Node, IAnimationService
{
    public async Task PlayAndWait(IEnumerable<Animation> animations)
    {
        var list = animations.ToList();
        if (list.Count == 0) return;

        var tasks = list.Select(a => a.PlayAsync(this)).ToList();
        await Task.WhenAll(tasks);
    }

    public static Vector2 PositionToVector(Position pos)
    {
        return GameScale.GridToWorldCentered(pos.X, pos.Y);
    }

    public static Vector2 PositionToVectorCentered(Position pos)
    {
        return PositionToVector(pos);
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

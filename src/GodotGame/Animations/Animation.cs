namespace RoboRallyGame.Godot.Animations;

using System.Threading.Tasks;
using Godot;

public abstract class Animation
{
    public abstract Task PlayAsync(Node context);
}

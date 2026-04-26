namespace RoboRallyGame.Godot;

using System.Collections.Generic;
using System.Threading.Tasks;
using RoboRallyGame.Godot.Animations;

public interface IAnimationService
{
    Task PlayAndWait(IEnumerable<Animation> animations);
}

namespace AiTest.Godot;

using System.Collections.Generic;
using System.Threading.Tasks;
using AiTest.Godot.Animations;

public interface IAnimationService
{
    Task PlayAndWait(IEnumerable<Animation> animations);
}

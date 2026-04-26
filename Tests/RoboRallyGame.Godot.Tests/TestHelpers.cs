using RoboRallyGame.Godot;
using RoboRallyGame.Core.Engine;

namespace RoboRallyGame.Godot.Tests;

public static class TestHelpers
{
    public static IGameProvider CreateProvider(IGameEngine engine) => new LocalGameProvider(engine);
    public static IGameProvider CreateProvider(GameRunner runner) => new LocalGameProvider(runner.Engine);
}
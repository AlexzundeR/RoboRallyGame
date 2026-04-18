using AiTest.Godot;
using AiTest.Core.Engine;

namespace AiTest.Godot.Tests;

public static class TestHelpers
{
    public static IGameProvider CreateProvider(IGameEngine engine) => new LocalGameProvider(engine);
    public static IGameProvider CreateProvider(GameRunner runner) => new LocalGameProvider(runner.Engine);
}
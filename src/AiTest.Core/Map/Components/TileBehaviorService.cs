namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;

public class TileBehaviorService
{
    public void ApplyPostMovement(Core.Models.Tile tile, Robot robot)
    {
        var behavior = tile.GetComponent<ITileBehaviorComponent>();
        behavior?.ApplyPostMovement(robot);
    }
}

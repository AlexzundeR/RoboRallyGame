namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;

public interface ITileComponent
{
}

public interface IEndOfRoundComponent : ITileComponent
{
    TileActionResult ApplyEndOfRound(Player player);
}

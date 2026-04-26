namespace RoboRallyGame.Core.Models.Components;

using RoboRallyGame.Core.Entities;

public interface ITileComponent
{
}

public interface IEndOfRoundComponent : ITileComponent
{
    TileActionResult ApplyEndOfRound(Player player);
}

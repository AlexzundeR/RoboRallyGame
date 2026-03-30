namespace AiTest.Godot;

using System.Collections.Generic;
using AiTest.Core.Config;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public static class GameMapConfigs
{
    public static GameConfiguration CreateDefault2Player()
    {
        return new GameConfiguration
        {
            MapWidth = 15,
            MapHeight = 15,
            Players = new List<PlayerConfig>
            {
                new() { Name = "Player1", StartPosition = new Position(2, 2) },
                new() { Name = "Player2", StartPosition = new Position(12, 2) }
            },
            Map = new MapConfig
            {
                Checkpoints = new List<CheckpointPlacement>
                {
                    new() { X = 6, Y = 6, Number = 1 },
                    new() { X = 7, Y = 6, Number = 2 },
                    new() { X = 6, Y = 7, Number = 3 },
                    new() { X = 7, Y = 7, Number = 4 }
                },
                Starts = new List<StartPlacement>
                {
                    new() { X = 2, Y = 2 },
                    new() { X = 12, Y = 2 },
                    new() { X = 2, Y = 12 },
                    new() { X = 12, Y = 12 }
                },
                Tiles = new List<TilePlacement>
                {
                    // Walls
                    new() { X = 0, Y = 0, Type = TileType.Wall },
                    new() { X = 1, Y = 0, Type = TileType.Wall },
                    new() { X = 2, Y = 0, Type = TileType.Wall },
                    new() { X = 3, Y = 0, Type = TileType.Wall },
                    new() { X = 4, Y = 0, Type = TileType.Wall },
                    new() { X = 5, Y = 0, Type = TileType.Wall },
                    new() { X = 6, Y = 0, Type = TileType.Wall },
                    new() { X = 7, Y = 0, Type = TileType.Wall },
                    new() { X = 8, Y = 0, Type = TileType.Wall },
                    new() { X = 9, Y = 0, Type = TileType.Wall },
                    new() { X = 10, Y = 0, Type = TileType.Wall },
                    new() { X = 11, Y = 0, Type = TileType.Wall },
                    new() { X = 12, Y = 0, Type = TileType.Wall },
                    new() { X = 13, Y = 0, Type = TileType.Wall },
                    new() { X = 14, Y = 0, Type = TileType.Wall },

                    new() { X = 0, Y = 14, Type = TileType.Wall },
                    new() { X = 1, Y = 14, Type = TileType.Wall },
                    new() { X = 2, Y = 14, Type = TileType.Wall },
                    new() { X = 3, Y = 14, Type = TileType.Wall },
                    new() { X = 4, Y = 14, Type = TileType.Wall },
                    new() { X = 5, Y = 14, Type = TileType.Wall },
                    new() { X = 6, Y = 14, Type = TileType.Wall },
                    new() { X = 7, Y = 14, Type = TileType.Wall },
                    new() { X = 8, Y = 14, Type = TileType.Wall },
                    new() { X = 9, Y = 14, Type = TileType.Wall },
                    new() { X = 10, Y = 14, Type = TileType.Wall },
                    new() { X = 11, Y = 14, Type = TileType.Wall },
                    new() { X = 12, Y = 14, Type = TileType.Wall },
                    new() { X = 13, Y = 14, Type = TileType.Wall },
                    new() { X = 14, Y = 14, Type = TileType.Wall },

                    new() { X = 0, Y = 1, Type = TileType.Wall },
                    new() { X = 0, Y = 2, Type = TileType.Wall },
                    new() { X = 0, Y = 3, Type = TileType.Wall },
                    new() { X = 0, Y = 4, Type = TileType.Wall },
                    new() { X = 0, Y = 5, Type = TileType.Wall },
                    new() { X = 0, Y = 6, Type = TileType.Wall },
                    new() { X = 0, Y = 7, Type = TileType.Wall },
                    new() { X = 0, Y = 8, Type = TileType.Wall },
                    new() { X = 0, Y = 9, Type = TileType.Wall },
                    new() { X = 0, Y = 10, Type = TileType.Wall },
                    new() { X = 0, Y = 11, Type = TileType.Wall },
                    new() { X = 0, Y = 12, Type = TileType.Wall },
                    new() { X = 0, Y = 13, Type = TileType.Wall },

                    new() { X = 14, Y = 1, Type = TileType.Wall },
                    new() { X = 14, Y = 2, Type = TileType.Wall },
                    new() { X = 14, Y = 3, Type = TileType.Wall },
                    new() { X = 14, Y = 4, Type = TileType.Wall },
                    new() { X = 14, Y = 5, Type = TileType.Wall },
                    new() { X = 14, Y = 6, Type = TileType.Wall },
                    new() { X = 14, Y = 7, Type = TileType.Wall },
                    new() { X = 14, Y = 8, Type = TileType.Wall },
                    new() { X = 14, Y = 9, Type = TileType.Wall },
                    new() { X = 14, Y = 10, Type = TileType.Wall },
                    new() { X = 14, Y = 11, Type = TileType.Wall },
                    new() { X = 14, Y = 12, Type = TileType.Wall },
                    new() { X = 14, Y = 13, Type = TileType.Wall },

                    // Conveyors (speed 1)
                    new() { X = 4, Y = 3, Type = TileType.Conveyor, ConveyorDir = Direction.East },
                    new() { X = 5, Y = 3, Type = TileType.Conveyor, ConveyorDir = Direction.East },
                    new() { X = 6, Y = 3, Type = TileType.Conveyor, ConveyorDir = Direction.East },
                    new() { X = 9, Y = 3, Type = TileType.Conveyor, ConveyorDir = Direction.West },
                    new() { X = 10, Y = 3, Type = TileType.Conveyor, ConveyorDir = Direction.West },

                    new() { X = 4, Y = 11, Type = TileType.Conveyor, ConveyorDir = Direction.East },
                    new() { X = 10, Y = 11, Type = TileType.Conveyor, ConveyorDir = Direction.West },

                    // South/North conveyors (speed 1)
                    new() { X = 7, Y = 5, Type = TileType.Conveyor, ConveyorDir = Direction.South },
                    new() { X = 7, Y = 6, Type = TileType.Conveyor, ConveyorDir = Direction.South },
                    new() { X = 7, Y = 8, Type = TileType.Conveyor, ConveyorDir = Direction.North },
                    new() { X = 7, Y = 9, Type = TileType.Conveyor, ConveyorDir = Direction.North },

                    // Express Conveyors (speed 2)
                    new() { X = 3, Y = 5, Type = TileType.Conveyor, ConveyorDir = Direction.South, IsExpress = true },
                    new() { X = 3, Y = 6, Type = TileType.Conveyor, ConveyorDir = Direction.South, IsExpress = true },
                    new() { X = 3, Y = 8, Type = TileType.Conveyor, ConveyorDir = Direction.North, IsExpress = true },
                    new() { X = 3, Y = 9, Type = TileType.Conveyor, ConveyorDir = Direction.North, IsExpress = true },

                    new() { X = 11, Y = 5, Type = TileType.Conveyor, ConveyorDir = Direction.South, IsExpress = true },
                    new() { X = 11, Y = 6, Type = TileType.Conveyor, ConveyorDir = Direction.South, IsExpress = true },
                    new() { X = 11, Y = 8, Type = TileType.Conveyor, ConveyorDir = Direction.North, IsExpress = true },
                    new() { X = 11, Y = 9, Type = TileType.Conveyor, ConveyorDir = Direction.North, IsExpress = true },

                    // More variety - East/West express
                    new() { X = 5, Y = 5, Type = TileType.Conveyor, ConveyorDir = Direction.East, IsExpress = true },
                    new() { X = 5, Y = 9, Type = TileType.Conveyor, ConveyorDir = Direction.West, IsExpress = true },
                    new() { X = 9, Y = 5, Type = TileType.Conveyor, ConveyorDir = Direction.East, IsExpress = true },
                    new() { X = 9, Y = 9, Type = TileType.Conveyor, ConveyorDir = Direction.West, IsExpress = true },

                    // Gears
                    new() { X = 10, Y = 4, Type = TileType.Gear, IsClockwise = true },
                    new() { X = 10, Y = 5, Type = TileType.Gear, IsClockwise = false },
                    new() { X = 4, Y = 9, Type = TileType.Gear, IsClockwise = false },
                    new() { X = 4, Y = 10, Type = TileType.Gear, IsClockwise = true },

                    // Lasers (horizontal: West/East, vertical: North/South)
                    new() { X = 2, Y = 5, Type = TileType.Laser, LaserDirection = Direction.West },
                    new() { X = 5, Y = 2, Type = TileType.Laser, LaserDirection = Direction.North },
                    new() { X = 9, Y = 2, Type = TileType.Laser, LaserDirection = Direction.North },
                    new() { X = 12, Y = 5, Type = TileType.Laser, LaserDirection = Direction.East },
                    new() { X = 2, Y = 9, Type = TileType.Laser, LaserDirection = Direction.West },
                    new() { X = 12, Y = 9, Type = TileType.Laser, LaserDirection = Direction.East },

                    // Pits
                    new() { X = 5, Y = 12, Type = TileType.Pit },
                    new() { X = 9, Y = 12, Type = TileType.Pit },

                    // Repair Sites
                    new() { X = 7, Y = 3, Type = TileType.RepairSite },
                    new() { X = 7, Y = 11, Type = TileType.RepairSitePlus },

                    // Barricade and Tunnels (using Floor with blocked directions)
                    new() { X = 7, Y = 5, Type = TileType.Floor, BlockedDirections = new List<Direction> { Direction.North, Direction.East, Direction.West } },
                    new() { X = 5, Y = 9, Type = TileType.Floor, BlockedDirections = new List<Direction> { Direction.North, Direction.South } },
                    new() { X = 9, Y = 5, Type = TileType.Floor, BlockedDirections = new List<Direction> { Direction.East, Direction.West } }
                }
            }
        };
    }
}

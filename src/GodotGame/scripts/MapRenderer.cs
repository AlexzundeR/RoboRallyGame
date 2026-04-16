using Godot;
using System.Collections.Generic;
using AiTest.Godot.Tiles;
using AiTest.Godot.Components;
using AiTest.Godot.Services;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

namespace AiTest.Godot;

public partial class MapRenderer : Node2D
{
    private readonly Dictionary<Position, Tile> _tiles = new();
    
    private PackedScene _floorTileScene = null!;
    private PackedScene _wallTileScene = null!;
    private PackedScene _pitTileScene = null!;
    private PackedScene _conveyorTileScene = null!;
    private PackedScene _gearTileScene = null!;
    private PackedScene _laserTileScene = null!;
    private PackedScene _startTileScene = null!;
    private PackedScene _checkpointTileScene = null!;
    private PackedScene _barricadeRendererScene = null!;

    public override void _Ready()
    {
        _floorTileScene = GD.Load<PackedScene>("res://Tiles/FloorTile.tscn");
        _wallTileScene = GD.Load<PackedScene>("res://Tiles/WallTile.tscn");
        _pitTileScene = GD.Load<PackedScene>("res://Tiles/PitTile.tscn");
        _conveyorTileScene = GD.Load<PackedScene>("res://Tiles/ConveyorTile.tscn");
        _gearTileScene = GD.Load<PackedScene>("res://Tiles/GearTile.tscn");
        _laserTileScene = GD.Load<PackedScene>("res://Tiles/LaserTile.tscn");
        _startTileScene = GD.Load<PackedScene>("res://Tiles/StartTile.tscn");
        _checkpointTileScene = GD.Load<PackedScene>("res://Tiles/CheckpointTile.tscn");
        _barricadeRendererScene = GD.Load<PackedScene>("res://Components/BarricadeRenderer.tscn");
    }

    public void RenderMap(GameMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);
                CreateTileVisual(pos, tile);
            }
        }
    }

    private void CreateTileVisual(Position pos, Core.Models.Tile tileData)
    {
        var scene = GetSceneForTileType(tileData.Type);
        var tileNode = scene.Instantiate<Tile>();
        
        tileNode.Initialize(pos, tileData.Type);
        _tiles[pos] = tileNode;
        AddChild(tileNode);

        if (tileData.Type == TileType.Checkpoint)
        {
            if (tileNode is CheckpointTile checkpointTile)
            {
                checkpointTile.Initialize(pos, tileData.Type, tileData.CheckpointNumber ?? 1);
            }
        }
        
        if (tileData.Type == TileType.Conveyor)
        {
            if (tileNode is ConveyorTile conveyorTile)
            {
                var conveyor = tileData.GetComponent<ConveyorBehaviorComponent>();
                conveyorTile.Initialize(pos, tileData.Type, conveyor?.Direction ?? ConveyorDirection.North, conveyor?.IsExpress ?? false);
            }
        }
        
        if (tileData.Type == TileType.Gear)
        {
            if (tileNode is GearTile gearTile)
            {
                var gear = tileData.GetComponent<GearBehaviorComponent>();
                gearTile.Initialize(pos, tileData.Type, gear?.IsClockwise ?? true);
            }
        }
        
        var blockedComponent = tileData.GetComponent<BlockedMovementComponent>();
        if (blockedComponent != null && blockedComponent.BlockedDirections.Count > 0 && blockedComponent.BlockedDirections.Count < 4)
        {
            var barricadeRenderer = _barricadeRendererScene.Instantiate<BarricadeRenderer>();
            tileNode.AddChild(barricadeRenderer);
            barricadeRenderer.RenderBlockages(blockedComponent.BlockedDirections);
        }
        
        tileNode.StartAnimations();
    }

    private PackedScene GetSceneForTileType(TileType type)
    {
        return type switch
        {
            TileType.Floor => _floorTileScene,
            TileType.Wall => _wallTileScene,
            TileType.Pit => _pitTileScene,
            TileType.Conveyor => _conveyorTileScene,
            TileType.Gear => _gearTileScene,
            TileType.Laser => _laserTileScene,
            TileType.Start => _startTileScene,
            TileType.Checkpoint => _checkpointTileScene,
            _ => _floorTileScene
        };
    }
}

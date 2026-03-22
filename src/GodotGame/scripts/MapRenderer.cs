using Godot;
using System.Collections.Generic;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;
using AiTest.Godot.Tiles;
using AiTest.Godot.Components;

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
    
    private const int TileSize = 48;
    private const int MapOffsetY = 120;

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
        AddChild(tileNode);
        _tiles[pos] = tileNode;

        if (tileData.Type == TileType.Checkpoint)
        {
            if (tileNode is CheckpointTile checkpointTile)
            {
                checkpointTile.Initialize(pos, tileData.Type, tileData.CheckpointNumber ?? 1);
            }
        }
        
        if (tileData.Type == TileType.ConveyorCW || tileData.Type == TileType.ConveyorCCW ||
            tileData.Type == TileType.ExpressConveyorCW || tileData.Type == TileType.ExpressConveyorCCW)
        {
            if (tileNode is ConveyorTile conveyorTile)
            {
                conveyorTile.Initialize(pos, tileData.Type);
            }
        }
        
        if (tileData.Type == TileType.GearCW || tileData.Type == TileType.GearCCW)
        {
            if (tileNode is GearTile gearTile)
            {
                gearTile.Initialize(pos, tileData.Type);
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
            TileType.ConveyorCW or TileType.ConveyorCCW or 
            TileType.ExpressConveyorCW or TileType.ExpressConveyorCCW => _conveyorTileScene,
            TileType.GearCW or TileType.GearCCW => _gearTileScene,
            TileType.LaserH or TileType.LaserV => _laserTileScene,
            TileType.Start => _startTileScene,
            TileType.Checkpoint => _checkpointTileScene,
            _ => _floorTileScene
        };
    }
}

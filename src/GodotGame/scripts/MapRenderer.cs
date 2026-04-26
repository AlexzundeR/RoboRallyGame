using Godot;
using System.Collections.Generic;
using RoboRallyGame.Godot.Tiles;
using RoboRallyGame.Godot.Components;
using RoboRallyGame.Godot.Models;
using RoboRallyGame.Godot;

public partial class MapRenderer : Node2D
{
    private readonly Dictionary<(int, int), Tile> _tiles = new();
    
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

    public void RenderMap(MapData map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tileData = map.GetTile(x, y);
                CreateTileVisual(x, y, tileData);
            }
        }
    }

    private void CreateTileVisual(int x, int y, RoboRallyGame.Godot.Models.TileData tileData)
    {
        var scene = GetSceneForTileType(tileData.Type);
        var tileNode = scene.Instantiate<Tile>();
        
        tileNode.Initialize(x, y, (int)tileData.Type);
        _tiles[(x, y)] = tileNode;
        AddChild(tileNode);

        if (tileData.Type == TileType.Checkpoint)
        {
            if (tileNode is CheckpointTile checkpointTile)
            {
                checkpointTile.Initialize(x, y, (int)tileData.Type, tileData.CheckpointNumber ?? 1);
            }
        }
        
        if (tileData.Type == TileType.Conveyor)
        {
            if (tileNode is ConveyorTile conveyorTile)
            {
                conveyorTile.Initialize(x, y, (int)tileData.Type, tileData.ConveyorDir ?? ConveyorDirection.North, tileData.IsExpress ?? false);
            }
        }
        
        if (tileData.Type == TileType.Gear)
        {
            if (tileNode is GearTile gearTile)
            {
                gearTile.Initialize(x, y, (int)tileData.Type, tileData.IsClockwise ?? true);
            }
        }
        
        if (tileData.BlockedDirections != null && tileData.BlockedDirections.Count > 0 && tileData.BlockedDirections.Count < 4)
        {
            var barricadeRenderer = _barricadeRendererScene.Instantiate<BarricadeRenderer>();
            tileNode.AddChild(barricadeRenderer);
            barricadeRenderer.RenderBlockages(tileData.BlockedDirections);
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

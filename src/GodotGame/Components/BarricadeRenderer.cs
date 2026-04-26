namespace RoboRallyGame.Godot.Components;

using System;
using System.Collections.Generic;
using Godot;

public partial class BarricadeRenderer : Node2D
{
    private const float StripeWidth = 6f;
    private const int StripeCount = 8;
    
    private readonly Dictionary<int, List<Polygon2D>> _stripesByDirection = new();

    private static readonly Color Yellow = new(0.9f, 0.7f, 0, 1);
    private static readonly Color Black = new(0.05f, 0.05f, 0.05f, 1);

    public override void _Ready()
    {
        CreateEdgeStripes(0, 0);
        CreateEdgeStripes(2, 48);
        CreateEdgeStripes(1, 48);
        CreateEdgeStripes(3, 0);
        
        HideAll();
    }

    private void CreateEdgeStripes(int direction, float edgePos)
    {
        var stripes = new List<Polygon2D>();
        
        for (int i = 0; i < 8; i++)
        {
            bool isYellow = i % 2 == 0;
            
            Polygon2D stripe = direction switch
            {
                0 => CreateNorthStripe(edgePos, i, isYellow),
                2 => CreateSouthStripe(edgePos, i, isYellow),
                1 => CreateEastStripe(edgePos, i, isYellow),
                3 => CreateWestStripe(edgePos, i, isYellow),
                _ => throw new ArgumentException($"Unknown direction: {direction}")
            };
            
            AddChild(stripe);
            stripes.Add(stripe);
        }
        
        _stripesByDirection[direction] = stripes;
    }

    private Polygon2D CreateNorthStripe(float edgeY, int index, bool isYellow)
    {
        float startX = index * StripeWidth;
        
        var points = new Vector2[]
        {
            new Vector2(startX, 0),
            new Vector2(startX + StripeWidth, StripeWidth),
            new Vector2(startX + StripeWidth, 0),
            new Vector2(startX, 0)
        };

        return new Polygon2D { Polygon = points, Color = isYellow ? Yellow : Black };
    }

    private Polygon2D CreateSouthStripe(float edgeY, int index, bool isYellow)
    {
        float startX = index * StripeWidth;
        
        var points = new Vector2[]
        {
            new Vector2(startX, 48 - StripeWidth),
            new Vector2(startX + StripeWidth, 48),
            new Vector2(startX + StripeWidth, 48 - StripeWidth),
            new Vector2(startX, 48 - StripeWidth)
        };

        return new Polygon2D { Polygon = points, Color = isYellow ? Yellow : Black };
    }

    private Polygon2D CreateEastStripe(float edgeX, int index, bool isYellow)
    {
        float startY = index * StripeWidth;
        
        var points = new Vector2[]
        {
            new Vector2(48 - StripeWidth, startY),
            new Vector2(48, startY + StripeWidth),
            new Vector2(48 - StripeWidth, startY + StripeWidth),
            new Vector2(48 - StripeWidth, startY)
        };

        return new Polygon2D { Polygon = points, Color = isYellow ? Yellow : Black };
    }

    private Polygon2D CreateWestStripe(float edgeX, int index, bool isYellow)
    {
        float startY = index * StripeWidth;
        
        var points = new Vector2[]
        {
            new Vector2(0, startY),
            new Vector2(StripeWidth, startY + StripeWidth),
            new Vector2(StripeWidth, startY),
            new Vector2(0, startY)
        };

        return new Polygon2D { Polygon = points, Color = isYellow ? Yellow : Black };
    }

    private void HideAll()
    {
        foreach (var stripes in _stripesByDirection.Values)
        {
            foreach (var stripe in stripes)
                stripe.Hide();
        }
    }

    public void RenderBlockages(IReadOnlyList<int> blockedDirections)
    {
        HideAll();
        
        foreach (var dir in blockedDirections)
        {
            if (_stripesByDirection.TryGetValue(dir, out var stripes))
            {
                foreach (var stripe in stripes)
                    stripe.Show();
            }
        }
    }

    public void Clear()
    {
        HideAll();
    }
}
namespace AiTest.Godot.Components;

using System.Collections.Generic;
using Godot;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

public partial class BarricadeRenderer : Node2D
{
    private const float StripeWidth = 6f;
    private const int StripeCount = 8;
    
    private readonly Dictionary<Direction, List<Polygon2D>> _stripesByDirection = new();

    private static readonly Color Yellow = new(0.9f, 0.7f, 0, 1);
    private static readonly Color Black = new(0.05f, 0.05f, 0.05f, 1);

    public override void _Ready()
    {
        CreateDiagonalStripes(Direction.North, new Vector2(24, 2), false);
        CreateDiagonalStripes(Direction.South, new Vector2(24, 46), false);
        CreateDiagonalStripes(Direction.East, new Vector2(46, 24), true);
        CreateDiagonalStripes(Direction.West, new Vector2(2, 24), true);
        
        HideAll();
    }

    private void CreateDiagonalStripes(Direction direction, Vector2 center, bool vertical)
    {
        var stripes = new List<Polygon2D>();
        float totalWidth = StripeCount * StripeWidth;
        
        for (int i = 0; i < StripeCount; i++)
        {
            bool isYellow = i % 2 == 0;
            
            Polygon2D stripe;
            if (vertical)
            {
                stripe = CreateVerticalDiagonalStripe(center, totalWidth, i, isYellow);
            }
            else
            {
                stripe = CreateHorizontalDiagonalStripe(center, totalWidth, i, isYellow);
            }
            
            AddChild(stripe);
            stripes.Add(stripe);
        }
        
        _stripesByDirection[direction] = stripes;
    }

    private Polygon2D CreateVerticalDiagonalStripe(Vector2 center, float totalWidth, int index, bool isYellow)
    {
        float startOffset = index * StripeWidth - totalWidth / 2;
        
        var points = new Vector2[]
        {
            new Vector2(startOffset, -8),
            new Vector2(startOffset + StripeWidth, -8 + StripeWidth),
            new Vector2(startOffset + StripeWidth, -8 + StripeWidth + 48),
            new Vector2(startOffset, -8 + 48)
        };

        return new Polygon2D
        {
            Polygon = points,
            Color = isYellow ? Yellow : Black
        };
    }

    private Polygon2D CreateHorizontalDiagonalStripe(Vector2 center, float totalWidth, int index, bool isYellow)
    {
        float startOffset = index * StripeWidth - totalWidth / 2;
        
        var points = new Vector2[]
        {
            new Vector2(-8, startOffset),
            new Vector2(-8 + StripeWidth, startOffset + StripeWidth),
            new Vector2(-8 + 48, startOffset + StripeWidth),
            new Vector2(-8 + 48, startOffset)
        };

        return new Polygon2D
        {
            Polygon = points,
            Color = isYellow ? Yellow : Black
        };
    }

    private void HideAll()
    {
        foreach (var stripes in _stripesByDirection.Values)
        {
            foreach (var stripe in stripes)
                stripe.Hide();
        }
    }

    public void RenderBlockages(DirectionSet blockedDirections)
    {
        HideAll();
        
        foreach (var direction in blockedDirections.AllDirections())
        {
            if (_stripesByDirection.TryGetValue(direction, out var stripes))
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

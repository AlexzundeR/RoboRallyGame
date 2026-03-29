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
        CreateCrossStripes(Direction.North, new Vector2(24, 0), true);
        CreateCrossStripes(Direction.South, new Vector2(24, 48), true);
        CreateCrossStripes(Direction.East, new Vector2(48, 24), false);
        CreateCrossStripes(Direction.West, new Vector2(0, 24), false);
        
        HideAll();
    }

    private void CreateCrossStripes(Direction direction, Vector2 edgePosition, bool horizontal)
    {
        var stripes = new List<Polygon2D>();
        float totalWidth = StripeCount * StripeWidth;
        
        for (int i = 0; i < StripeCount; i++)
        {
            bool isYellow = i % 2 == 0;
            Polygon2D stripe;
            
            if (horizontal)
            {
                stripe = CreateHorizontalEdgeStripe(edgePosition, totalWidth, i, isYellow);
            }
            else
            {
                stripe = CreateVerticalEdgeStripe(edgePosition, totalWidth, i, isYellow);
            }
            
            AddChild(stripe);
            stripes.Add(stripe);
        }
        
        _stripesByDirection[direction] = stripes;
    }

    private Polygon2D CreateHorizontalEdgeStripe(Vector2 edgePos, float totalWidth, int index, bool isYellow)
    {
        float startX = index * StripeWidth - totalWidth / 2;
        
        var points = new Vector2[]
        {
            new Vector2(startX, edgePos.Y - 4),
            new Vector2(startX + StripeWidth, edgePos.Y - 4 + StripeWidth),
            new Vector2(startX + StripeWidth, edgePos.Y + StripeWidth),
            new Vector2(startX, edgePos.Y)
        };

        return new Polygon2D
        {
            Polygon = points,
            Color = isYellow ? Yellow : Black
        };
    }

    private Polygon2D CreateVerticalEdgeStripe(Vector2 edgePos, float totalWidth, int index, bool isYellow)
    {
        float startY = index * StripeWidth - totalWidth / 2;
        
        var points = new Vector2[]
        {
            new Vector2(edgePos.X - 4, startY),
            new Vector2(edgePos.X - 4 + StripeWidth, startY + StripeWidth),
            new Vector2(edgePos.X + StripeWidth, startY + StripeWidth),
            new Vector2(edgePos.X, startY)
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

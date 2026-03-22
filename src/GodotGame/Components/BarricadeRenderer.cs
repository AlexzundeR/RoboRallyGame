namespace AiTest.Godot.Components;

using System.Collections.Generic;
using Godot;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

public partial class BarricadeRenderer : Node2D
{
    private const float StripeWidth = 6f;
    private const float StripeCount = 3f;
    
    private readonly Dictionary<Direction, Polygon2D> _stripes = new();

    public override void _Ready()
    {
        CreateStripe(Direction.North, new Vector2(24, 0), 48f, false);
        CreateStripe(Direction.South, new Vector2(24, 48), 48f, false);
        CreateStripe(Direction.East, new Vector2(48, 24), 48f, true);
        CreateStripe(Direction.West, new Vector2(0, 24), 48f, true);
        
        foreach (var stripe in _stripes.Values)
            stripe.Hide();
    }

    private void CreateStripe(Direction direction, Vector2 position, float length, bool vertical)
    {
        Vector2[] points;
        if (vertical)
        {
            points = new Vector2[]
            {
                new Vector2(0, -StripeWidth * StripeCount / 2),
                new Vector2(StripeWidth, -StripeWidth * StripeCount / 2),
                new Vector2(StripeWidth, StripeWidth * StripeCount / 2),
                new Vector2(0, StripeWidth * StripeCount / 2)
            };
        }
        else
        {
            points = new Vector2[]
            {
                new Vector2(-StripeWidth * StripeCount / 2, 0),
                new Vector2(StripeWidth * StripeCount / 2, 0),
                new Vector2(StripeWidth * StripeCount / 2, StripeWidth),
                new Vector2(-StripeWidth * StripeCount / 2, StripeWidth)
            };
        }

        var stripe = new Polygon2D
        {
            Position = position,
            Polygon = points,
            Color = new Color(0.9f, 0.7f, 0, 1)
        };

        AddChild(stripe);
        _stripes[direction] = stripe;
    }

    public void RenderBlockages(DirectionSet blockedDirections)
    {
        foreach (var direction in blockedDirections.AllDirections())
        {
            if (_stripes.TryGetValue(direction, out var stripe))
            {
                stripe.Show();
            }
        }
    }

    public void Clear()
    {
        foreach (var stripe in _stripes.Values)
            stripe.Hide();
    }
}

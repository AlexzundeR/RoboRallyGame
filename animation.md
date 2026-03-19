# Animation System in Godot

## Key Factors for Creating Animations

### 1. Coordinate System - World vs Screen Coordinates

**CRITICAL**: Godot has different coordinate systems:
- **Node2D** uses **world coordinates** (positions in the game world)
- **CanvasLayer** uses **screen coordinates** (positions relative to screen)

If you add children to a `Node2D`, use **world coordinates** for positions.
If you add children to a `CanvasLayer`, use **screen coordinates**.

```csharp
// WRONG - using world coordinates with CanvasLayer
var canvasLayer = new CanvasLayer();
var sprite = new Sprite2D { Position = new Vector2(100, 100) }; // This is screen position!

// CORRECT - add effects to Node2D (world coordinates)
var worldNode = _robotLayer; // Node2D
var sprite = new Sprite2D { Position = new Vector2(100, 100) }; // This is world position!
worldNode.AddChild(sprite);
```

### 2. Z-Index for Layering

To ensure animations appear on top of other elements:
```csharp
laserBeam.ZIndex = 1000;
laserLine.ZIndex = 1001;
hitMarker.ZIndex = 1002;
```

Higher ZIndex values render on top of lower values.

### 3. Animation Queue System

To ensure animations play in the correct order (e.g., movement before laser fire), use a queue-based system:

```csharp
// 1. Events fire from game engine (OnMove, OnRotate, OnLaserFire, etc.)
// 2. Events are filtered by current phase (1=Movement, 2=Conveyor, 3=Laser)
// 3. Animations are QUEUED instead of executing immediately
QueueAnimation(new QueuedMove { PlayerId = playerId, From = from, To = to });

// 4. ExecuteNextAnimation() runs animations one by one
// 5. Each animation calls ScheduleNext() when done, which triggers the next
```

### 4. Phase-Based Animation Control

The game has three phases per register execution:
1. **Movement Phase** - robots execute their cards (move, rotate, backup)
2. **Conveyor Phase** - board conveyors push robots
3. **Laser Phase** - robots fire lasers, damage is applied

Events:
- `MovementPhaseStartedEvent` - starts movement animations
- `ConveyorPhaseStartedEvent` - starts conveyor animations
- `LaserPhaseStartedEvent` - starts laser animations
- `PhaseExecutionCompleteEvent` - signals end of all phases

### 5. Tween Timing

Animations happen concurrently unless you add delays:
```csharp
var tween = CreateTween();

// Add delay before animation starts
tween.TweenInterval(delayInSeconds);

// Then animate
tween.TweenProperty(target, "position", endPos, duration);
tween.Play();
```

### 6. Sequential Animations with Callbacks

To chain animations (one after another), use callbacks:
```csharp
var tween = CreateTween();
tween.TweenProperty(robot, "position", endPos, 0.6f);
tween.TweenCallback(Callable.From(ScheduleNext)); // ScheduleNext calls ExecuteNextAnimation
tween.Play();
```

### 7. Parent Node for Effect Groups

When creating complex effects with multiple nodes, parent them to a single Node2D:
```csharp
var laserRoot = new Node2D();
parent.AddChild(laserRoot);

var laserLine = new Line2D();
laserRoot.AddChild(laserLine);

var hitMarker = new ColorRect();
laserRoot.AddChild(hitMarker);

// Animate the whole group
tween.TweenProperty(laserRoot, "modulate:a", 0f, 0.5f);
```

## Common Mistakes

1. **Using CanvasLayer with world positions** - creates effects in wrong location
2. **Forgetting ZIndex** - effects appear behind other elements
3. **Immediate execution** - executing animations synchronously instead of queuing them
4. **Not calling Play()** - animation never starts
5. **Orphaned nodes** - forgetting to QueueFree() temporary effect nodes

## Animation Durations

Recommended durations for game actions:
- Move: 0.6s
- Rotate: 0.4s
- Conveyor: 0.5s
- Laser: 2.0-2.5s (needs to be visible!)
- Damage flash: 0.5s
- Checkpoint: 1.2s
- Destroy: 1.0s

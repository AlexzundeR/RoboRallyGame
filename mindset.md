# World Action Rendering - Mindset

## Context

We need to visualize game actions:
- Robot movements on the map
- Laser shots
- Conveyor belt movements
- Push-back effects
- Damage effects
- Other board element interactions

## Current State

Currently, actions execute instantly in the game engine. The UI only shows the final state.

## Key Questions

1. **Where should animation logic live?**
   - Core engine (pure C#)
   - Godot-specific (C# with Godot APIs)
   - Separate animation system?

2. **How to describe animations?**
   - Command pattern?
   - Event-driven?
   - Timeline/tween system?

3. **How to coordinate with game logic?**
   - Animations must not block logic
   - But we need to wait for animations before next action
   - How to handle animation cancellation/interrupts?

## Ideas

### Idea 1: Animation Queue with Commands

```
Action → AnimationCommand → ExecuteAnimation → NotifyComplete → NextAction
```

Pros: Clear separation, easy to debug
Cons: Might be verbose

### Idea 2: Event-Based with Visualizer

```
Core fires Event → Visualizer subscribes → Visualizer plays animation → Visualizer fires completion
```

Pros: Core doesn't need to know about visuals, core can continue
Cons: Need to sync state, race conditions

### Idea 3: Timeline/Tween Approach

```
Build timeline of all actions → Play timeline → Done
```

Pros: Can optimize/order animations, easy to see full picture
Cons: Need to collect all intents first

## Considerations

1. **Not all actions need animations** - in tests/simulations, we skip visuals
2. **Animations should be skippable** - for testing or impatient players
3. **Different speeds** - might want slow-mo for debugging, fast for impatient players
4. **Multiple simultaneous animations** - robots can move at the same time

## Current Thinking

I lean towards a **hybrid approach**:
- Core engine produces "action intents" (pure data, no visuals)
- Godot-side visualizer consumes intents and plays animations
- Visualizer notifies completion back to core when needed (for sequencing)
- Tests use a "null visualizer" that just passes through

### Action Intent Structure

```csharp
interface IActionIntent {
    ActionType Type { get; }
    int SourceId { get; }
    Position Start { get; }
    Position End { get; }
    double Duration { get; }
}
```

### Types of Intents
- MoveIntent - robot movement
- RotateIntent - robot rotation
- LaserIntent - laser shot (with path)
- PushIntent - push-back effect
- DamageIntent - damage effect
- ConveyorIntent - conveyor movement
- FlagCaptureIntent - checkpoint reached

## Next Steps

1. Define IActionIntent interface
2. Create concrete intent types
3. Modify GameEngine to produce intents
4. Create IVisualizer interface
5. Create GodotVisualizer that implements IVisualizer
6. Hook into ExecutionPhase

## Open Questions

- Do we need to animate board elements (conveyors, flags) separately?
- How to handle failed movements (blocked by wall)?
- Should animations be interruptible?
- Do we need prediction/reconciliation for network play?

## Revised Thinking

After deeper analysis, I think the cleanest approach is **event-driven with visual intents**:

### Architecture

```
GameEngine (Core)                Godot (Visual)
      │                                │
      │  ExecuteRegister()             │
      │───────────────────────────────>
      │                                │
      │  [Game state updated]          │  [Observe state diff]
      │                                │
      │  Fire VisualEvent              │
      │<───────────────────────────────│
      │                                │
      │                    [Play animation]
      │                                │
      │                    [NotifyComplete]
      │<───────────────────────────────│
      │                                │
      │  [Ready for next register]     │
```

### Why this approach?

1. **Core stays pure** - No animation knowledge in Core
2. **Godot controls timing** - Can speed up, slow down, skip
3. **Testable** - Core just fires events, tests ignore visuals
4. **Flexible** - Godot can add particle effects, sounds, etc.

### State Diff Approach

Instead of explicit intents, Godot observes:
- Before ExecuteRegister: snapshot robot positions/directions
- After ExecuteRegister: compare, animate differences

```csharp
// In Godot
void OnRegisterExecuted(RegisterExecutedEvent e) {
    foreach (var player in state.Players) {
        if (player.Robot.Position != _oldPositions[player.Id]) {
            AnimateMove(player, _oldPositions[player.Id], player.Robot.Position);
        }
        if (player.Robot.Direction != _oldDirections[player.Id]) {
            AnimateRotate(player, _oldDirections[player.Id], player.Robot.Direction);
        }
    }
    _oldPositions = snapshot();
    _oldDirections = snapshot();
}
```

### Laser Animation Challenge

Lasers are trickier because:
- They're not part of Robot state
- We need to know the laser path
- Multiple robots can shoot simultaneously

Solution: Add LaserFired event with path info

```csharp
public record LaserFiredEvent(int SourceRobotId, IReadOnlyList<Position> Path, bool HitRobot);
```

### Animation Queue

Need to handle:
1. Sequential animations (one after another)
2. Parallel animations (robots moving at same time)
3. Effects during animations (laser beam visible while moving)

Godot can use Tween or AnimationPlayer for this.

## Implementation Status

### Core (Done)
- Added visual events: MoveEvent, RotateEvent, LaserFireEvent, DamageEvent, ConveyorMoveEvent, CheckpointEvent, RobotDestroyedEvent, RobotRebootEvent
- GameEngine fires events via: OnMove, OnRotate, OnLaserFire, OnDamage, OnConveyorMove, OnCheckpoint, OnRobotDestroyed, OnRobotReboot
- Events are optional - no handler means no-op
- All 162 core tests pass
- Added ExecutionStartedEvent, RegistersClearedEvent

### Godot AnimationController (Done)
- Created AnimationController.cs in Godot project
- Fixed Control -> ColorRect for color operations
- Added AnimateRotate with direction-based rotation and PivotOffset
- Added ActionLog with RichTextLabel - shows game actions in real-time
- Fixed laser beam to include source position and direction
- All animation durations increased for better visibility:
  - Move: 0.5s, Rotate: 0.35s, Conveyor: 0.4s
  - Laser: 1.0s (width 8), Damage flash: 0.4s
  - Checkpoint: 1.0s, Destroy: 0.8s

### ExecutionPhase Fix
- Fixed simultaneous execution - all robots execute cards at the same time
- Removed player loop that was causing double execution
- Added RegistersClearedEvent to trigger UI update

### UI Updates
- Added ActionLog panel on the right side of the screen
- Fixed game window layout to fit player panels
- Registers now properly clear visually after execution phase

### Next Steps
1. Test in Godot - verify laser beams appear
2. Verify register cards clear after execution
3. Test simultaneous robot movement animation
4. Consider adding delays between execution phases

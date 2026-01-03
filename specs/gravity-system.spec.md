# Gravity System Specification

## Purpose

Manage gravitational attraction toward celestial bodies. Entities affected by gravity experience a force pulling them toward the nearest/dominant gravity source.

## Design Goals

- **Predictable gameplay** – Players should intuit gravity boundaries
- **Tunable** – Designers can adjust feel without code changes
- **Performant** – O(n) per affected entity, where n = active gravity sources

## Interfaces

### IGravitySource

Implemented by objects that generate gravity fields.

```csharp
public interface IGravitySource
{
    /// <summary>Center point of the gravity field (world space).</summary>
    Vector3 GravityCenter { get; }
    
    /// <summary>Gravity strength at the surface (m/s²).</summary>
    float BaseStrength { get; }
    
    /// <summary>Maximum range of the gravity field (meters).</summary>
    float MaxRange { get; }
    
    /// <summary>Priority for tie-breaking overlapping fields.</summary>
    int Priority { get; }
    
    /// <summary>Calculate gravity vector for a given world position.</summary>
    Vector3 CalculateGravity(Vector3 worldPosition);
}
```

### IGravityAffected

Implemented by entities that respond to gravity.

```csharp
public interface IGravityAffected
{
    /// <summary>Current gravity vector being applied.</summary>
    Vector3 CurrentGravity { get; }
    
    /// <summary>The dominant gravity source affecting this entity.</summary>
    IGravitySource DominantSource { get; }
    
    /// <summary>Whether gravity is currently enabled for this entity.</summary>
    bool GravityEnabled { get; set; }
}
```

## Components

### GravityBody : MonoBehaviour, IGravitySource

Attached to planets, moons, asteroids – any body with gravity.

**Inspector Fields:**
- `baseStrength` (float, default 9.8) – Surface gravity in m/s²
- `maxRange` (float) – Outer boundary of gravity influence
- `priority` (int, default 0) – Higher = wins ties
- `useColliderRadius` (bool) – Auto-calculate surface from collider

**Runtime:**
- Registers with `GravityManager` on `OnEnable`
- Unregisters on `OnDisable`
- Draws gizmo showing gravity range in editor

### GravitySolver : MonoBehaviour, IGravityAffected

Attached to player, ship, physics objects that need gravity.

**Inspector Fields:**
- `gravityEnabled` (bool, default true)
- `gravityScale` (float, default 1.0) – Multiplier for gameplay tuning

**Runtime:**
- Queries `GravityManager` for dominant source each `FixedUpdate`
- Exposes `CurrentGravity` for motor/rigidbody to consume
- Does NOT apply forces directly (consumer's responsibility)

### GravityManager : Singleton

Central registry of all active gravity sources.

**Responsibilities:**
- Maintain list of active `IGravitySource` instances
- Provide `GetDominantSource(Vector3 position)` query
- Provide `GetGravityAt(Vector3 position)` for quick lookups

## Gravity Formula

**Linear falloff with hard cutoff** (recommended):

```
distance = Vector3.Distance(entityPosition, gravityCenter)
if (distance > maxRange) return Vector3.zero

normalizedDistance = distance / maxRange
strength = baseStrength * (1 - normalizedDistance)
direction = (gravityCenter - entityPosition).normalized
gravity = direction * strength
```

**Why linear:**
- Predictable feel for players
- No division-by-zero edge cases
- Easy to visualize and tune

## Behaviors

| Scenario | Behavior |
|----------|----------|
| Entity enters gravity field | GravitySolver recalculates dominant source |
| Multiple overlapping fields | Highest priority wins; if tied, closest wins |
| Entity exactly equidistant | Use priority; if still tied, use instance ID (deterministic) |
| Ship toggles gravity | Set `GravityEnabled = false`, ship ignores all fields |
| Zero-g zone needed | Create explicit zone trigger, not emergent from field gaps |

## Edge Cases

1. **No gravity sources** – Return `Vector3.zero`, entity floats
2. **Source destroyed while dominant** – Solver recalculates next frame
3. **Very fast entity crosses fields** – Recalc happens in `FixedUpdate`, may feel abrupt (acceptable for MVP)

## Performance Notes

- GravityManager uses spatial partitioning if sources > 10 (future optimization)
- GravitySolver caches dominant source, only recalculates when position changes significantly
- Gizmos disabled in builds

## Testing Checklist

- [ ] Entity on planet surface experiences correct down direction
- [ ] Entity between two planets goes to higher-priority one
- [ ] Entity outside all fields experiences zero gravity
- [ ] Disabling gravity on entity makes it float
- [ ] Adding/removing GravityBody at runtime works correctly

## Dependencies

- `Game.Core` (base classes, interfaces)

## Files

```
Scripts/Gravity/
├── IGravitySource.cs
├── IGravityAffected.cs
├── GravityBody.cs
├── GravitySolver.cs
└── GravityManager.cs
```

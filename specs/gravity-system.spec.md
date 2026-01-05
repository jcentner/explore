# Gravity System Specification

**Status: ✅ IMPLEMENTED (Milestone 1 + 3)**

## Implementation Notes

| Component | File | Status |
|-----------|------|--------|
| `IGravitySource` | `Scripts/Core/IGravitySource.cs` | ✅ Complete |
| `IGravityAffected` | `Scripts/Core/IGravityAffected.cs` | ✅ Complete |
| `GravityManager` | `Scripts/Gravity/GravityManager.cs` | ✅ Complete |
| `GravityBody` | `Scripts/Gravity/GravityBody.cs` | ✅ Complete |
| `GravitySolver` | `Scripts/Gravity/GravitySolver.cs` | ✅ Complete |
| `GravityPreset` | `Scripts/Gravity/GravityPreset.cs` | ✅ Complete |
| `GravityDebugPanel` | `Scripts/Gravity/GravityDebugPanel.cs` | ✅ Complete |
| `GravityIndicatorPanel` | `Scripts/UI/Panels/GravityIndicatorPanel.cs` | ✅ Complete |

### Milestone 3 Enhancements
- **Multi-body accumulation**: All sources contribute simultaneously (inverse-square falloff)
- **Smooth orientation blending**: No jarring snaps when crossing boundaries (90°/s default)
- **Emergent Lagrange points**: Gravity < 0.25 m/s² clamped to zero automatically
- **Zero-G behavior**: Thrust-based movement when floating
- **UI indicator**: Arrow shows gravity direction, "ZERO-G" text when floating
- **Debug panel (F3)**: Position, velocity, contributors, solver settings

### Known Limitations
- No spatial partitioning yet (fine for < 10 sources)
- Orientation uses dominant source only (strongest contributor)

---

## Purpose

Manage gravitational attraction toward celestial bodies. Entities affected by gravity experience a force pulling them toward the nearest/dominant gravity source.

## Design Goals

- **Predictable gameplay** – Players should intuit gravity boundaries
- **Tunable** – Designers can adjust feel without code changes
- **Performant** – O(n) per affected entity, where n = active gravity sources

## Enums

### GravityMode

Determines how a gravity source participates in multi-body gravity calculations.

```csharp
public enum GravityMode
{
    /// <summary>Only considered for dominant source selection (orientation).</summary>
    Dominant,
    /// <summary>Only contributes to accumulated gravity sum (force).</summary>
    Accumulate,
    /// <summary>Both contributes to sum AND can be selected as dominant.</summary>
    Both
}
```

**Use cases:**
- `Both` (default): Standard planets/moons that affect both movement and orientation
- `Accumulate`: Subtle gravity wells that pull but don't reorient the player
- `Dominant`: Artificial gravity zones that only affect orientation, not physics

## Interfaces

### IGravitySource

Implemented by objects that generate gravity fields.

```csharp
public interface IGravitySource
{
    Vector3 GravityCenter { get; }      // Center point (world space)
    float BaseStrength { get; }          // Gravity at surface (m/s²)
    float MaxRange { get; }              // Outer boundary (performance cutoff)
    float SurfaceRadius { get; }         // Body radius (for inverse-square)
    float Mass { get; }                  // Derived: BaseStrength × SurfaceRadius²
    int Priority { get; }                // Tie-breaker for dominant selection
    GravityMode Mode { get; }            // How source participates in multi-body
    
    Vector3 CalculateGravity(Vector3 worldPosition);
}
```

### IGravityAffected

Implemented by entities that respond to gravity.

```csharp
public interface IGravityAffected
{
    Vector3 CurrentGravity { get; }      // Accumulated gravity being applied
    IGravitySource DominantSource { get; } // Source used for orientation
    bool GravityEnabled { get; set; }    // Toggle gravity response
    bool IsInZeroG { get; }              // True when gravity < threshold
}
```

## Components

### GravityBody : MonoBehaviour, IGravitySource

Attached to planets, moons, asteroids – any body with gravity.

**Inspector Fields:**
- `baseStrength` (float, default 9.8) – Surface gravity in m/s²
- `maxRange` (float) – Outer boundary of gravity influence (hard cutoff)
- `surfaceRadius` (float) – Body radius, auto-detected from SphereCollider if `useColliderRadius` is true
- `priority` (int, default 0) – Tie-breaker for dominant selection (higher wins)
- `mode` (GravityMode, default Both) – How source participates in multi-body
- `useColliderRadius` (bool, default true) – Auto-calculate surface from SphereCollider

**Derived Properties:**
- `Mass` = `baseStrength × surfaceRadius²` – Used in inverse-square formula

**Runtime:**
- Registers with `GravityManager` on `OnEnable`
- Unregisters on `OnDisable`
- Draws gizmo showing gravity range and inverse-square contours (50%, 25%, 10%)

### GravitySolver : MonoBehaviour, IGravityAffected

Attached to player, ship, physics objects that need gravity.

**Inspector Fields:**
- `gravityEnabled` (bool, default true) – Toggle gravity response
- `gravityScale` (float, default 1.0) – Multiplier for gameplay tuning
- `minGravityThreshold` (float, default 0.25) – Below this, gravity clamped to zero (triggers zero-g)
- `orientationSpeed` (float, default 90) – Degrees/second for up-alignment to dominant source

**Runtime:**
- Queries `GravityManager.GetAccumulatedGravity()` each `FixedUpdate`
- Queries `GravityManager.GetDominantSource()` for orientation target
- Clamps gravity below threshold to zero (enables emergent Lagrange points)
- Smoothly blends `LocalUp` toward dominant source direction
- Exposes `CurrentGravity` and `IsInZeroG` for consumers

### GravityManager : Singleton

Central registry of all active gravity sources.

**Key Methods:**
- `Register(IGravitySource)` / `Unregister(IGravitySource)` – Called by GravityBody
- `GetAccumulatedGravity(Vector3)` – Sum of all sources (respects `GravityMode`)
- `GetDominantSource(Vector3)` – Strongest source for orientation (respects `GravityMode`)
- `GetAllContributors(Vector3)` – Debug: all sources with magnitude/percentage
- `GetGravityAt(Vector3)` – Legacy: gravity from dominant source only

## Gravity Formula

**Inverse-square falloff with hard cutoff:**

```
distance = Vector3.Distance(entityPosition, gravityCenter)
if (distance > maxRange) return Vector3.zero
if (distance < surfaceRadius) distance = surfaceRadius  // Clamp at surface

// Inverse-square: g = g₀ × (r₀² / r²)
magnitude = baseStrength * (surfaceRadius² / distance²)
direction = (gravityCenter - entityPosition).normalized
gravity = direction * magnitude
```

**Why inverse-square:**
- Physically accurate – feels natural to players
- Multi-body interactions work correctly – gravity sources combine realistically
- Emergent Lagrange points – gravity cancels where sources balance
- Surface clamping prevents infinite gravity at center

## Behaviors

| Scenario | Behavior |
|----------|----------|
| Entity enters gravity field | GravitySolver queries accumulated gravity from all sources |
| Multiple overlapping fields | All sources contribute to accumulated gravity (inverse-square sum) |
| Dominant source selection | Strongest magnitude wins; priority is tie-breaker only |
| Gravity below threshold | Clamped to zero → `IsInZeroG = true` |
| Emergent Lagrange points | Where sources cancel out, gravity < threshold → zero-g zone |
| Ship toggles gravity | Set `GravityEnabled = false`, ship ignores all fields |

## Zero-G Detection

When accumulated gravity magnitude falls below `minGravityThreshold` (default 0.25 m/s²):
- Gravity is clamped to `Vector3.zero`
- `IsInZeroG` property returns `true`
- Player enters thrust-based movement mode
- Creates emergent Lagrange-like points where gravity sources balance

## Edge Cases

1. **No gravity sources** – Return `Vector3.zero`, entity floats (`IsInZeroG = true`)
2. **Source destroyed while dominant** – Solver recalculates next frame
3. **Very fast entity crosses fields** – Recalc happens in `FixedUpdate`, may feel abrupt (acceptable for MVP)
4. **Inside gravity body** – Distance clamped to surface radius (prevents infinite gravity)
5. **GravityMode.Accumulate source** – Contributes to force but never selected as dominant

## Performance Notes

- GravityManager uses spatial partitioning if sources > 10 (future optimization)
- GravitySolver caches dominant source, only recalculates when position changes significantly
- Gizmos disabled in builds

## Testing Checklist

- [x] Entity on planet surface experiences correct down direction
- [x] Entity between two planets goes to higher-priority one (or closer if same priority)
- [x] Entity outside all fields experiences zero gravity
- [ ] Disabling gravity on entity makes it float
- [x] Adding/removing GravityBody at runtime works correctly

## Dependencies

- `Game.Core` (base classes, interfaces)

## Files

```
Scripts/Core/
├── IGravitySource.cs      # Interface + GravityMode enum
├── IGravityAffected.cs    # Interface

Scripts/Gravity/
├── GravityBody.cs         # Gravity source component
├── GravitySolver.cs       # Gravity consumer component
├── GravityManager.cs      # Singleton registry
├── GravityPreset.cs       # ScriptableObject for preset configurations
├── GravityDebugPanel.cs   # F3 debug overlay
└── GravityStableZone.cs   # Optional visual marker for stable points

Scripts/UI/Panels/
└── GravityIndicatorPanel.cs  # HUD gravity direction indicator
```

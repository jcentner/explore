# Milestone 3: Advanced Gravity System

**Status:** ✅ Complete  
**Goal:** Evolve from single-dominant-source gravity to multi-body accumulation with emergent Lagrange-like behavior, while maintaining the "learnable physics" pillar.

## 1. Context & Problem Statement

### Current State (Milestone 1-2)
- Single dominant gravity source (priority-based, then distance)
- Hard switch between sources (no blending)
- Player/ship instantly snaps to new "down" when crossing boundaries
- Works well but feels artificial at boundaries

### Desired State
- Multiple gravity sources contribute simultaneously
- Smooth transitions as gravity balance shifts
- Emergent stable points (like Lagrange points) between bodies
- Visual indicator showing combined gravity direction
- Maintains "learnable physics" - players can intuit behavior

### Design Pillar Alignment
> "Consistent, learnable physics (not realistic physics)"

This milestone adds realism *where it improves gameplay feel*, not for simulation accuracy. We want:
- Smooth transitions (feels natural)
- Emergent stable points (rewards exploration)
- Visual feedback (gravity is learnable)

We explicitly avoid:
- True N-body simulation complexity
- Orbital mechanics requirements
- Physics that feel random or unpredictable

---

## 2. Architecture Changes

### Current Flow
```
GravitySolver.FixedUpdate()
    → GravityManager.GetDominantSource(position)
    → Returns single IGravitySource
    → GravitySolver.CurrentGravity = source.CalculateGravity(position)
```

### New Flow
```
GravitySolver.FixedUpdate()
    → GravityManager.GetAccumulatedGravity(position)
    → Iterates all sources, sums weighted contributions
    → GravitySolver.CurrentGravity = accumulated vector
    → GravitySolver.DominantSource = strongest contributor (for orientation)
```

### Key Insight
We still need a "dominant" source concept for:
- Player orientation ("which way is down" for walking)
- Camera alignment
- Landing detection

The accumulated gravity affects physics; dominant source affects orientation.

---

## 3. Gravity Formula Options

### Option A: Inverse-Square (Realistic)
```csharp
float strength = (G * mass) / (distance * distance);
```
**Pros:** Real Lagrange points emerge naturally  
**Cons:** Requires careful mass tuning, can feel unintuitive

### Option B: Inverse-Linear with Soft Blending (Recommended)
```csharp
// Each source contributes based on proximity and strength
float influence = baseStrength * (1 - distance / maxRange);
influence = Mathf.Max(0, influence); // Clamp at range boundary

// Accumulate all contributions
Vector3 totalGravity = Vector3.zero;
foreach (var source in sources)
{
    Vector3 direction = (source.Center - position).normalized;
    float contribution = CalculateInfluence(source, position);
    totalGravity += direction * contribution;
}
```
**Pros:** Tunable per-body, predictable falloff, designer-friendly  
**Cons:** Lagrange points need explicit tuning or won't emerge

### Option C: Hybrid (Best of Both)
```csharp
// Use inverse-square for physics accumulation
// Use linear falloff for "sphere of influence" boundaries
// Blend between them based on distance from boundary
```

### Recommendation: Option B with Tunable Curves

Use **AnimationCurve** for falloff, allowing designers to:
- Create sharp boundaries (current behavior)
- Create smooth blends (new behavior)  
- Tune per-body for gameplay needs

---

## 4. Implementation Tasks

### Phase 1: Core Gravity Accumulation

#### Task 1.1: Extend IGravitySource
- [x] Add `float Mass { get; }` property (for realistic calculations)
- [x] Add `float SurfaceRadius { get; }` for inverse-square calculations
- [x] Add `GravityMode Mode { get; }` enum (Dominant, Accumulate, Both)

```csharp
public enum GravityMode
{
    Dominant,    // Old behavior - this source wins if closest/priority
    Accumulate,  // New behavior - contributes to sum
    Both         // Can be dominant AND contribute to accumulation
}
```

#### Task 1.2: Update GravityBody
- [x] Add `Mass` property auto-derived from `BaseStrength × SurfaceRadius²`
- [x] Add `[SerializeField] GravityMode _mode = GravityMode.Both`
- [x] Implement inverse-square `CalculateGravity()`: `g = g₀ × (r₀² / r²)`
- [x] Clamp gravity at surface to prevent infinite values

#### Task 1.3: Update GravityManager
- [x] Add `GetAccumulatedGravity(Vector3 position)` method
- [x] Refactor `GetDominantSource(Vector3 position)` to use magnitude (not distance)
- [x] Add `GetAllContributors(Vector3 position)` returning `List<GravityContribution>`
- [x] Add `GravityContribution` struct for source/direction/magnitude/influence data

#### Task 1.4: Update GravitySolver
- [x] Change from single-source to accumulated gravity via `GetAccumulatedGravity()`
- [x] Keep `DominantSource` property for orientation consumers
- [x] Add `GravityContributions` list for UI/debugging
- [x] Add `IsInZeroG` property with configurable `_zeroGThreshold`

#### Task 1.5: Create GravityPreset ScriptableObject
- [x] Create `GravityPreset.cs` with tunable parameters
- [x] Include presets: Earth-like, Moon-like, Asteroid
- [x] Add `OrientationBlendSpeed` for Phase 2 use

#### Task 1.6: Update Gizmo Visualizations
- [x] `GravityBody`: Draw inverse-square falloff contours (50%, 25%, 10%)
- [x] `GravitySolver`: Draw individual contributions when multiple sources

**Files Modified:**
- `Assets/_Project/Scripts/Core/IGravitySource.cs` ✅
- `Assets/_Project/Scripts/Gravity/GravityBody.cs` ✅
- `Assets/_Project/Scripts/Gravity/GravityManager.cs` ✅
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs` ✅

**Files Created:**
- `Assets/_Project/Scripts/Gravity/GravityPreset.cs` ✅

---

### Phase 2: Smooth Transitions

#### Task 2.1: Orientation Blending
- [x] Extend GravitySolver with smoothed orientation
- [x] Add `_smoothedUp` and `_targetUp` private fields
- [x] Add `_orientationBlendSpeed` (degrees/second) with default 90°/s
- [x] Add `_maxRotationSpeed` limit to prevent nausea (default 180°/s)
- [x] Implement `UpdateSmoothedOrientation()` with proper 180° flip handling
- [x] Add `RawLocalUp` property for unsmoothed direction
- [x] `LocalUp` now returns smoothed direction

```csharp
// Smooth blend with max rotation rate limiting:
float maxAngleThisFrame = _orientationBlendSpeed * Time.fixedDeltaTime;
if (_maxRotationSpeed > 0f)
    maxAngleThisFrame = Mathf.Min(maxAngleThisFrame, _maxRotationSpeed * Time.fixedDeltaTime);
float blendT = Mathf.Clamp01(maxAngleThisFrame / angleDiff);
_smoothedUp = Vector3.Slerp(_smoothedUp, _targetUp, blendT).normalized;
```

#### Task 2.2: Update CharacterMotorSpherical
- [x] Consume smoothed orientation from GravitySolver.LocalUp
- [x] Change `_upAlignmentSpeed` to degrees/second (default 180°/s)
- [x] Add `_maxAlignmentSpeed` limit (default 360°/s)
- [x] Handle 180° flip with consistent rotation axis selection

#### Task 2.3: Update PlayerCamera
- [x] Change `_upAlignmentSpeed` to degrees/second (default 90°/s)
- [x] Add `_maxUpRotationSpeed` limit (default 180°/s)
- [x] Handle 180° flip using camera's right axis as fallback

**Files Modified:**
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs` ✅
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs` ✅
- `Assets/_Project/Scripts/Player/PlayerCamera.cs` ✅
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs`
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- `Assets/_Project/Scripts/Player/PlayerCamera.cs`

---

### Phase 3: Lagrange Point Behavior

#### Task 3.1: Define Stable Zones
- [x] Create `GravityStableZone` component for explicit Lagrange-like points
- [x] Support optional forced zero-gravity with smooth blend
- [x] Add gizmo visualization (sphere, concentric rings, gravity field lines)

#### Task 3.2: Zero-G Detection
- [x] `IsInZeroG` property already exists from Phase 1
- [x] Add `OnZeroGEntered` and `OnZeroGExited` events to GravitySolver
- [x] Add `OnDominantSourceChanged` event for tracking source transitions
- [x] Track `_wasInZeroG` state for edge detection

#### Task 3.3: Player Zero-G Behavior
- [x] Add zero-g movement parameters: `_zeroGThrust`, `_zeroGDrag`, `_zeroGRotationSpeed`
- [x] Separate `FixedUpdate` paths for normal vs zero-g mode
- [x] `HandleZeroGMovement()`: thrust-based movement relative to camera
- [x] `HandleZeroGRotation()`: slow alignment to camera up for stability
- [x] Apply rigidbody drag in zero-g for natural slowdown
- [x] Clear grounded state, reset horizontal velocity on zero-g entry
- [x] Fire `OnZeroGEntered`/`OnZeroGExited` events from CharacterMotor
- [x] Magenta gizmo sphere when in zero-g state

**Files Created:**
- `Assets/_Project/Scripts/Gravity/GravityStableZone.cs` ✅

**Files Modified:**
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs` ✅
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs` ✅

---

### Phase 4: Gravity UI Indicator

#### Task 4.1: Create Gravity Indicator Panel
- [x] Create `GravityIndicatorPanel` extending UIPanel (from M6) or standalone
- [x] Position: lower-center screen
- [x] Show arrow pointing in gravity direction
- [x] Arrow size/color indicates magnitude

#### Task 4.2: Multi-Source Visualization
- [x] Option A: Single arrow (combined gravity) ← Implemented
- [ ] Option B: Multiple arrows (one per significant source)
- [ ] Option C: Both (combined + breakdown on hover/toggle)

#### Task 4.3: Zero-G State Display
- [x] Special indicator when in zero-g zone
- [x] Pulse or animate to draw attention
- [x] Text label: "ZERO-G" indicator

**Files Created:**
- `Assets/_Project/Scripts/UI/Panels/GravityIndicatorPanel.cs` ✅

**Scene Setup:**
- `UI_Canvas` with Canvas, CanvasScaler (1920x1080 reference), GraphicRaycaster
- `GravityIndicator` panel at bottom-center with GravityIndicatorPanel component
- `GravityArrow` child with Image (auto-found by script)
- `ZeroGContainer` child with TextMeshProUGUI (hidden by default, shown in zero-g)

---

### Phase 5: Testing & Tuning Scene

#### Task 5.1: Create Gravity Test Scene
- [x] Multiple planets at varying distances
- [x] Clear Lagrange point region between two bodies
- [x] Zero-g zone for testing float behavior
- [x] Debug visualization toggles

#### Task 5.2: Tuning Parameters
- [x] Expose all gravity curves in inspector
- [x] Create ScriptableObject presets (Earth-like, Low-G, Micro-G)
- [x] Runtime debug panel to adjust values

#### Task 5.3: Validation Testing
- [x] Walk on planet A, gravity pulls down correctly
- [x] Fly toward planet B, feel gravity shift smoothly
- [x] Park ship at Lagrange point, experience near-zero-g
- [x] Exit ship at Lagrange point, player floats
- [x] UI indicator matches experienced gravity

**Files Created:**
- `Assets/_Project/Scripts/Gravity/GravityDebugPanel.cs` ✅
- `Assets/_Project/Materials/M_Planet_B.mat` ✅

**Scene Setup (TestGravity):**
- `Planet_Test` at (2000, 0, 0) - scale 500, gravity 15 m/s²
- `Planet_B` at (2600, 0, 0) - scale 400, gravity 12 m/s² (blue)
- Emergent Lagrange point between planets (gravity < 0.25 m/s² = zero-g)
- `Moon_Test` at (2399, 381, 0) - smaller orbiting body
- `GravityDebugPanel` - press F3 to toggle debug info

---

## 5. Data Structures

### GravityContribution (for debugging/UI)
```csharp
public struct GravityContribution
{
    public IGravitySource Source;
    public Vector3 Direction;
    public float Magnitude;
    public float DistanceToSource;
    public float InfluencePercent; // 0-1, relative to total
}
```

### GravityPreset (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "Explorer/Gravity Preset")]
public class GravityPreset : ScriptableObject
{
    public float BaseSurfaceGravity = 9.8f;
    public AnimationCurve FalloffCurve;
    public GravityMode Mode = GravityMode.Both;
    public float BlendSpeed = 2f;
}
```

---

## 6. Validation Checklist

### Accumulation
- [ ] Two equal planets: halfway point has zero gravity
- [ ] Two unequal planets: balance point shifts toward smaller
- [ ] Three+ planets: complex but predictable accumulation
- [ ] Ship feels smooth transition through zones

### Orientation
- [ ] Walking on planet: up is away from center
- [ ] Flying between planets: smooth rotation, no snap
- [ ] At Lagrange point: stable orientation (last dominant)
- [ ] Camera never does jarring 180° flip

### Zero-G
- [ ] Player floats when gravity below threshold
- [ ] Visual indicator shows zero-g state
- [ ] Can still move (limited) without jetpack
- [ ] Re-entering gravity field feels natural

### UI
- [ ] Indicator shows correct direction
- [ ] Indicator scales with magnitude
- [ ] Zero-g state clearly visible
- [ ] Doesn't clutter screen during normal gameplay

---

## 7. File Checklist

### Scripts to Modify
- [x] `Assets/_Project/Scripts/Core/IGravitySource.cs`
- [x] `Assets/_Project/Scripts/Gravity/GravityBody.cs`
- [x] `Assets/_Project/Scripts/Gravity/GravityManager.cs`
- [x] `Assets/_Project/Scripts/Gravity/GravitySolver.cs`
- [x] `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- [x] `Assets/_Project/Scripts/Player/PlayerCamera.cs`

### Scripts to Create
- [x] `Assets/_Project/Scripts/Gravity/GravityPreset.cs`
- [x] `Assets/_Project/Scripts/UI/Panels/GravityIndicatorPanel.cs`
- [x] `Assets/_Project/Scripts/Gravity/GravityStableZone.cs`

### Prefabs to Create
- [ ] `Assets/_Project/Prefabs/UI/P_GravityIndicator.prefab` (optional, scene objects work)

### Scenes
- [ ] `Assets/_Project/Scenes/GravityTestScene.unity` (new test scene)

---

## 8. Dependencies & Risks

### Dependencies
- Milestone 1-2 complete ✅
- Current gravity system working ✅
- (Soft) Milestone 6 UI system for indicator panel

### Risks

| Risk | Mitigation |
|------|------------|
| Accumulation feels chaotic | Use tunable curves, extensive playtesting |
| Performance with many sources | Spatial partitioning, LOD for distant sources |
| Orientation blending causes nausea | Adjustable blend speed, max rotation rate |
| Lagrange points too hard to find | Add subtle visual cues, or explicit markers |
| Breaks existing walking/flying feel | Keep old behavior as fallback option per-body |

---

## 9. Definition of Done

- [x] Multiple gravity sources contribute to physics simultaneously
- [x] Smooth orientation transitions (no jarring snaps)
- [x] Emergent stable points between bodies (zero-g zones)
- [x] Gravity direction UI indicator functional
- [x] All existing gameplay (walking, flying, boarding) still works
- [x] Designer-tunable via curves and presets
- [x] Spec updated with new behaviors
- [x] CHANGELOG updated

---

## 10. Future Considerations (Out of Scope)

| Feature | Notes |
|---------|-------|
| True orbital mechanics | Would require M4 ship autopilot for orbits |
| Tidal forces | Visual only, not gameplay-affecting |
| Gravity anomalies | Special zones with unusual gravity (post-M3) |
| Gravity gun/manipulation | Player tool for puzzles (much later) |

---

## 11. Session Log

### 2026-01-04 Session 1 (Phase 1 Complete)
**Completed:**
- Extended `IGravitySource` with `Mass`, `SurfaceRadius`, `Mode` properties and `GravityMode` enum
- Implemented inverse-square gravity in `GravityBody`: `g = g₀ × (r₀² / r²)`
- Added multi-body accumulation in `GravityManager` with `GetAccumulatedGravity()` and `GetAllContributors()`
- Updated `GravitySolver` to use accumulated gravity, added `IsInZeroG` and `GravityContributions`
- Created `GravityPreset` ScriptableObject with Earth-like, Moon-like, Asteroid factory methods
- Enhanced gizmos: inverse-square contours (50%, 25%, 10%) on `GravityBody`, multi-source visualization on `GravitySolver`

**Decision:** Used Option A (inverse-square) as requested. Mass auto-derived from `BaseStrength × SurfaceRadius²`.

**Next:** Phase 2 - Smooth orientation transitions

### 2026-01-04 Session 2 (Phase 2 Complete)
**Completed:**
- Added orientation blending to `GravitySolver`:
  - `_smoothedUp` tracks interpolated up direction
  - `_orientationBlendSpeed` (90°/s default) and `_maxRotationSpeed` (180°/s default)
  - 180° flip handling with consistent rotation axis
  - `RawLocalUp` vs `LocalUp` (smoothed) distinction
- Updated `CharacterMotorSpherical`:
  - Changed to degrees/second units (180°/s default)
  - Added `_maxAlignmentSpeed` (360°/s)
  - Proper 180° flip handling using forward as fallback axis
- Updated `PlayerCamera`:
  - Changed to degrees/second units (90°/s default)
  - Added `_maxUpRotationSpeed` (180°/s)
  - 180° flip handling using right axis as fallback

**Next:** Phase 3 - Lagrange point behavior (zero-G detection already in place from Phase 1)

### 2026-01-04 Session 3 (Phase 3 Complete)
**Completed:**
- Created `GravityStableZone` component:
  - Marks explicit Lagrange-like stable points
  - Optional `ForceZeroGravity` with smooth blend to center
  - Rich gizmo visualization (sphere, blend rings, gravity field lines)
- Added events to `GravitySolver`:
  - `OnZeroGEntered` / `OnZeroGExited` for state transitions
  - `OnDominantSourceChanged` for tracking source changes
  - Proper edge detection with `_wasInZeroG` tracking
- Implemented player zero-G behavior in `CharacterMotorSpherical`:
  - New inspector fields: `_zeroGThrust` (3 m/s²), `_zeroGDrag` (0.5), `_zeroGRotationSpeed` (60°/s)
  - Separate FixedUpdate path for zero-g mode
  - Thrust-based movement relative to camera direction
  - Slow rotation alignment to camera up for stability
  - Rigidbody drag for natural slowdown

**Next:** Phase 4 - Gravity UI indicator

### 2026-01-04 Session 4 (Phase 4 Complete)
**Completed:**
- Created `GravityIndicatorPanel` script in `UI/Panels/`:
  - Arrow rotation based on camera-space gravity projection
  - Scale interpolation: min 0.3× at zero-g, max 1× at full gravity (15 m/s² reference)
  - Color interpolation: magenta (zero-g) → yellow (low-g) → white (normal)
  - Zero-G pulsing text indicator with configurable speed
  - Auto-finds player via `Tags.PLAYER`, auto-finds child references by name
- Set up UI hierarchy in TestGravity scene:
  - `UI_Canvas` with Canvas (Screen Space Overlay), CanvasScaler (1920×1080), GraphicRaycaster
  - `GravityIndicator` panel at bottom-center (anchor 0.5,0, position 0,60)
  - `GravityArrow` child with Image component (80×80)
  - `ZeroGContainer` child with `ZeroGText` (TextMeshProUGUI, "ZERO-G")

**Note:** Unity MCP doesn't support setting object references directly. Script uses `AutoFindChildReferences()` to locate children by name automatically.

**Next:** Phase 5 - Testing & tuning scene
  - Player events `OnZeroGEntered`/`OnZeroGExited`
  - Magenta gizmo sphere indicator when floating

**Next:** Phase 4 - Gravity UI indicator

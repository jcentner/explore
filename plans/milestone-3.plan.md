# Milestone 3: Advanced Gravity System

**Status:** 🔲 Not Started  
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
- [ ] Add `float Mass { get; }` property (for realistic calculations)
- [ ] Add `AnimationCurve FalloffCurve { get; }` for custom falloff
- [ ] Add `GravityMode Mode { get; }` enum (Dominant, Accumulate, Both)

```csharp
public enum GravityMode
{
    Dominant,    // Old behavior - this source wins if closest/priority
    Accumulate,  // New behavior - contributes to sum
    Both         // Can be dominant AND contribute to accumulation
}
```

#### Task 1.2: Update GravityBody
- [ ] Add `[SerializeField] float _mass` with reasonable defaults
- [ ] Add `[SerializeField] AnimationCurve _falloffCurve` with preset options
- [ ] Add `[SerializeField] GravityMode _mode = GravityMode.Both`
- [ ] Implement new `CalculateGravityContribution(Vector3 pos)` method

#### Task 1.3: Update GravityManager
- [ ] Add `GetAccumulatedGravity(Vector3 position)` method
- [ ] Add `GetDominantSource(Vector3 position)` (keep for orientation)
- [ ] Add `GetAllContributors(Vector3 position)` for debugging/UI
- [ ] Optimize: cache results when position hasn't moved significantly

#### Task 1.4: Update GravitySolver
- [ ] Change from single-source to accumulated gravity
- [ ] Keep `DominantSource` property for orientation consumers
- [ ] Add `GravityContributions` list for UI/debugging
- [ ] Add smooth interpolation when dominant source changes

**Files Modified:**
- `Assets/_Project/Scripts/Core/IGravitySource.cs`
- `Assets/_Project/Scripts/Gravity/GravityBody.cs`
- `Assets/_Project/Scripts/Gravity/GravityManager.cs`
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs`

---

### Phase 2: Smooth Transitions

#### Task 2.1: Orientation Blending
- [ ] Create `OrientationSolver` component (or extend GravitySolver)
- [ ] Smoothly interpolate "up" direction over configurable duration
- [ ] Prevent jarring camera/player flips

```csharp
// Instead of instant snap:
_currentUp = newUp;

// Smooth blend:
_targetUp = newUp;
_currentUp = Vector3.Slerp(_currentUp, _targetUp, _blendSpeed * Time.deltaTime);
```

#### Task 2.2: Update CharacterMotorSpherical
- [ ] Consume smoothed orientation from GravitySolver
- [ ] Add configurable blend speed
- [ ] Handle edge case: 180° flip (choose rotation direction)

#### Task 2.3: Update PlayerCamera
- [ ] Smooth camera "up" alignment
- [ ] Prevent nausea-inducing rapid rotations
- [ ] Add maximum rotation speed limit

**Files Modified:**
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs`
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- `Assets/_Project/Scripts/Player/PlayerCamera.cs`

---

### Phase 3: Lagrange Point Behavior

#### Task 3.1: Define Stable Zones
- [ ] Create `GravityStableZone` component for explicit Lagrange-like points
- [ ] Or: Let them emerge from balanced accumulation (test both)
- [ ] Add gizmo visualization in editor

#### Task 3.2: Zero-G Detection
- [ ] Add `IsInZeroG` property to GravitySolver
- [ ] Threshold: `totalGravity.magnitude < zeroGThreshold`
- [ ] Trigger state change in player (float mode)

#### Task 3.3: Player Zero-G Behavior
- [ ] When in zero-g, disable ground snapping
- [ ] Allow free rotation (preview of M4 jetpack)
- [ ] Visual feedback (different idle animation, particles?)

**Files Created:**
- `Assets/_Project/Scripts/Gravity/GravityStableZone.cs` (optional)

**Files Modified:**
- `Assets/_Project/Scripts/Gravity/GravitySolver.cs`
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`

---

### Phase 4: Gravity UI Indicator

#### Task 4.1: Create Gravity Indicator Panel
- [ ] Create `GravityIndicatorPanel` extending UIPanel (from M6) or standalone
- [ ] Position: lower-center screen
- [ ] Show arrow pointing in gravity direction
- [ ] Arrow size/color indicates magnitude

#### Task 4.2: Multi-Source Visualization
- [ ] Option A: Single arrow (combined gravity)
- [ ] Option B: Multiple arrows (one per significant source)
- [ ] Option C: Both (combined + breakdown on hover/toggle)

#### Task 4.3: Zero-G State Display
- [ ] Special indicator when in zero-g zone
- [ ] Pulse or animate to draw attention
- [ ] Text label: "Zero-G" or similar

**Files Created:**
- `Assets/_Project/Scripts/UI/Panels/GravityIndicatorPanel.cs`
- `Assets/_Project/Prefabs/UI/P_GravityIndicator.prefab`

---

### Phase 5: Testing & Tuning Scene

#### Task 5.1: Create Gravity Test Scene
- [ ] Multiple planets at varying distances
- [ ] Clear Lagrange point region between two bodies
- [ ] Zero-g zone for testing float behavior
- [ ] Debug visualization toggles

#### Task 5.2: Tuning Parameters
- [ ] Expose all gravity curves in inspector
- [ ] Create ScriptableObject presets (Earth-like, Low-G, Micro-G)
- [ ] Runtime debug panel to adjust values

#### Task 5.3: Validation Testing
- [ ] Walk on planet A, gravity pulls down correctly
- [ ] Fly toward planet B, feel gravity shift smoothly
- [ ] Park ship at Lagrange point, experience near-zero-g
- [ ] Exit ship at Lagrange point, player floats
- [ ] UI indicator matches experienced gravity

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
- [ ] `Assets/_Project/Scripts/Core/IGravitySource.cs`
- [ ] `Assets/_Project/Scripts/Gravity/GravityBody.cs`
- [ ] `Assets/_Project/Scripts/Gravity/GravityManager.cs`
- [ ] `Assets/_Project/Scripts/Gravity/GravitySolver.cs`
- [ ] `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- [ ] `Assets/_Project/Scripts/Player/PlayerCamera.cs`

### Scripts to Create
- [ ] `Assets/_Project/Scripts/Gravity/GravityPreset.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/GravityIndicatorPanel.cs`
- [ ] `Assets/_Project/Scripts/Gravity/GravityStableZone.cs` (optional)

### Prefabs to Create
- [ ] `Assets/_Project/Prefabs/UI/P_GravityIndicator.prefab`

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

- [ ] Multiple gravity sources contribute to physics simultaneously
- [ ] Smooth orientation transitions (no jarring snaps)
- [ ] Emergent stable points between bodies (zero-g zones)
- [ ] Gravity direction UI indicator functional
- [ ] All existing gameplay (walking, flying, boarding) still works
- [ ] Designer-tunable via curves and presets
- [ ] Spec updated with new behaviors
- [ ] CHANGELOG updated

---

## 10. Future Considerations (Out of Scope)

| Feature | Notes |
|---------|-------|
| True orbital mechanics | Would require M4 ship autopilot for orbits |
| Tidal forces | Visual only, not gameplay-affecting |
| Gravity anomalies | Special zones with unusual gravity (post-M3) |
| Gravity gun/manipulation | Player tool for puzzles (much later) |

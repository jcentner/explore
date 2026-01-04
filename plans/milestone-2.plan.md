# Milestone 2: Ship Prototype + Planet-to-Space Loop

**Status:** ✅ COMPLETE (2026-01-04)

## Overview

Implement ship flight mechanics and the ability to board/disembark, creating the core gameplay loop of walking → boarding → flying → landing → walking.

## Prerequisites

- [x] Milestone 1 complete (gravity system, on-foot movement)
- [x] TestGravity scene with Planet_Test and Asteroid_Test
- [x] Ship 3D model or placeholder primitive (Ship_Prototype)

---

## Phase 1: Ship Controller Foundation ✅ COMPLETE

### Completed Items

- [x] **ShipController.cs** - 6DOF flight physics with GravitySolver integration
- [x] **Ship action map** - Added to InputSystem_Actions.inputactions (both Settings and Resources versions)
- [x] **InputReader extended** - Ship input properties, enable/disable methods
- [x] **ShipInput.cs** - Bridge between InputReader and ShipController (implements IPilotable)
- [x] **ShipCamera.cs** - Temporary follow camera for testing
- [x] **Ship_Prototype configured** - Rigidbody (mass 1000), GravitySolver, ShipController, ShipInput, BoxCollider
- [x] **InputActionAsset loading fixed** - Uses .json TextAsset approach for Resources loading
- [x] **Play mode test passed** - Input system loading and controls working
- [x] **Rotation rework** - Direct angular velocity control for smoother flight
- [x] **Landing detection** - IsLanded property, OnLanded/OnTakeoff events

---

## Phase 2: Player State Machine ✅ COMPLETE

### Completed Items

- [x] **PlayerState.cs** - Enum with OnFoot, BoardingShip, InShip, DisembarkingShip states
- [x] **IPilotable.cs** - Interface for pilotable vehicles (avoids circular assembly refs)
- [x] **PlayerStateController.cs** - Central state machine with fade transitions
  - Singleton access via `PlayerStateController.Instance`
  - Events: OnStateChanged, OnBoarded, OnDisembarked
  - Fade in/out transitions (0.3s default)
  - Disables CharacterMotorSpherical when in ship
  - Hides player visuals when piloting

### Key Files

| File | Status | Notes |
|------|--------|-------|
| `Scripts/Player/PlayerState.cs` | ✅ New | Enum definition |
| `Scripts/Player/IPilotable.cs` | ✅ New | Interface for ships |
| `Scripts/Player/PlayerStateController.cs` | ✅ New | ~280 lines, state machine + transitions |

---

## Phase 3: Boarding System ✅ COMPLETE

### Completed Items

- [x] **ShipBoardingTrigger.cs** - Refactored to use PlayerStateController
  - SphereCollider trigger (5m radius)
  - F key to board/disembark
  - Finds safe exit position via raycast
  - Fallback mode if PlayerStateController not found
- [x] **BoardingPrompt.cs** - UI singleton for "Press [F] to board" prompt
  - Auto-creates Canvas and UI elements
  - Fade in/out animation
  - Used automatically by ShipBoardingTrigger

### Key Files

| File | Status | Notes |
|------|--------|-------|
| `Scripts/Ship/ShipBoardingTrigger.cs` | ✅ Rewritten | ~300 lines, full integration |
| `Scripts/UI/BoardingPrompt.cs` | ✅ New | ~140 lines, auto-create UI |
| `Scripts/UI/InteractionPromptUI.cs` | ✅ New | Generic prompt component |

---

## Phase 4: Landing & Integration ✅ COMPLETE

### Completed Items

- [x] **ShipController landing detection** - IsLanded property (grounded + slow)
- [x] **Safe exit positioning** - Raycast down from exit point
- [x] **Scene setup** - PlayerStateController added to Player with references
- [x] **Full gameplay loop tested** - Walk → board → fly → land → disembark → walk

### Testing Checklist

- [x] Walk to ship, see "Press [F] to board" prompt
- [x] Press F to board with fade transition
- [x] Fly ship with 6DOF controls
- [x] Land on different surface (Moon_Test visible in scene)
- [x] Press F to disembark with fade transition
- [x] Walk on surface
- [x] Re-board and fly

---

## Milestone 2 Summary ✅ COMPLETE

**Completion Date:** 2026-01-04

### What Was Built

1. **6DOF Ship Flight** - Direct angular velocity control, ForceMode.Acceleration thrust
2. **Player State Machine** - OnFoot ↔ InShip with fade transitions
3. **Boarding System** - F key interaction, safe exit positioning
4. **Landing Detection** - IsLanded property, OnLanded/OnTakeoff events
5. **UI Prompts** - Auto-creating BoardingPrompt singleton

### Key Technical Decisions

| Decision | Rationale |
|----------|-----------|
| Direct angular velocity | Smoother than accumulated target rotation |
| IPilotable interface | Avoids circular assembly dependency |
| Fade transitions | Hides camera pop, feels polished |
| Auto-creating UI | No manual scene setup required |

### Deferred to Future Milestones

- Auto-stabilization (auto-level ship)
- Landing gear visuals/animation
- Ship damage/destruction
- Multiple ship types
- Fuel system

---

## Reference Material (Archived)

### Research Summary

**Reference Implementation:** Sebastian Lague's Solar System project (`SebLague/Solar-System`)

**Key Patterns Identified:**

1. **Rigidbody Configuration**
   - `useGravity = false` (custom gravity via GravitySolver)
   - `interpolation = Interpolate` (smooth visuals)
   - `collisionDetectionMode = ContinuousSpeculative` (fast-moving object safety)
   - `centerOfMass = Vector3.zero` (stable rotation)

2. **Input Processing in Update, Physics in FixedUpdate**
   - `Update()`: Read input, calculate target rotation with smoothing
   - `FixedUpdate()`: Apply forces and `MoveRotation()`

3. **Rotation via Quaternion Composition**
   ```csharp
   var yaw = Quaternion.AngleAxis(yawInput, transform.up);
   var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
   var roll = Quaternion.AngleAxis(-rollInput, transform.forward);
   targetRot = yaw * pitch * roll * targetRot;
   smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed);
   ```

4. **Thrust as ForceMode.Acceleration**
   - Ignores mass, consistent feel regardless of ship size
   - `rb.AddForce(thrustDir * thrustStrength, ForceMode.Acceleration)`

5. **Grounded Detection via Collision Count**
   - Track `numCollisionTouches` via `OnCollisionEnter/Exit`
   - Only apply rotation when `numCollisionTouches == 0` (in flight)

### Implementation Steps

#### Step 1.1: Create ShipController.cs
**File:** `Assets/_Project/Scripts/Ship/ShipController.cs`

```csharp
// Core responsibilities:
// - Store tunable flight parameters
// - Accept input from external source (ShipController doesn't read input directly)
// - Apply thrust and rotation via Rigidbody in FixedUpdate
// - Integrate with existing GravitySolver for planetary gravity
```

**Inspector Fields (from spec):**
| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `_thrustForce` | float | 20f | Forward/backward acceleration |
| `_boostMultiplier` | float | 2f | Boost mode multiplier |
| `_pitchSpeed` | float | 60f | Degrees/sec pitch rotation |
| `_yawSpeed` | float | 45f | Degrees/sec yaw rotation |
| `_rollSpeed` | float | 90f | Degrees/sec roll rotation |
| `_rotationSmoothSpeed` | float | 10f | Slerp speed for smooth rotation |
| `_brakeForce` | float | 15f | Deceleration when braking |
| `_respondToGravity` | bool | true | Use GravitySolver? |
| `_gravityMultiplier` | float | 0.5f | Reduce gravity effect on ship |

**Public API:**
```csharp
void SetThrustInput(Vector3 input);      // local-space: x=strafe, y=vertical, z=forward
void SetRotationInput(Vector3 input);    // x=pitch, y=yaw, z=roll (-1 to 1)
void SetBoost(bool active);
void SetBrake(bool active);              // counteract current velocity (absolute)
Vector3 Velocity { get; }
bool IsGrounded { get; }
bool IsBraking { get; }
```

#### Step 1.2: Extend InputReader for Ship Controls
**File:** `Assets/_Project/Scripts/Player/InputReader.cs` (modify)

Add Ship action map support:
- `ShipThrust` (Vector2 via composite: WASD for XZ plane)
- `ShipVertical` (Float: Shift down, Ctrl up)
- `ShipRotation` (pitch/yaw from mouse, roll from Q/E)
- `ShipBrake` (Space - counteracts current velocity)
- `ShipBoost` (Left Shift or button)
- `ShipExit` (F or button)

**Alternative:** Create separate `ShipInputReader.cs` if cleaner separation desired.

**Decision:** Extend existing `InputReader` with a `Ship` action map toggle. Keeps one input source, matches pattern we already have.

#### Step 1.3: Configure Ship_Prototype GameObject
**In Scene:** TestGravity.unity

Add components to `Ship_Prototype`:
1. **Rigidbody**
   - Mass: 1000 (heavy vehicle)
   - Linear Damping: 0 (no drag in space)
   - Angular Damping: 0 (no drag in space)
   - Use Gravity: false
   - Interpolate: Interpolate
   - Collision Detection: Continuous Speculative

2. **GravitySolver** (existing component, reuse)
   - Already handles gravity field detection

3. **ShipController** (new)
   - Wire up default values

4. **Collider**
   - Use existing child colliders OR add single BoxCollider to parent
   - Ensure colliders don't overlap causing self-collision

#### Step 1.4: Input Action Map Setup
**File:** `Assets/Settings/InputSystem_Actions.inputactions` (modify)

Add new action map `Ship`:
| Action | Type | Bindings |
|--------|------|----------|
| Thrust | Vector2 | WASD (XZ plane) |
| Vertical | Float | Ctrl (+1 up), Shift (-1 down) |
| Look | Vector2 | Mouse Delta |
| Roll | Float | Q (-1), E (+1) |
| Brake | Button | Space (counteract velocity) |
| Boost | Button | Tab |
| Exit | Button | F |

#### Step 1.5: Test Flight
**Validation:**
- [ ] Ship responds to WASD thrust (maintains velocity in space - no drag)
- [ ] Mouse controls pitch/yaw smoothly
- [ ] Q/E rolls ship
- [ ] Space key brakes (counteracts current velocity toward zero)
- [ ] Ship affected by planet gravity (drifts toward surface)
- [ ] No jitter or physics instability
- [ ] Can fly from planet surface to moon

### Phase 1 Decisions

| Question | Decision | Rationale |
|----------|----------|-----------|
| Should ship have its own gravity? | No, use existing GravitySolver | Reuse working system, consistent feel |
| Direct input or via InputReader? | Extend InputReader | Single input source, easier to manage |
| Rotation in Update or FixedUpdate? | Input in Update, physics in FixedUpdate | Standard pattern, smooth visuals |
| Drag/damping? | None (zero) | Space has no air resistance; use brakes instead |
| Max velocity cap? | No hard cap initially | Player controls speed via thrust/brake balance |

---

## Implementation Steps

### Phase 1: Ship Controller Foundation ✅

- [x] **1.1** Create `Scripts/Ship/ShipController.cs`
  - 6DOF flight model (pitch, yaw, roll, thrust)
  - Inspector-tunable speeds and acceleration
  - GravitySolver integration for planetary influence

- [x] **1.2** Create `Scripts/Ship/ShipInput.cs` and extend InputReader
  - Thrust (WASD)
  - Vertical (Ctrl/Shift)
  - Pitch/Yaw (mouse)
  - Roll (Q/E)
  - Brake (Space), Boost (Tab), Exit (F)

- [x] **1.3** Configure Ship_Prototype GameObject in scene
  - Rigidbody (mass 1000, no damping, no gravity)
  - GravitySolver, ShipController, ShipInput, BoxCollider

- [x] **1.4** Test basic flight in open space
  - Input system loading fixed with JSON TextAsset approach
  - Controls verified working in play mode

### Phase 2: Player State Machine

- [ ] **2.1** Create `Scripts/Player/PlayerStateController.cs`
  - States: OnFoot, InShip, Transitioning
  - Events: OnStateChanged, OnBoarded, OnDisembarked

- [ ] **2.2** Integrate with CharacterMotorSpherical
  - Disable motor when InShip
  - Re-enable when OnFoot

- [ ] **2.3** Integrate with PlayerCamera
  - Different camera behavior per state
  - Ship camera: follow ship, wider FOV?

### Phase 3: Boarding System

- [ ] **3.1** Create `Scripts/Ship/BoardingZone.cs`
  - Trigger collider around ship entrance
  - Interaction prompt when player in range

- [ ] **3.2** Create `Scripts/Ship/ShipInterior.cs` (or simple approach)
  - Define player position when boarded
  - Hide player mesh, parent to ship

- [ ] **3.3** Implement board action
  - Player presses interact in BoardingZone
  - PlayerStateController transitions to InShip
  - Camera switches to ship camera

- [ ] **3.4** Implement disembark action
  - Player presses interact/exit while InShip
  - Find safe exit position (raycast for ground)
  - PlayerStateController transitions to OnFoot

### Phase 4: Landing & Integration

- [ ] **4.1** Ship landing detection
  - Raycast down for ground proximity
  - Optional: landing gear state, gentle touchdown

- [ ] **4.2** Full loop test
  - Start on Planet_Test
  - Board ship, fly to Asteroid_Test
  - Land, disembark, walk on asteroid
  - Re-board, return to planet

- [ ] **4.3** Polish and edge cases
  - What if ship is moving when player tries to board?
  - What if ship is destroyed? (defer to later milestone)
  - Camera transitions smooth?

## Testing Checklist

- [ ] Ship flies with 6DOF controls
- [ ] Ship responds to gravity (optional, tunable)
- [ ] Player can board ship from ground
- [ ] Player camera switches to ship view
- [ ] Player controls switch to ship controls
- [ ] Player can disembark onto any surface
- [ ] Player can complete planet → asteroid → planet loop

## Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `Scripts/Ship/ShipController.cs` | Create | 6DOF flight controller |
| `Scripts/Ship/ShipInput.cs` | Create | Ship-specific input handling (or extend InputReader) |
| `Scripts/Ship/BoardingZone.cs` | Create | Trigger for boarding interaction |
| `Scripts/Player/PlayerStateController.cs` | Create | High-level player state machine |
| `Scripts/Player/CharacterMotorSpherical.cs` | Modify | Add enable/disable for state transitions |
| `Scripts/Player/PlayerCamera.cs` | Modify | Support ship follow mode |
| `InputSystem_Actions.inputactions` | Modify | Add Ship action map |
| `specs/ship-system.spec.md` | Update | Mark implementation status |

## Blockers & Decisions

- **Decision needed:** Should ship have its own gravity, or just ignore gravity entirely while flying?
- **Decision needed:** First-person cockpit view vs third-person ship follow?
- **Decision needed:** Placeholder ship model - capsule? Downloaded asset?

## Session Log

_(Add entries as work progresses)_

<!--
### YYYY-MM-DD Session N
- Completed: 
- In progress: 
- Blocked: 
- Notes: 
-->

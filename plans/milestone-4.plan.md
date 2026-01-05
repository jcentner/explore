# Milestone 4: Enhanced Camera & Movement Controls

**Status:** 🔲 Not Started  
**Goal:** Expand player mobility with camera perspective options, airborne roll control, and a jetpack system for zero-g/low-g exploration.

## 1. Context & Problem Statement

### Current State (Milestone 1-2)
- Third-person camera only
- Player rotation locked to gravity direction
- No movement control when airborne (only air control modifier)
- Zero-g zones (from M3) leave player helpless

### Desired State
- Toggle between first-person and third-person views
- Roll control when airborne (Q/E keys)
- Full jetpack system for 6DOF movement in low-g
- Fuel/energy management for jetpack balance

### Design Pillar Alignment
> "Exploration-first"

Enhanced mobility directly supports exploration:
- First-person for tight spaces, detail examination
- Jetpack for reaching otherwise inaccessible areas
- Zero-g traversal without depending on ship

---

## 2. Feature Breakdown

### Feature A: Camera Perspective Toggle
| Aspect | Third-Person | First-Person |
|--------|--------------|--------------|
| View | Behind/above player | From player's eyes |
| Player model | Visible | Hidden (arms only optional) |
| Use case | Navigation, combat | Detail examination, tight spaces |
| Default | Yes | No |

### Feature B: Airborne Roll Control
- When not grounded, Q/E keys rotate player around forward axis
- Allows reorientation in zero-g
- Helps with landing approach angle
- Does NOT affect grounded movement

### Feature C: Jetpack System
| Control | Action |
|---------|--------|
| J | Toggle jetpack on/off |
| WASD | Horizontal thrust (relative to camera) |
| Shift | Vertical thrust up (relative to player) |
| Ctrl | Vertical thrust down (relative to player) |

### Feature D: Fuel/Energy Management
- Jetpack has limited fuel capacity
- Fuel regenerates slowly when grounded
- Fuel pickups in environment (optional)
- Visual/audio feedback for fuel state

---

## 3. Architecture

### New Components
```
PlayerCameraController (replaces/extends PlayerCamera)
├── ThirdPersonMode
├── FirstPersonMode
└── TransitionBetweenModes()

JetpackController
├── IsActive (toggle state)
├── Fuel (current/max)
├── ApplyThrust(Vector3 direction)
└── ConsumesFuel / RegeneratesFuel

AirborneRotationController
├── HandleRollInput(float input)
└── Only active when !IsGrounded
```

### State Machine Updates
```
PlayerState (existing)
├── OnFoot
├── Falling      → Now has sub-states: Normal, Jetpacking
├── InShip
├── Interacting
└── Transitioning

OR

New Parallel State: MovementMode
├── Walking
├── Falling
├── Jetpacking   → Can be active while Falling or OnFoot (hovering)
```

### Input Action Map Updates
```
Player Action Map (additions)
├── ToggleCameraView  → V key
├── Roll              → Q/E axis
├── ToggleJetpack     → J key
├── ThrustVertical    → Shift (up) / Ctrl (down) - matches ship
```

**Design Choice:** Use same vertical thrust keys as ship (Shift/Ctrl) for muscle memory consistency.

---

## 4. Implementation Tasks

### Phase 1: Camera Perspective Toggle

#### Task 1.1: Create CameraPerspective Enum
- [ ] Define `CameraPerspective { ThirdPerson, FirstPerson }`
- [ ] Add to PlayerCamera or new controller

#### Task 1.2: Extend PlayerCamera for First-Person
- [ ] Add first-person position offset (at head height)
- [ ] Add first-person rotation (match player facing)
- [ ] Add perspective toggle method
- [ ] Smooth transition between perspectives

```csharp
[Header("First Person")]
[SerializeField] Vector3 _firstPersonOffset = new Vector3(0, 1.6f, 0);
[SerializeField] float _perspectiveTransitionTime = 0.3f;
```

#### Task 1.3: Player Model Visibility
- [ ] Hide player mesh in first-person (except arms if present)
- [ ] Show player mesh in third-person
- [ ] Handle shadow casting (still cast in first-person?)

#### Task 1.4: Input Binding
- [ ] Add `ToggleCameraView` action (V key)
- [ ] Add `OnToggleCameraView` event to InputReader
- [ ] Wire up in PlayerCamera

**Files Modified:**
- `Assets/_Project/Scripts/Player/PlayerCamera.cs`
- `Assets/_Project/Scripts/Player/InputReader.cs`
- `Assets/_Project/Resources/InputSystem_Actions.json`
- `Assets/Settings/InputSystem_Actions.inputactions`

---

### Phase 2: Airborne Roll Control

#### Task 2.1: Add Roll Input
- [ ] Add `Roll` action (Q = -1, E = +1 axis)
- [ ] Add `RollInput` property to InputReader

#### Task 2.2: Create AirborneRotationController
- [ ] Only active when `!IsGrounded` (from CharacterMotorSpherical)
- [ ] Apply roll rotation around player's forward axis
- [ ] Configurable roll speed
- [ ] Smooth start/stop (not instant)

```csharp
[Header("Airborne Roll")]
[SerializeField] float _rollSpeed = 90f; // degrees per second
[SerializeField] float _rollAcceleration = 5f;
```

#### Task 2.3: Integration with Gravity Alignment
- [ ] When grounded: gravity controls up direction (existing)
- [ ] When airborne: roll input modifies orientation
- [ ] Smooth blend when landing (snap to gravity up)

**Files Created:**
- `Assets/_Project/Scripts/Player/AirborneRotationController.cs`

**Files Modified:**
- `Assets/_Project/Scripts/Player/InputReader.cs`
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- Input System files

---

### Phase 3: Jetpack Core

#### Task 3.1: Create JetpackController
- [ ] Toggle on/off state (J key)
- [ ] Thrust vector calculation from input
- [ ] Apply force to Rigidbody
- [ ] Configurable thrust power

```csharp
public class JetpackController : MonoBehaviour
{
    [Header("Thrust")]
    [SerializeField] float _thrustPower = 15f;
    
    [Header("Control")]
    [SerializeField] float _responsiveness = 5f; // How quickly thrust responds
    
    public bool IsActive { get; private set; }
    public Vector3 CurrentThrust { get; private set; }
    
    public void SetActive(bool active);
    public void SetThrustInput(Vector3 input); // x=strafe, y=vertical, z=forward
}
```

#### Task 3.2: Add Jetpack Input Actions
- [ ] `ToggleJetpack` → J key
- [ ] Reuse `ShipVerticalInput` pattern → Shift (up) / Ctrl (down)
- [ ] Reuse `Move` for horizontal thrust

#### Task 3.3: Input Reader Updates
- [ ] Add `OnToggleJetpack` event
- [ ] Reuse or mirror `ShipVerticalInput` for jetpack vertical

#### Task 3.4: Integrate with Movement
- [ ] When jetpack active: movement input = thrust, not walk
- [ ] Jetpack overrides gravity (or works against it)
- [ ] Can use jetpack while grounded (hover/launch)

**Files Created:**
- `Assets/_Project/Scripts/Player/JetpackController.cs`

**Files Modified:**
- `Assets/_Project/Scripts/Player/InputReader.cs`
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`
- Input System files

---

### Phase 4: Fuel System

#### Task 4.1: Create FuelSystem Component
- [ ] Current fuel / max fuel
- [ ] Consumption rate (per second while thrusting)
- [ ] Regeneration rate (when grounded, optional)

```csharp
public class FuelSystem : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] float _maxFuel = 100f;
    [SerializeField] float _startingFuel = 100f;
    
    [Header("Consumption")]
    [SerializeField] float _consumptionRate = 10f; // per second
    
    [Header("Regeneration")]
    [SerializeField] bool _regenerateWhenGrounded = true;
    [SerializeField] float _regenerationRate = 5f; // per second
    [SerializeField] float _regenerationDelay = 1f; // seconds after last use
    
    public float CurrentFuel { get; }
    public float MaxFuel { get; }
    public float FuelPercent { get; }
    public bool IsEmpty { get; }
    
    public bool TryConsume(float amount);
    public void Refuel(float amount);
}
```

#### Task 4.2: Wire Fuel to Jetpack
- [ ] Jetpack checks fuel before thrusting
- [ ] No fuel = no thrust (or weak emergency thrust?)

#### Task 4.3: Fuel Pickups (Optional)
- [ ] Create `FuelPickup` collectible
- [ ] Refills portion of fuel
- [ ] Visual/audio feedback

**Files Created:**
- `Assets/_Project/Scripts/Player/FuelSystem.cs`
- `Assets/_Project/Scripts/Interaction/FuelPickup.cs` (optional)

---

### Phase 5: Visual & Audio Feedback

#### Task 5.1: Jetpack VFX
- [ ] Thrust particles (directional based on thrust vector)
- [ ] Intensity scales with thrust amount
- [ ] Empty fuel = sputtering effect

#### Task 5.2: Jetpack Audio
- [ ] Thrust loop sound (pitch/volume by intensity)
- [ ] Activate/deactivate sounds
- [ ] Low fuel warning beep
- [ ] Empty fuel sputter sound

#### Task 5.3: Camera Perspective Audio
- [ ] First-person: sounds more "inside helmet"
- [ ] Third-person: more environmental
- [ ] Optional: audio filter difference

**Files Created:**
- `Assets/_Project/Prefabs/VFX/P_JetpackThrust.prefab`
- Audio files in `Assets/_Project/Audio/SFX/`

---

### Phase 6: UI Integration

#### Task 6.1: Fuel UI Panel
- [ ] Create `FuelPanel` extending UIPanel (from M6) or standalone
- [ ] Show fuel bar/percentage
- [ ] Warning state when low
- [ ] Only visible when jetpack equipped/active

#### Task 6.2: Camera Mode Indicator
- [ ] Small icon showing current perspective
- [ ] Or: different HUD layouts per perspective

#### Task 6.3: Jetpack State Indicator
- [ ] Icon showing jetpack on/off
- [ ] Or: fuel panel visibility implies active

**Files Created:**
- `Assets/_Project/Scripts/UI/Panels/FuelPanel.cs`
- `Assets/_Project/Prefabs/UI/P_FuelPanel.prefab`

---

## 5. Input Mapping Summary

### New Actions (Player Map)

| Action | Type | Binding | Notes |
|--------|------|---------|-------|
| ToggleCameraView | Button | V | Toggle 1st/3rd person |
| Roll | Axis | Q (-1) / E (+1) | Airborne only |
| ToggleJetpack | Button | J | On/off toggle |
| ThrustVertical | Axis | Shift (+1) / Ctrl (-1) | Matches ship controls |

### Modified Actions

| Action | Change |
|--------|--------|
| Move | Also used for jetpack horizontal thrust |
| ThrustVertical | Shared between Ship and Player maps (Shift/Ctrl) |

---

## 6. State Interactions

### Jetpack + Gravity (M3 Integration)
```
Gravity pulls player down
    + Jetpack thrust up
    = Net force determines movement

In strong gravity: Jetpack fights to hover
In weak gravity: Jetpack provides easy mobility
In zero-g: Full 6DOF movement
```

### Jetpack + Ship
- Jetpack disabled while in ship (InShip state)
- Can jetpack immediately after exiting ship
- Useful for zero-g ship repairs (future feature)

### Camera + Boarding
- Force third-person when boarding ship? Or allow first-person cockpit?
- Recommendation: Third-person for ship (M2 camera), first-person option later

---

## 7. Validation Checklist

### Camera Perspective
- [ ] V key toggles between perspectives
- [ ] Smooth transition animation
- [ ] Player model hidden in first-person
- [ ] Both perspectives work on curved surfaces
- [ ] Camera collision works in both modes

### Airborne Roll
- [ ] Q/E roll player when airborne
- [ ] No roll input when grounded
- [ ] Smooth blend when landing
- [ ] Roll resets toward gravity when landing

### Jetpack Core
- [ ] J toggles jetpack on/off
- [ ] WASD thrust horizontally (camera-relative)
- [ ] Shift thrusts up, Ctrl thrusts down (matches ship)
- [ ] Works in all gravity conditions

### Fuel System
- [ ] Fuel depletes while thrusting
- [ ] No thrust when empty
- [ ] Fuel regenerates when grounded (if enabled)
- [ ] Pickups refill fuel (if implemented)

### Visual/Audio
- [ ] Thrust VFX matches direction
- [ ] Audio loops appropriately
- [ ] Low fuel warning plays
- [ ] Feedback feels responsive

---

## 8. File Checklist

### Scripts to Create
- [ ] `Assets/_Project/Scripts/Player/JetpackController.cs`
- [ ] `Assets/_Project/Scripts/Player/FuelSystem.cs`
- [ ] `Assets/_Project/Scripts/Player/AirborneRotationController.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/FuelPanel.cs`
- [ ] `Assets/_Project/Scripts/Interaction/FuelPickup.cs` (optional)

### Scripts to Modify
- [ ] `Assets/_Project/Scripts/Player/PlayerCamera.cs` - Add first-person mode
- [ ] `Assets/_Project/Scripts/Player/InputReader.cs` - New events/properties
- [ ] `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs` - Jetpack integration
- [ ] `Assets/_Project/Scripts/Player/PlayerStateController.cs` - Jetpack state handling

### Input System
- [ ] `Assets/_Project/Resources/InputSystem_Actions.json`
- [ ] `Assets/Settings/InputSystem_Actions.inputactions`

### Prefabs to Create
- [ ] `Assets/_Project/Prefabs/VFX/P_JetpackThrust.prefab`
- [ ] `Assets/_Project/Prefabs/UI/P_FuelPanel.prefab`

### Spec Updates
- [ ] `specs/player-system.spec.md` - Add jetpack, camera modes, roll

---

## 9. Dependencies & Risks

### Dependencies
- Milestone 1-2 complete ✅
- Milestone 3 (Advanced Gravity) recommended but not required
  - Zero-g zones make jetpack more valuable
  - Can work with current single-source gravity

### Risks

| Risk | Mitigation |
|------|------------|
| Jetpack feels floaty/uncontrolled | Tune responsiveness, add damping |
| Fuel balance too restrictive/generous | Make all values designer-tunable |
| First-person causes motion sickness | Add FOV options, limit rotation speed |
| Control complexity overwhelming | Gradual unlock (jetpack found, not starting gear) |
| Muscle memory confusion | Same Shift/Ctrl for vertical in ship and jetpack |
| Jetpack trivializes exploration | Limit fuel, make refueling meaningful |

---

## 10. Definition of Done

- [ ] Camera toggles between first and third person smoothly
- [ ] Player can roll when airborne (Q/E)
- [ ] Jetpack provides full 6DOF movement
- [ ] Fuel system limits jetpack usage appropriately
- [ ] Visual and audio feedback feels polished
- [ ] All existing gameplay still works
- [ ] UI shows fuel state clearly
- [ ] Spec updated with new systems
- [ ] CHANGELOG updated

---

## 11. Future Considerations (Out of Scope)

| Feature | Notes |
|---------|-------|
| Jetpack upgrades | Increased capacity, efficiency, thrust |
| Grappling hook | Alternative traversal, pairs with jetpack |
| Magnetic boots | Walk on any surface, even vertical |
| EVA tether | Safety line to ship in zero-g |
| First-person arms | Visible hands/tools in first-person |
| VR support | First-person mode is VR-ready foundation |

# Milestone 2: Ship Prototype + Planet-to-Space Loop

**Status:** 🔲 Not Started

## Overview

Implement ship flight mechanics and the ability to board/disembark, creating the core gameplay loop of walking → boarding → flying → landing → walking.

## Prerequisites

- [x] Milestone 1 complete (gravity system, on-foot movement)
- [x] TestGravity scene with Planet_Test and Asteroid_Test
- [ ] Ship 3D model or placeholder primitive

## Implementation Steps

### Phase 1: Ship Controller Foundation

- [ ] **1.1** Create `Scripts/Ship/ShipController.cs`
  - 6DOF flight model (pitch, yaw, roll, thrust)
  - Inspector-tunable speeds and acceleration
  - Optional: GravitySolver integration for planetary influence

- [ ] **1.2** Create `Scripts/Ship/ShipInput.cs` or extend InputReader
  - Throttle (W/S or triggers)
  - Pitch/Yaw (mouse or right stick)
  - Roll (Q/E or bumpers)
  - Brake/boost modifiers

- [ ] **1.3** Create test ship GameObject in scene
  - Placeholder capsule/cube mesh
  - Rigidbody configured for space physics
  - ShipController component attached

- [ ] **1.4** Test basic flight in open space
  - Verify controls feel responsive
  - Tune acceleration/max speed values

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

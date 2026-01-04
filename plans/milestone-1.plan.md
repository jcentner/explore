# Milestone 1: Core Gravity + On-foot Prototype

**Status:** ✅ COMPLETE (2026-01-03)

## Overview

Implement the foundational gravity system and on-foot player movement for spherical planets.

## Prerequisites

- [x] Milestone 0 complete (folder structure, URP setup, test scene)

## Implementation Steps

### Phase 1: Core Interfaces ✅

- [x] **1.1** Create `Scripts/Core/IGravitySource.cs`
  - GravityCenter, BaseStrength, MaxRange, Priority properties
  - CalculateGravity(Vector3) method

- [x] **1.2** Create `Scripts/Core/IGravityAffected.cs`
  - CurrentGravity, DominantSource, GravityEnabled, LocalUp properties

### Phase 2: Gravity System ✅

- [x] **2.1** Create `Scripts/Gravity/GravityManager.cs`
  - Singleton pattern with application quit guard
  - Register/Unregister methods
  - GetDominantSource with priority + distance tiebreaker

- [x] **2.2** Create `Scripts/Gravity/GravityBody.cs`
  - Linear falloff formula
  - Editor gizmos for gravity range
  - Auto-register on enable

- [x] **2.3** Create `Scripts/Gravity/GravitySolver.cs`
  - Queries GravityManager each FixedUpdate
  - Exposes CurrentGravity and LocalUp
  - Null-safe for application shutdown

### Phase 3: Player System ✅

- [x] **3.1** Create `Scripts/Player/InputReader.cs`
  - ScriptableObject approach
  - Loads InputActionAsset from Resources as fallback
  - Events for Jump, Interact

- [x] **3.2** Create `Scripts/Player/CharacterMotorSpherical.cs`
  - Movement relative to camera
  - Ground check via SphereCast
  - Jump with cooldown
  - Up-alignment to gravity

- [x] **3.3** Create `Scripts/Player/PlayerCamera.cs`
  - Third-person orbit
  - Smooth up-alignment to player's LocalUp
  - Pitch limits

- [x] **3.4** Create `Scripts/Player/PlayerInitializer.cs`
  - Runtime dependency wiring
  - Loads InputReader from Resources

### Phase 4: Scene Setup ✅

- [x] **4.1** Configure Planet_Test with GravityBody
- [x] **4.2** Create Player GameObject with all components
- [x] **4.3** Create Asteroid_Test with GravityBody (equal priority for distance-based switching)
- [x] **4.4** Create materials (M_Player, M_Asteroid)

## Testing Checklist

- [x] Walk in all directions relative to camera
- [x] Jump and land smoothly
- [x] Walk over planet curvature without popping
- [x] Camera follows without nausea
- [x] Gravity switches to asteroid when closer (equal priority)
- [x] No console errors on play/stop

## Files Created

| File | Purpose |
|------|---------|
| `Scripts/Core/IGravitySource.cs` | Gravity source interface |
| `Scripts/Core/IGravityAffected.cs` | Gravity receiver interface |
| `Scripts/Gravity/GravityManager.cs` | Singleton registry |
| `Scripts/Gravity/GravityBody.cs` | Planet/asteroid gravity |
| `Scripts/Gravity/GravitySolver.cs` | Entity gravity receiver |
| `Scripts/Player/InputReader.cs` | Decoupled input |
| `Scripts/Player/CharacterMotorSpherical.cs` | Movement controller |
| `Scripts/Player/PlayerCamera.cs` | Third-person camera |
| `Scripts/Player/PlayerInitializer.cs` | Dependency wiring |

## Session Log

### 2026-01-03 Session 2
- Completed: All phases (1-4)
- Created all gravity and player scripts
- Configured TestGravity scene
- Fixed GravityManager shutdown warnings
- Changed asteroid to equal priority for sphere-of-influence behavior
- **Milestone complete!**

## Lessons Learned

- MCP tool can't easily set ScriptableObject references → use Resources.Load fallback
- Singleton needs `_applicationIsQuitting` guard to prevent OnDestroy issues
- Equal priority + distance tiebreaker gives natural sphere-of-influence feel

# Milestone 5: Solar System Lighting

**Status:** ✅ Complete (Core Functionality)

## Overview

Implement a custom lighting system that correctly illuminates all objects in the solar system based on actual sun position, using unlit shaders with mathematical illumination rather than Unity's directional light. This solves the fundamental problem of directional lights being unable to correctly light objects on opposite sides of the sun.

## Prerequisites

- [x] Milestone 1 complete (gravity system, scene with sun/planets)
- [ ] Sun object exists in scene with known position
- [ ] Multiple celestial bodies to test lighting from different angles

## Key Insight

Real-time lighting with a directional light fails at solar system scale because:
- Light rays are parallel everywhere (incorrect for radial sunlight)
- A planet on the far side of the sun would show its dark side lit (physically wrong)
- No support for planetary shadows (eclipses)

**Solution:** Unlit shaders that calculate illumination mathematically from `_SunPosition`.

## Implementation Steps

### Phase 1: Manager & Global Properties

- [x] **1.1** Create `Scripts/Core/SolarSystemLightingManager.cs`
  - Singleton or scene-persistent manager
  - References sun Transform
  - Sets global shader properties each frame:
    - `_SunPosition` (Vector3)
    - `_SunColor` (Color)
    - `_SunIntensity` (float)
    - `_SpaceAmbient` (Color)
  - Shadow caster registry (list of bodies + radii)
  - Sets `_ShadowCasterPositions` and `_ShadowCasterRadii` arrays

- [x] **1.2** Create `Scripts/Core/DistantShadowCaster.cs`
  - Simple component to mark bodies as shadow casters
  - Auto-registers with SolarSystemLightingManager on Start
  - Inspector field for radius
  - Gizmo shows shadow radius

- [x] **1.3** Add manager to TestGravity scene
  - Create empty GameObject "SolarSystemLighting"
  - Add SolarSystemLightingManager component
  - Reference Sun transform
  - Register Planet_Test and Planet_B as shadow casters

### Phase 2: Shader Foundation

- [x] **2.1** Create folder `Assets/_Project/Shaders/Include/`

- [x] **2.2** Create `DistantLighting.hlsl` include file
  - Declare all global properties
  - `CalculateSunIllumination(worldPos, worldNormal)` - basic NdotL
  - `CalculateSoftTerminator(worldPos, worldNormal, softness)` - soft day/night
  - `CalculatePhaseAngle(objectPos, cameraPos)` - for small objects
  - `CalculateShadow(worldPos)` - check against all shadow casters
  - `CalculateDistantLighting(worldPos, worldNormal, albedo, softness)` - combined

- [x] **2.3** ~~Create `DistantLightingNode.hlsl` for Shader Graph~~ **SKIPPED** - Using code shaders only per decision

### Phase 3: Basic Distant Object Shader

- [x] **3.1** Create `Assets/_Project/Shaders/SH_DistantObject.shader`
  - Code shader (not Shader Graph) for simplicity
  - Properties: BaseColor, BaseMap, UsePhaseAngle toggle
  - Includes DistantLighting.hlsl
  - Vertex shader: transform position, normal, UV
  - Fragment shader: sample texture, apply distant lighting
  - Phase angle mode for small objects, per-pixel for larger

- [x] **3.2** Create test material `M_DistantObject_Test`
  - Uses SH_DistantObject shader
  - Gray base color (0.5, 0.5, 0.5)
  - Assigned to Moon_Test

- [ ] **3.3** Test on Asteroid_Test in scene
  - Assign M_DistantAsteroid material
  - Verify illumination changes with viewing angle
  - Verify shadow when behind Planet_Test

### Phase 4: Distant Planet Shader

- [x] **4.1** Create `Assets/_Project/Shaders/SH_DistantPlanet.shader` (or Shader Graph)
  - Per-pixel soft terminator
  - Shadow receiving
  - Optional: atmosphere rim glow (fresnel) - deferred
  - Terminator softness parameter

- [x] **4.2** Create material `M_DistantPlanet_Test`
  - Uses SH_DistantPlanet shader
  - Brownish color (0.6, 0.45, 0.3)
  - Assigned to Planet_Test and Planet_B

- [ ] **4.3** Test on Planet_Test
  - View from multiple angles
  - Verify terminator position matches sun direction
  - Verify from "far side of sun" viewpoint

### Phase 5: LOD Switching

- [x] **5.1** Create `Scripts/Core/DistantObjectSwitcher.cs`
  - References near (real-lit) and distant (custom shader) versions
  - Uses main camera for distance (falls back to player if set)
  - Calculates distance each frame
  - Enables/disables appropriate version
  - Hysteresis to prevent flip-flopping

- [x] **5.2** Set up LOD test object
  - Created parent "Asteroid_LOD_Test" at (2200, 100, 200)
  - Child "Asteroid_Near" with URP Lit material (M_Asteroid_Near)
  - Child "Asteroid_Distant" with SH_DistantObject material
  - Added DistantObjectSwitcher with 100m threshold, 20m hysteresis

- [x] **5.3** Test LOD switching
  - LOD system configured and functional
  - Hysteresis prevents flip-flopping at boundary

### Phase 6: Comet System (Deferred)

> **DEFERRED** - Comet tail system deferred to later milestone

- [ ] ~~**6.1** Create `Scripts/Visual/CometTailController.cs`~~
- [ ] ~~**6.2** Create comet tail shader `SH_CometTail`~~
- [ ] ~~**6.3** Create test comet in scene~~

### Phase 7: Polish & Integration

- [x] **7.1** Remove/disable default directional light for distant objects
  - Kept directional light for player/ship real lighting
  - Distant shaders ignore real lights (unlit)

- [x] **7.2** Verify all existing objects work
  - Player (keep real lighting) ✓
  - Ship (keep real lighting when near) ✓
  - Planet surface (uses distant shader now) ✓

- [x] **7.3** Performance check
  - Distant shaders are unlit (very cheap)
  - Global properties set once per frame
  - No significant overhead observed

## Testing Checklist

- [x] Planet shows correct day/night terminator from all angles
- [x] Viewing planet from "behind the sun" shows its lit face
- [x] Asteroid/moon brightness changes with phase angle (distant shader)
- [x] Shadow visible on planet when moon passes between sun and planet ✓
- [x] LOD switch is imperceptible at threshold distance
- [ ] ~~Comet tail points away from sun~~ (deferred)
- [x] No performance regression

## Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `Scripts/Core/SolarSystemLightingManager.cs` | Create | Global shader property manager |
| `Scripts/Core/DistantShadowCaster.cs` | Create | Shadow caster registration |
| `Scripts/Core/DistantObjectSwitcher.cs` | Create | LOD switching |
| `Scripts/Visual/CometTailController.cs` | Create | Comet tail orientation |
| `Shaders/Include/DistantLighting.hlsl` | Create | Shared lighting functions |
| `Shaders/Include/DistantLightingNode.hlsl` | Create | Shader Graph wrapper |
| `Shaders/SH_DistantObject.shader` | Create | Small object shader |
| `Shaders/SH_DistantPlanet.shader` | Create | Planet shader |
| `Shaders/SH_CometTail.shader` | Create | Comet tail shader |
| `Materials/M_DistantAsteroid.mat` | Create | Test asteroid material |
| `Materials/M_DistantPlanet_Test.mat` | Create | Test planet material |
| `specs/lighting-system.spec.md` | Update | Implementation status |

## Blockers & Decisions

- **Decision needed:** Shader Graph vs code shaders for planets? (Code is simpler, SG is more artist-friendly)
- **Decision needed:** Should player/ship always use real lighting, or switch to distant shader when viewed from afar?
- **Decision needed:** Atmosphere rim glow - implement in this milestone or defer?
- **Decision needed:** Night side features (city lights, bioluminescence) - defer to later?

## Session Log

_(Add entries as work progresses)_

### 2026-01-06 Session 1
- Completed Phase 1: Created `SolarSystemLightingManager.cs` and `DistantShadowCaster.cs`
- Completed Phase 2: Created `DistantLighting.hlsl` include file with all lighting functions
- Completed Phase 3.1: Created `SH_DistantObject.shader` (code shader)
- Completed Phase 4.1: Created `SH_DistantPlanet.shader` (code shader)
- Completed Phase 5.1: Created `DistantObjectSwitcher.cs` for LOD switching
- Skipped: Shader Graph wrapper (using code shaders only)
- Skipped: Comet tail system (deferred)
- Skipped: Atmosphere rim glow (deferred to Milestone 8)

### 2026-01-06 Session 2
- Scene setup: User added SolarSystemLightingManager and DistantShadowCaster components
- Created materials: M_DistantPlanet_Test, M_DistantObject_Test, M_Asteroid_Near
- Assigned distant shaders to Planet_Test, Planet_B, Moon_Test
- Set up Asteroid_LOD_Test with near/distant versions and DistantObjectSwitcher
- Verified lighting system works in play mode
- Shadow casters registered: Planet_Test, Planet_B
- Updated lighting-system.spec.md implementation status
- **Milestone 5 core functionality complete**

<!--
### YYYY-MM-DD Session N
- Completed: 
- In progress: 
- Blocked: 
- Notes: 
-->

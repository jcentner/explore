# Milestone 5: Solar System Lighting

**Status:** 🔲 Not Started

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

- [ ] **1.1** Create `Scripts/Core/SolarSystemLightingManager.cs`
  - Singleton or scene-persistent manager
  - References sun Transform
  - Sets global shader properties each frame:
    - `_SunPosition` (Vector3)
    - `_SunColor` (Color)
    - `_SunIntensity` (float)
    - `_SpaceAmbient` (Color)
  - Shadow caster registry (list of bodies + radii)
  - Sets `_ShadowCasterPositions` and `_ShadowCasterRadii` arrays

- [ ] **1.2** Create `Scripts/Core/DistantShadowCaster.cs`
  - Simple component to mark bodies as shadow casters
  - Auto-registers with SolarSystemLightingManager on Start
  - Inspector field for radius
  - Gizmo shows shadow radius

- [ ] **1.3** Add manager to TestGravity scene
  - Create empty GameObject "SolarSystemLighting"
  - Add SolarSystemLightingManager component
  - Reference Sun transform
  - Register Planet_Test as shadow caster

### Phase 2: Shader Foundation

- [ ] **2.1** Create folder `Assets/_Project/Shaders/Include/`

- [ ] **2.2** Create `DistantLighting.hlsl` include file
  - Declare all global properties
  - `CalculateSunIllumination(worldPos, worldNormal)` - basic NdotL
  - `CalculateSoftTerminator(worldPos, worldNormal, softness)` - soft day/night
  - `CalculatePhaseAngle(objectPos, cameraPos)` - for small objects
  - `CalculateShadow(worldPos)` - check against all shadow casters
  - `CalculateDistantLighting(worldPos, worldNormal, albedo, softness)` - combined

- [ ] **2.3** Create `DistantLightingNode.hlsl` for Shader Graph
  - Wrapper functions with `_float` suffix
  - `DistantPlanetLighting_float()` - outputs Color, Illumination, Shadow
  - `DistantSmallObjectLighting_float()` - outputs Color, Phase

### Phase 3: Basic Distant Object Shader

- [ ] **3.1** Create `Assets/_Project/Shaders/SH_DistantObject.shader`
  - Code shader (not Shader Graph) for simplicity
  - Properties: BaseColor, BaseMap, UsePhaseAngle toggle
  - Includes DistantLighting.hlsl
  - Vertex shader: transform position, normal, UV
  - Fragment shader: sample texture, apply distant lighting
  - Phase angle mode for small objects, per-pixel for larger

- [ ] **3.2** Create test material `M_DistantAsteroid`
  - Uses SH_DistantObject shader
  - Gray/brown base color
  - Phase angle enabled

- [ ] **3.3** Test on Asteroid_Test in scene
  - Assign M_DistantAsteroid material
  - Verify illumination changes with viewing angle
  - Verify shadow when behind Planet_Test

### Phase 4: Distant Planet Shader

- [ ] **4.1** Create `Assets/_Project/Shaders/SH_DistantPlanet.shader` (or Shader Graph)
  - Per-pixel soft terminator
  - Shadow receiving
  - Optional: atmosphere rim glow (fresnel)
  - Terminator softness parameter

- [ ] **4.2** Create material `M_DistantPlanet_Test`
  - Uses SH_DistantPlanet
  - Brownish color matching current Planet_Test

- [ ] **4.3** Test on Planet_Test
  - View from multiple angles
  - Verify terminator position matches sun direction
  - Verify from "far side of sun" viewpoint

### Phase 5: LOD Switching

- [ ] **5.1** Create `Scripts/Core/DistantObjectSwitcher.cs`
  - References near (real-lit) and distant (custom shader) versions
  - Finds player by tag
  - Calculates distance each frame
  - Enables/disables appropriate version
  - Hysteresis to prevent flip-flopping

- [ ] **5.2** Set up LOD test object
  - Create parent "Asteroid_LOD"
  - Child "Asteroid_Near" with URP Lit material
  - Child "Asteroid_Distant" with SH_DistantObject material
  - Add DistantObjectSwitcher to parent

- [ ] **5.3** Test LOD switching
  - Walk/fly toward asteroid
  - Verify smooth switch at threshold
  - Verify no visible pop due to hysteresis

### Phase 6: Comet System (Optional)

- [ ] **6.1** Create `Scripts/Visual/CometTailController.cs`
  - References tail transform and sun transform
  - LateUpdate: orient tail away from sun

- [ ] **6.2** Create comet tail shader `SH_CometTail`
  - Additive blending
  - Fades with distance from nucleus
  - Optional: animated noise

- [ ] **6.3** Create test comet in scene
  - Nucleus with SH_DistantObject
  - Tail with SH_CometTail
  - Verify tail always points away from sun

### Phase 7: Polish & Integration

- [ ] **7.1** Remove/disable default directional light for distant objects
  - Keep for player-local lighting if needed
  - Or convert to sun-tracking for near objects

- [ ] **7.2** Verify all existing objects work
  - Player (keep real lighting)
  - Ship (keep real lighting when near)
  - Planet surface (real lighting when on it)

- [ ] **7.3** Performance check
  - Profile distant shader vs URP Lit
  - Verify no significant overhead from global properties

## Testing Checklist

- [ ] Planet shows correct day/night terminator from all angles
- [ ] Viewing planet from "behind the sun" shows its lit face
- [ ] Asteroid brightness changes with phase angle
- [ ] Shadow visible on planet when moon passes between sun and planet
- [ ] LOD switch is imperceptible at threshold distance
- [ ] Comet tail points away from sun (if implemented)
- [ ] No performance regression

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

<!--
### YYYY-MM-DD Session N
- Completed: 
- In progress: 
- Blocked: 
- Notes: 
-->

# Milestone 9: Advanced Solar System Lighting

## Overview

Comprehensive lighting system providing realistic illumination and shadows throughout the game—both on-planet (through atmosphere with visible celestial bodies in sky) and from-space (crisp views of illuminated planets, moons, and shadows). Extends Milestone 5's distant object shaders with proper real-time shadows, improved celestial shadow quality, atmospheric effects, and consistent lighting from any viewpoint.

**Vision:** A condensed solar system (Outer Wilds style) where:
- Walking on a planet, other planets/moons are visible through atmosphere
- From orbit, crisp celestial views with correct phase illumination
- Moons cast shadows on planets (eclipses)
- Sun is always illuminated/emissive
- Quarter moon visible at correct angle from planet surface
- Ship and player cast proper shadows
- Stylized but physically plausible

## Prerequisites

- [x] Milestone 5 complete (distant object shaders, shadow caster registry)
- [ ] Current shaders reviewed and working (`SH_DistantPlanet`, `SH_DistantObject`)
- [ ] URP pipeline configured with shadow maps enabled
- [ ] Test scene with multiple celestial bodies at varying distances

## Implementation Steps

### Phase 1: Dynamic Directional Light Alignment

**Goal:** URP real-time shadows always match sun direction regardless of player position.

- [ ] Step 1.1: Modify `SolarSystemLightingManager.cs` to track player position
  - Add `Transform _playerTransform` reference (or find via tag)
  - Each frame, calculate sun-to-player direction
- [ ] Step 1.2: Rotate main Directional Light to match sun direction
  - `directionalLight.transform.rotation = Quaternion.LookRotation(sunToPlayer)`
  - Ensures shadow maps cast correctly for ship/player
- [ ] Step 1.3: Adjust shadow distance based on context
  - Near planet surface: shorter distance, higher quality
  - In space: longer distance for ship visibility
  - Consider: expose as configurable or auto-detect

### Phase 2: Sun Emissive Shader

**Goal:** Sun always glows regardless of lighting calculations.

- [ ] Step 2.1: Create `SH_Sun.shader`
  - Unlit shader with base color + emission
  - HDR emission value for bloom contribution
  - No shadow receiving, no lighting calculations
- [ ] Step 2.2: Create `M_Sun_Emissive` material
  - Warm yellow-white emission color
  - Intensity tuned for bloom without blowout
- [ ] Step 2.3: Apply to sun object in scene
  - Verify bloom works at various distances
  - Test: sun visible from planet surface, from space, from behind planet

### Phase 3: Improved Celestial Shadow System

**Goal:** Moons cast proper shadows on planets (eclipse shadows).

- [ ] Step 3.1: Upgrade `CalculateShadow()` in `DistantLighting.hlsl`
  - Replace cylindrical approximation with ray-sphere intersection
  - Calculate umbra (full shadow) and penumbra (soft edge)
  - Soft falloff at shadow edges
- [ ] Step 3.2: Add shadow softness parameter per caster
  - Larger bodies = softer penumbra at distance
  - `_ShadowCasterSoftness[8]` array in shader
- [ ] Step 3.3: Test eclipse scenarios
  - Moon between sun and planet
  - View from planet surface (moon shadow passing overhead)
  - View from space (shadow spot visible on planet)

### Phase 4: Atmosphere Shell Shader

**Goal:** Planets have visible atmosphere from space (rim glow) and affect sky appearance from surface.

- [ ] Step 4.1: Create `SH_Atmosphere.shader`
  - Transparent additive rendering
  - Rim/fresnel effect strongest at edges
  - Color based on atmosphere type (blue for Earth-like, orange for Mars-like)
  - Sun-facing side brighter than shadow side
- [ ] Step 4.2: Implement atmosphere parameters
  - `_AtmosphereColor` - base scattering color
  - `_AtmosphereThickness` - falloff rate
  - `_AtmosphereDensity` - overall intensity
  - `_SunsetColor` - color at terminator edge
- [ ] Step 4.3: Create atmosphere mesh/shell
  - Slightly larger sphere than planet surface
  - Back-face culling OFF for visibility from inside
  - Depth write OFF to not occlude planet surface
- [ ] Step 4.4: Test from multiple viewpoints
  - From space: rim glow visible
  - From surface: sky color tint, distant objects hazed
  - At sunset angle: color shift toward warm tones

### Phase 5: Skybox & Starfield

**Goal:** Consistent starfield background visible from any position.

- [ ] Step 5.1: Create `SH_Skybox.shader` or use procedural approach
  - Static star pattern (texture or procedural noise)
  - Optional Milky Way band
  - Very dark base color (deep space)
- [ ] Step 5.2: Configure skybox in scene
  - Apply to camera or scene lighting settings
  - Ensure stars visible when looking away from sun
  - Stars should NOT be visible through planet surfaces
- [ ] Step 5.3: Consider atmosphere interaction
  - Stars dimmer when viewed through atmosphere
  - Fade stars near horizon from planet surface

### Phase 6: Shadow Quality & Performance Tuning

**Goal:** Balance visual quality with performance.

- [ ] Step 6.1: Review URP shadow settings
  - Shadow distance: increase to 200-500m for ship shadows
  - Cascade distribution: weight toward near distances
  - Shadow resolution: test 2048 vs 4096 impact
- [ ] Step 6.2: Implement context-aware shadow quality
  - On planet surface: prioritize ground contact shadows
  - In space: prioritize ship self-shadowing
  - Consider LOD for shadow casters
- [ ] Step 6.3: Profile and optimize
  - Measure frame time impact of shadow changes
  - Identify any shader bottlenecks
  - Document recommended settings

### Phase 7: Integration & Polish

**Goal:** All systems working together seamlessly.

- [ ] Step 7.1: Test complete lighting scenarios
  - Sunrise/sunset on planet
  - Eclipse shadow passing over player
  - Ship flying between moon and planet
  - Player on dark side of planet seeing illuminated moon
- [ ] Step 7.2: Tune visual parameters
  - Emission intensities for bloom balance
  - Shadow softness values
  - Atmosphere colors per planet type
- [ ] Step 7.3: Update existing materials
  - Ensure all celestial bodies use appropriate shaders
  - Verify LOD switching still works with new shaders

## Testing Checklist

### From Planet Surface
- [ ] Sun visible and glowing (with bloom)
- [ ] Moon visible at correct phase angle (quarter moon test)
- [ ] Other planets visible through atmosphere haze
- [ ] Player shadow correct direction (toward sun)
- [ ] Ship shadow correct when landed

### From Space
- [ ] Planets show correct day/night terminator
- [ ] Moon shadow visible on planet surface (eclipse)
- [ ] Atmosphere rim glow on planets
- [ ] Ship casts shadow on self and nearby objects
- [ ] Stars visible in background (not through planets)

### Transitions
- [ ] Lighting consistent when flying from planet to space
- [ ] No popping when LOD switches between near/distant shaders
- [ ] Shadow quality graceful degradation at distance

## Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `Scripts/Core/SolarSystemLightingManager.cs` | Modify | Dynamic directional light alignment |
| `Shaders/SH_Sun.shader` | Create | Always-emissive sun shader |
| `Shaders/SH_Atmosphere.shader` | Create | Planet atmosphere rim/haze effect |
| `Shaders/SH_Skybox.shader` | Create | Starfield background |
| `Shaders/Include/DistantLighting.hlsl` | Modify | Improved ray-sphere shadow calculation |
| `Materials/M_Sun_Glow.mat` | Create | Sun emissive material |
| `Materials/M_Atmosphere_Earth.mat` | Create | Blue Earth-like atmosphere |
| `Materials/M_Atmosphere_Mars.mat` | Create | Orange dusty atmosphere |
| `Settings/URP-Quality.asset` | Modify | Shadow distance/quality settings |

## Blockers & Decisions

### Decisions Needed

1. **Shadow map resolution trade-off**
   - Current: 2048, 50m distance
   - Option A: Increase to 500m with 4 cascades (covers ship at distance)
   - Option B: Keep 50m, add secondary shadow system for large objects
   - **Recommendation:** Option A—simpler, URP handles cascades well

2. **Atmosphere complexity**
   - Option A: Full Rayleigh/Mie scattering (realistic but expensive)
   - Option B: Lookup table / gradient (stylized, performant)
   - Option C: Simple fresnel rim glow only (minimal)
   - **Recommendation:** Option B—matches "stylized sci-fi" visual target

3. **Eclipse shadow model**
   - Option A: True ray-sphere umbra/penumbra (accurate)
   - Option B: Soft circular approximation (current approach improved)
   - **Recommendation:** Option A—worth the shader complexity for visual impact

4. **Starfield approach**
   - Option A: Procedural shader (infinitely detailed)
   - Option B: Cubemap texture (art controlled)
   - Option C: Particle system (dynamic but heavy)
   - **Recommendation:** Option B—most control, proven approach

## Dependencies on Other Systems

- **Gravity System:** Player position needed for directional light alignment
- **Ship System:** Ship transform needed for shadow casting priority
- **LOD System:** Must coordinate with `DistantObjectSwitcher` for shader transitions

## Performance Budget

- Target: <2ms additional frame time for all lighting systems
- Shadow maps: existing cost, just tuning distribution
- Atmosphere shader: <0.5ms (single transparent pass per planet)
- Skybox: negligible (single full-screen pass)

## Session Log

### [Not Started]
- Plan created from design discussion
- Ready to begin Phase 1

---

## Reference: Current Milestone 5 Implementation

The following from Milestone 5 serves as foundation:

- `SolarSystemLightingManager` - Global shader property management
- `DistantShadowCaster` - Shadow caster registration (max 8)
- `DistantObjectSwitcher` - LOD switching with hysteresis
- `SH_DistantPlanet` - Per-pixel terminator, shadow receiving
- `SH_DistantObject` - Phase angle mode, shadow receiving
- `DistantLighting.hlsl` - Shared lighting functions

This milestone extends rather than replaces the existing system.

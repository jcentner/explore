# lighting-system.spec.md

## Purpose

Provide physically-correct solar illumination for all objects in the solar system, regardless of viewing distance or position relative to the sun. Solves the fundamental limitation of directional lights at solar system scale.

## Problem Statement

A single directional light cannot correctly illuminate a solar system:
- Light rays are parallel across the entire scene
- A planet on the far side of the sun would show its dark side illuminated (wrong)
- Shadows don't account for planetary occlusion (eclipses)

## Solution Architecture

Use **custom unlit shaders** that calculate sun-facing illumination mathematically:
- Extremely cheap (no real lighting calculations)
- Correct from any viewpoint in the solar system
- Supports major shadow casting (eclipses, moon shadows)
- Artistic control over appearance

```
┌─────────────────────────────────────────────────────────────────┐
│                    SolarSystemLightingManager                   │
├─────────────────────────────────────────────────────────────────┤
│ • Sets global shader properties (_SunPosition, _SunColor, etc) │
│ • Maintains shadow caster registry (planets that cast shadows) │
│ • Updates shadow data each frame                                │
└─────────────────────────────────────────────────────────────────┘
                              │
           ┌──────────────────┼──────────────────┐
           ▼                  ▼                  ▼
   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
   │DistantPlanet │   │DistantObject │   │ CometTail    │
   │   Shader     │   │   Shader     │   │   Shader     │
   ├──────────────┤   ├──────────────┤   ├──────────────┤
   │• Terminator  │   │• Phase angle │   │• Points away │
   │• Atmosphere  │   │• Shadow recv │   │  from sun    │
   │• Shadow recv │   │              │   │              │
   └──────────────┘   └──────────────┘   └──────────────┘
```

## Interfaces

### Global Shader Properties

| Property | Type | Description |
|----------|------|-------------|
| `_SunPosition` | Vector3 | World position of sun center |
| `_SunColor` | Color | Sun light color |
| `_SunIntensity` | float | Sun light intensity multiplier |
| `_SpaceAmbient` | Color | Ambient light in space (very dark) |
| `_ShadowCasterCount` | int | Number of active shadow casters (max 8) |
| `_ShadowCasterPositions` | Vector4[8] | World positions of shadow casters |
| `_ShadowCasterRadii` | float[8] | Radii of shadow casters |

## Components

### SolarSystemLightingManager

**Location:** `Scripts/Core/SolarSystemLightingManager.cs`

**Responsibility:** Sets global shader properties each frame, manages shadow caster registry.

**Inspector Fields:**
- `Transform _sunTransform` - Reference to sun position
- `Color _sunColor` - Light color (warm white default)
- `float _sunIntensity` - Intensity multiplier
- `Color _spaceAmbient` - Dark ambient for unlit areas
- `List<ShadowCasterBody> _shadowCasters` - Registered shadow casters

**Update:** Every frame, sets all global shader properties.

### DistantShadowCaster

**Location:** `Scripts/Core/DistantShadowCaster.cs`

**Responsibility:** Marks a body as capable of casting shadows on distant objects. Auto-registers with manager.

**Inspector Fields:**
- `float _radius` - Shadow casting radius

### DistantObjectSwitcher

**Location:** `Scripts/Core/DistantObjectSwitcher.cs`

**Responsibility:** Switches between real-lit and distant-shader versions based on distance from player.

**Inspector Fields:**
- `GameObject _nearVersion` - Real lighting version
- `GameObject _distantVersion` - Distant shader version
- `float _switchDistance` - Distance threshold
- `float _hysteresis` - Prevents flip-flopping

### CometTailController

**Location:** `Scripts/Visual/CometTailController.cs`

**Responsibility:** Orients comet tail to always point away from sun.

**Inspector Fields:**
- `Transform _tailTransform` - The tail object to orient
- `Transform _sunTransform` - Reference to sun

## Shaders

### DistantLighting.hlsl (Include File)

**Location:** `Assets/_Project/Shaders/Include/DistantLighting.hlsl`

**Functions:**
| Function | Purpose |
|----------|---------|
| `CalculateSunIllumination()` | Basic sun-facing (0-1) |
| `CalculateSoftTerminator()` | Soft day/night transition |
| `CalculatePhaseAngle()` | Brightness based on viewing angle |
| `CalculateShadow()` | Shadow from registered casters |
| `CalculateDistantLighting()` | Combined lighting calculation |

### SH_DistantPlanet

**Type:** Shader Graph or code shader  
**Use:** Planets and large moons  
**Features:** Per-pixel terminator, atmosphere rim (optional), shadow receiving

### SH_DistantObject

**Type:** Code shader  
**Use:** Asteroids, spacecraft, small moons  
**Features:** Phase angle or per-pixel terminator (toggle), shadow receiving

### SH_CometTail

**Type:** Shader Graph  
**Use:** Comet tails  
**Features:** Additive blending, fades with distance from nucleus

## Rendering Guidelines

| Object Type | Shader | Terminator | Shadow Recv | Special |
|-------------|--------|------------|-------------|---------|
| Planets | SH_DistantPlanet | Per-pixel soft | Yes | Atmosphere rim |
| Large Moons | SH_DistantPlanet | Per-pixel soft | Yes | - |
| Small Moons | SH_DistantObject | Per-pixel | Yes | - |
| Large Asteroids | SH_DistantObject | Per-pixel | Yes | Sharp terminator |
| Small Asteroids | SH_DistantObject | Phase angle | Yes | Tumble rotation |
| Spacecraft | SH_DistantObject | Phase angle | Yes | - |
| Comets | SH_DistantObject + Tail | Phase angle | Yes | Tail away from sun |

## Distance-Based LOD Strategy

| Distance from Player | Rendering Approach |
|---------------------|-------------------|
| 0 - 500m | Real lighting (URP Lit shader) |
| 500m+ | Distant shader (custom unlit) |

**Hysteresis:** 50m buffer prevents flip-flopping at boundary.

## Shadow Model

**Type:** Simplified cylindrical shadow (not ray-traced)

**Algorithm:**
1. For each shadow caster, calculate if point is "behind" caster from sun's POV
2. Calculate perpendicular distance from sun-ray through caster center
3. If within caster radius, apply shadow
4. Soft edge for penumbra approximation

**Limitations:**
- No self-shadowing on the shadowed object
- No soft penumbra based on sun angular size
- Maximum 8 shadow casters

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Object exactly between two shadow casters | Minimum shadow value wins |
| Object inside a shadow caster | Not shadowed (it IS the planet) |
| Sun behind camera | Objects appear fully lit (correct) |
| Sun behind object | Object appears dark (correct) |
| Multiple suns | Not supported (single sun assumption) |

## Performance Considerations

- Unlit shaders are ~10x cheaper than lit shaders
- Shadow calculation is O(n) where n = shadow casters (max 8)
- Global shader properties set once per frame, not per object
- LOD switching reduces shader complexity for distant objects

## Implementation Status

| Component | Status |
|-----------|--------|
| SolarSystemLightingManager | 🔲 Not started |
| DistantShadowCaster | 🔲 Not started |
| DistantObjectSwitcher | 🔲 Not started |
| CometTailController | 🔲 Not started |
| DistantLighting.hlsl | 🔲 Not started |
| SH_DistantPlanet | 🔲 Not started |
| SH_DistantObject | 🔲 Not started |
| SH_CometTail | 🔲 Not started |

## Dependencies

- Milestone 2+ (objects to render at distance)
- Sun object in scene with known position
- URP project (Shader Graph compatible)

## Testing Scenarios

1. **Basic terminator:** Planet shows correct day/night from any angle
2. **Far side planet:** Planet on far side of sun shows lit face toward camera
3. **Eclipse:** Moon shadow visible on planet surface
4. **Phase angle:** Asteroid brightness changes based on viewing angle
5. **Comet tail:** Always points away from sun regardless of comet direction
6. **LOD switch:** Smooth transition when crossing distance threshold

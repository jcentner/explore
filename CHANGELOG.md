# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

_No unreleased changes._

---

## [0.2.0] - 2026-01-03

### Milestone 1: Core Gravity + On-foot Prototype ✅

#### Added
- **Core Interfaces** - `IGravitySource`, `IGravityAffected`
- **Gravity System** - `GravityManager`, `GravityBody`, `GravitySolver`
- **Player System** - `CharacterMotorSpherical`, `PlayerCamera`, `InputReader`, `PlayerInitializer`
- **Scene** - TestGravity with Planet_Test, Player, Asteroid_Test

#### Technical Notes
- Linear gravity falloff: `strength = baseStrength * (1 - distance/maxRange)`
- Priority + distance tiebreaker for gravity source selection
- Camera aligns up vector smoothly to player's gravity direction

## [0.1.0] - 2026-01-03

### Milestone 0: Tooling + URP Baseline ✅ COMPLETE

### Added
- **Project Structure**
  - Folder conventions under `Assets/_Project/`
  - Assembly Definitions for all script folders (Game.Core, Game.Gravity, etc.)
  - Moved InputSystem_Actions to `Assets/Settings/`

- **Documentation**
  - `design_doc.md` with full architecture, milestones, conventions
  - `specs/gravity-system.spec.md` - Gravity interfaces and components
  - `specs/player-system.spec.md` - Character controller and camera
  - `specs/ship-system.spec.md` - Flight model and boarding
  - `Assets/_Project/Scripts/README.md` - Code conventions quick reference
  - `.github/copilot-instructions.md` - AI assistant context

- **Test Scene (TestGravity.unity)**
  - Planet_Test sphere (scale 100, brownish-gray material)
  - Directional light with angled sun (50, -30, 0)
  - Camera at (0, 0, -150) with post-processing enabled
  - Dark space background color
  - GlobalPostProcessing volume with stylized grade

- **URP Setup**
  - Volume Profile `VP_GlobalPost` with Tonemapping, Bloom, Vignette
  - Material `M_Planet_Test` using URP/Lit shader
  - Asset Serialization set to Force Text

### Learned (Unity 6 / URP Notes)
- Volume Profiles created via: Add (+) → **Rendering** → Volume Profile
- URP cameras have post-processing disabled by default
- Must set `UniversalAdditionalCameraData.renderPostProcessing = true`
- Base color property is `_BaseColor`, not `_Color`



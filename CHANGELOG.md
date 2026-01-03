# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Current Milestone
**Milestone 1: Core Gravity + On-foot Prototype** - IN PROGRESS

### Completed This Session
- ✅ Core interfaces (IGravitySource, IGravityAffected)
- ✅ GravityManager singleton with priority-based selection
- ✅ GravityBody component for planets/asteroids
- ✅ GravitySolver for entities receiving gravity
- ✅ InputReader ScriptableObject (decoupled input)
- ✅ CharacterMotorSpherical (spherical gravity movement)
- ✅ PlayerCamera (third-person with gravity alignment)
- ✅ PlayerInitializer (runtime dependency wiring)
- ✅ TestGravity scene configured with Player, Planet, Asteroid

### Next Up
- Test and refine movement feel
- Add debug visualization (gravity field gizmos)
- Tune camera collision and smoothing

---

## [0.2.0] - 2026-01-03

### Milestone 1: Core Gravity + On-foot Prototype - INITIAL IMPLEMENTATION

### Added
- **Core Interfaces** (`Scripts/Core/`)
  - `IGravitySource` - Contract for gravity-generating bodies
  - `IGravityAffected` - Contract for entities receiving gravity

- **Gravity System** (`Scripts/Gravity/`)
  - `GravityManager` - Singleton registry, priority-based source selection
  - `GravityBody` - MonoBehaviour for planets/asteroids with linear falloff
  - `GravitySolver` - Queries GravityManager and applies forces to Rigidbody

- **Player System** (`Scripts/Player/`)
  - `InputReader` - ScriptableObject decoupling input from consumers
  - `CharacterMotorSpherical` - Movement, jumping, ground detection on curved surfaces
  - `PlayerCamera` - Third-person orbit camera with gravity-aligned up vector
  - `PlayerInitializer` - Bootstrap component that wires dependencies at runtime

- **Scene Setup**
  - Planet_Test now has GravityBody (strength 15, range 200)
  - Player GameObject with capsule body, all components wired
  - Asteroid_Test sphere with lower priority GravityBody
  - Materials: M_Player (blue), M_Asteroid (gray)
  - InputReader and InputActionAsset in Resources folder

### Technical Notes
- Gravity uses linear falloff: `strength = baseStrength * (1 - distance/maxRange)`
- Priority determines which gravity source wins (higher = dominant)
- Distance used as tiebreaker when priorities are equal
- Camera aligns up vector smoothly to match player's gravity direction

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

---

## Session Log

### 2026-01-03 (Session 2)
- Implemented core gravity system (GravityManager, GravityBody, GravitySolver)
- Created player movement system (CharacterMotorSpherical, PlayerCamera, InputReader)
- Set up TestGravity scene with Player, Planet, and Asteroid
- Fixed InputReader to load InputActionAsset from Resources as fallback
- Created PlayerInitializer for runtime dependency injection
- **Milestone 1 initial implementation complete!**

### 2026-01-03 (Session 1)
- Project review and design doc enhancement
- Created folder structure and assembly definitions
- Added spec files for Gravity, Player, Ship systems
- Updated design doc with Code Conventions, AI Workflow sections
- Set up TestGravity scene with planet, lighting, post-processing
- Created Copilot instructions based on learnings
- **Milestone 0 complete!**

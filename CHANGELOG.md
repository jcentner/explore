# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Current Milestone
**Milestone 1: Core Gravity + On-foot Prototype**

### Next Up
- Implement GravityBody and GravitySolver
- CharacterMotorSpherical for spherical gravity walking
- Camera stability on curved surfaces

---

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

### 2026-01-03 (Session 1)
- Project review and design doc enhancement
- Created folder structure and assembly definitions
- Added spec files for Gravity, Player, Ship systems
- Updated design doc with Code Conventions, AI Workflow sections
- Set up TestGravity scene with planet, lighting, post-processing
- Created Copilot instructions based on learnings
- **Milestone 0 complete!**

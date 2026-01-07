# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Milestone 6: UI Foundation (In Progress)

**Phase 3: Pause Menu ✅**
- Added `Pause` action to Player and Ship input maps (Escape key, Gamepad Start)
- Wired `InputReader.OnPause` event via new `_pauseAction` and `_shipPauseAction` fields
- Created `IPauseHandler` interface in Core for decoupled pause handling
- `PlayerStateController` subscribes to `OnPause` and invokes `UIService<IPauseHandler>.Instance?.HandlePause()`
- `UIManager` implements `IPauseHandler` and registers with `UIService`
- Created `PauseScreen.cs` with time scale control, cursor unlock, button callbacks
- Created `PauseScreen.uxml` template with overlay, title, Resume/Settings/Quit buttons
- Created `PauseScreen.uss` stylesheet with `.btn--large` and `.btn--danger` styles
- UIManager auto-initializes `PauseScreen` from template instance
- **Manual step required:** Assign `MainUI.uxml` to UIDocument in Inspector to test

**Phase 2: Interaction Prompts ✅**
- Extended `IInteractionPrompt` interface with `Show(key, context)` and `Show(InteractionPromptData)` overloads
- Added `InteractionPromptData` struct for structured prompt data
- Created `InteractionPromptPanel.cs` implementing `IInteractionPrompt` for UI Toolkit
- Created `InteractionPrompt.uxml` template with key indicator and action text
- Created `InteractionPrompt.uss` stylesheet with fade transitions
- Updated `MainUI.uxml` to include InteractionPrompt template instance
- Updated `UIManager` to auto-initialize `InteractionPromptPanel` from template
- Deleted old UGUI-based `BoardingPrompt.cs` and removed from scene

**Phase 1: Foundation ✅**
- Created `UIService<T>` generic service locator in `Game.Core`
- Migrated `ShipBoardingTrigger` and `BoardingPrompt` to use `UIService<IInteractionPrompt>`
- Removed old `InteractionPromptService` from `IInteractionPrompt.cs`
- Created `UIManager` singleton with screen stack, panel registry, pause management
- Created `UIScreen` and `UIPanel` abstract base classes for UI Toolkit
- Created `Core.uss` stylesheet with CSS variables, typography, button styles
- Created `MainUI.uxml` root document with Panels/Screens containers
- Added UI GameObject to scene with UIDocument + UIManager components

**Phase 0: Cleanup & Preparation ✅**
- Deleted orphaned `InteractionPromptUI.cs` (didn't implement `IInteractionPrompt`)
- Moved `GravityDebugPanel` from `Game.Gravity` to `Game.UI` assembly (fixed architecture violation)
- Created folder structure for UI Toolkit: `Scripts/UI/Panels/`, `Screens/`, `Base/`
- Created UI asset folders: `Assets/_Project/UI/Templates/`, `Styles/`
- Updated documentation: `ui-system.spec.md`, `ARCHITECTURE.md` for UI Toolkit architecture

---

### Milestone 5: Solar System Lighting ✅

- Custom unlit shaders for correct sun-facing illumination at any viewpoint
- `SolarSystemLightingManager` sets global shader properties (`_SunPosition`, `_SunColor`, etc.)
- `DistantShadowCaster` for cylindrical eclipse shadows (up to 8 casters)
- `DistantObjectSwitcher` for LOD switching between URP Lit and distant shaders
- `SH_DistantPlanet.shader` - Per-pixel soft terminator for planets/moons
- `SH_DistantObject.shader` - Phase angle mode for asteroids/small objects
- `DistantLighting.hlsl` - Shared HLSL include with lighting functions
- Test materials created and applied to celestial bodies

---

### Milestone 4: Camera Perspective Toggle ✅

- First-person / third-person camera toggle (V key)
- Smooth perspective transitions with model hiding (shadows preserved)
- Player helpless in zero-g (removed thrust controls, floats with physics only)

---

### Milestone 3: Advanced Gravity System ✅

- Multi-body gravity accumulation with inverse-square falloff
- Smooth orientation transitions (90°/s blending)
- Emergent Lagrange points (gravity cancellation zones)
- Gravity UI indicator + F3 debug panel
- Service locator pattern for decoupled architecture

---

### Milestone 6 Planning

- Created UI framework plan and specification
- Renumbered milestones (UI before Gate Transition)
- Updated plan for UI Toolkit architecture (UXML + USS instead of UGUI)

---

### Code Quality Improvements

- Decoupled Ship from UI assembly via service locator
- Replaced magic strings with Tags/Layers constants
- Fixed hardcoded input keys to use InputReader events

---

## [0.3.0] - 2026-01-04

### Milestone 2: Ship Prototype + Planet-to-Space Loop ✅

- 6DOF ship flight with physics (thrust, roll, brake, boost)
- Player state machine (OnFoot ↔ InShip with fade transitions)
- Boarding system with trigger zones and UI prompts
- Landing detection with ground stability

---

## [0.2.0] - 2026-01-03

### Milestone 1: Core Gravity + On-foot Prototype ✅

- Spherical gravity system (GravityManager, GravityBody, GravitySolver)
- On-foot controller with up-alignment (CharacterMotorSpherical)
- Camera follows player with gravity-aligned orientation
- Test scene with walkable planet and asteroid

---

## [0.1.0] - 2026-01-03

### Milestone 0: Tooling + URP Baseline ✅

- Project structure with assembly definitions
- URP setup with post-processing volume
- Test scene with planet sphere and directional lighting
- Documentation (design_doc, specs, copilot-instructions)



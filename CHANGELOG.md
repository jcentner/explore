# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Milestone 6 Planning: UI Foundation

#### Added
- **Milestone 6 Plan** (`plans/milestone-6.plan.md`)
  - Comprehensive UI framework architecture
  - UIManager, UIScreen, UIPanel base classes
  - UIService replacing InteractionPromptService
  - Pause menu and settings screen design
  - HUD panels (velocity, ship status, interaction prompts)
  - Five implementation phases with task breakdowns

- **UI System Specification** (`specs/ui-system.spec.md`)
  - Architecture and component hierarchy
  - Input integration with action maps
  - Visual standards (typography, colors, layout)
  - Performance guidelines
  - Testing scenarios

#### Changed
- **Renumbered milestones in design_doc.md**
  - New Milestone 6: UI Foundation
  - Gate Transition moved to Milestone 7
  - Vertical Slice moved to Milestone 8

---

### Code Quality & Architecture Improvements

#### Changed
- **Decoupled Ship from UI assembly** - Removed `Game.UI` dependency from `Game.Ship.asmdef`
  - Created `IInteractionPrompt` interface in `Explorer.Core`
  - Created `InteractionPromptService` static service locator
  - `BoardingPrompt` now implements `IInteractionPrompt` and auto-registers
  - `ShipBoardingTrigger` uses `InteractionPromptService` instead of direct `BoardingPrompt` reference

- **Fixed hardcoded F key** - `ShipBoardingTrigger` now routes through `InputReader` events
  - Uses `OnInteract` event (Player map) for boarding when in range
  - Uses `OnShipExit` event (Ship map) for disembarking when piloting
  - Supports future rebinding without code changes

- **Replaced magic strings with constants**
  - Created `Tags.cs` in `Explorer.Core` with `PLAYER`, `MAIN_CAMERA`, `GROUND`, `INTERACTABLE`
  - Created `Layers.cs` with layer indices and pre-computed layer masks
  - Updated `ShipBoardingTrigger` to use `Tags.PLAYER`

#### Added
- **Validation attributes on ShipController**
  - `[Range(0f, 2f)]` on `_gravityMultiplier` to prevent invalid values
  - `[Range(0.1f, 5f)]` on `_landedVelocityThreshold`

#### Fixed
- **Scene cleanup**
  - Added explicit `GravityManager` GameObject (was auto-creating at runtime)
  - Parented `ShipCamera` to `Ship_Prototype` for logical hierarchy

#### Documentation
- Updated `player-system.spec.md` - Marked `PlayerStateController` and related components as implemented
- Updated `ship-system.spec.md` - Added "Deferred Features" table, clarified implementation status

---

## [0.3.0] - 2026-01-04

### Milestone 2: Ship Prototype + Planet-to-Space Loop ✅

#### Added
- **Ship Flight System**
  - `ShipController.cs` - 6DOF physics flight with direct angular velocity control
  - `ShipInput.cs` - Bridges InputReader to ShipController, implements `IPilotable`
  - `ShipCamera.cs` - Follow camera with configurable offset and smoothing
  - Ship action map in InputSystem_Actions (WASD thrust, mouse look, Q/E roll, Space brake, Tab boost)

- **Player State Machine**
  - `PlayerState.cs` - Enum: OnFoot, BoardingShip, InShip, DisembarkingShip
  - `PlayerStateController.cs` - Central state controller with fade transitions
  - `IPilotable.cs` - Interface for pilotable vehicles (avoids circular deps)

- **Boarding System**
  - `ShipBoardingTrigger.cs` - SphereCollider trigger, F to board/disembark
  - `BoardingPrompt.cs` - Auto-creating UI singleton for interaction prompts
  - `InteractionPromptUI.cs` - Generic prompt component with fade animation
  - Safe exit positioning via ground raycast

- **Landing Detection**
  - `ShipController.IsLanded` - True when grounded AND velocity < 0.5 m/s
  - `OnLanded`/`OnTakeoff` events for future UI/audio hooks

- **Physics Materials**
  - `PM_ShipHull.physicMaterial` - Friction for landed ship stability

#### Technical Notes
- **Unity 6 Input System**: Must use `Keyboard.current.fKey.wasPressedThisFrame`, not old `Input.GetKeyDown`
- **InputActionAsset loading**: `.inputactions` files don't load as InputActionAsset via Resources; use `.json` TextAsset + `InputActionAsset.FromJson()`
- **Assembly dependencies**: Used `IPilotable` interface to avoid circular reference between Game.Player and Game.Ship
- **Ship rotation**: Direct angular velocity control (`rb.angularVelocity = desired`) is smoother than accumulated target rotation

#### Ship Controls
| Input | Action |
|-------|--------|
| WASD | Thrust (strafe/forward) |
| Ctrl/Shift | Vertical thrust |
| Mouse | Pitch/Yaw |
| Q/E | Roll |
| Space | Brake |
| Tab | Boost |
| F | Board/Disembark |

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



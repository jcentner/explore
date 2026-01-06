# Technical Architecture

This document covers the technical implementation details for the Explore game. For game design, vision, and scope, see [design_doc.md](design_doc.md).

## 1) Core Technical Decisions

### 1.1 Coordinate Strategy

* Single-system "float bubble" for MVP.
* Add **floating origin** if jitter becomes noticeable:
  * When player/ship exceeds threshold (e.g., 2–5 km), shift world so player returns near (0,0,0).
  * Shift all relevant objects consistently.

### 1.2 Gravity Model

> **Full details:** [specs/gravity-system.spec.md](specs/gravity-system.spec.md)

* Each gravitating body defines a field (direction toward center, inverse-square magnitude)
* Multi-body accumulation: all sources contribute simultaneously
* Dominant source selection for orientation (strongest magnitude wins)
* Zero-g emerges where gravity sources cancel out (below 0.25 m/s² threshold)

### 1.3 Controllers (Separate by Mode)

* **On-foot controller:** Spherical gravity + grounding (player helpless in zero-g)
* **Ship controller:** 6DOF physics flight, optional gravity response

> **Full details:** [specs/player-system.spec.md](specs/player-system.spec.md), [specs/ship-system.spec.md](specs/ship-system.spec.md)

### 1.4 Scene & Streaming Strategy

**Target Architecture (Post-MVP):**
* Scene `Core`: player, ship, UI, managers (persistent)
* Scene `System_A`: planets, props, POIs
* Scene `Gate_Destination_A`: destination environment
* Gate transition loads/unloads scenes asynchronously.

**Current (MVP):** Single `TestGravity.unity` scene contains all content. Multi-scene streaming deferred until Gate Transition milestone.

### 1.5 Gate Transition Strategy (Stylized Load)

Not a true portal:

1. Trigger → lock input
2. VFX/audio + fade/warp
3. Async load target
4. Teleport to exit anchor
5. Fade in → restore input

---

## 2) URP Production Setup

### Rendering Baseline

* Use the URP Renderer with:
  * **Opaque Texture** only if needed (some effects require it)
  * **Depth Texture** only if needed (fog/edge effects)
* Start with single main directional light + limited additional lights
* Prefer baked lighting for dense POIs/interiors once layout stabilizes

### Post-Processing Baseline

* Global Volume:
  * Tonemapping + color adjustments (style)
  * Bloom (for emissives/gates)
  * Vignette (subtle)
  * Optional film grain (subtle)
* Keep the effect stack minimal until the slice is fun.

### Materials/Shaders

* Standard URP Lit for most things initially (ship/props)
* Shader Graph only for:
  * Atmospheres (planet edge glow)
  * Rim/fresnel stylization
  * Gate VFX shaders
  * Decals/POI highlights (if needed)

### Performance Guardrails

* LODs for planets/large meshes
* Distance-based disabling for small props
* Avoid expensive real-time shadows for lots of lights
* Keep particle counts sane (especially during gate VFX)

---

## 3) Assembly Definitions

Use Assembly Definitions (`.asmdef`) to:
* **Speed up iteration** – Only recompile changed assemblies
* **Enforce architecture** – Prevent circular dependencies
* **Cleaner AI context** – Smaller, focused compilation units

### Assembly Structure

```
Game.Core.asmdef          → Core/, interfaces, utilities, constants (Tags, Layers)
    ↑
Game.Gravity.asmdef       → Gravity/ (depends on Core)
    ↑
Game.Player.asmdef        → Player/ (depends on Core, Gravity, InputSystem)
    ↑
Game.Ship.asmdef          → Ship/ (depends on Core, Gravity, Player, InputSystem)

Game.Gates.asmdef         → Gates/ (depends on Core, Gravity)
Game.Interaction.asmdef   → Interaction/ (depends on Core, InputSystem)
Game.Save.asmdef          → Save/ (depends on Core)

Game.UI.asmdef            → UI/ (depends on Core, Gravity) — decoupled from gameplay systems
```

**Rationale:** Separate assemblies provide better modularity. Each system can evolve independently.

---

## 4) Input System Strategy

Using Unity's **new Input System** (package already installed).

### Action Maps

| Action Map | Actions | Notes |
|------------|---------|-------|
| **Player** | Move, Look, Jump, Interact | On-foot controls |
| **Ship** | Thrust, Vertical, Look, Roll, Brake, Boost, Exit | Flight controls |
| **UI** | Navigate, Submit, Cancel, Pause | Menu navigation |

### Implementation Pattern

* `InputReader.cs` ScriptableObject decouples input from consumers
* Events-based: `OnJump`, `OnInteract`, `OnShipExit`
* Switch Action Maps on mode change (on-foot ↔ ship ↔ UI)
* Support rebinding via Input System's built-in rebind UI

### Control Scheme

* **Keyboard + Mouse** (primary)
* **Gamepad** (secondary, test periodically)

---

## 5) System Architecture

### 5.1 Implemented Components

| System | Components | Spec |
|--------|------------|------|
| **Gravity** | `GravityBody`, `GravityManager`, `GravitySolver` | [gravity-system.spec.md](specs/gravity-system.spec.md) |
| **Player** | `CharacterMotorSpherical`, `PlayerCamera`, `PlayerStateController`, `InputReader` | [player-system.spec.md](specs/player-system.spec.md) |
| **Ship** | `ShipController`, `ShipInput`, `ShipCamera`, `ShipBoardingTrigger` | [specs/ship-system.spec.md](specs/ship-system.spec.md) |
| **UI** | `BoardingPrompt`, `GravityIndicatorPanel`, `GravityDebugPanel` | [ui-system.spec.md](specs/ui-system.spec.md) |

### 5.2 Deferred Components

| Component | Purpose | Target |
|-----------|---------|--------|
| `FloatingOriginManager` | Precision at large distances | If needed |
| `GateController` | Gate transition orchestration | Milestone 7 |
| `InteractionSystem` | Raycast + prompt system | Milestone 6 |
| `SaveSystem` | Progression state persistence | Milestone 8 |

### 5.3 Core Utilities (Explorer.Core)

**Namespace convention:** All assemblies use `Explorer.[System]` (e.g., `Explorer.Gravity`, `Explorer.Player`).

**Constants (in `Tags.cs`):**
* `Tags` – String constants for Unity tags (`Tags.PLAYER`, `Tags.GROUND`)
* `Layers` – Layer indices and pre-computed masks (`Layers.GROUND_MASK`)

**Service Locators:**
* `InteractionPromptService` – UI prompts (gameplay calls `Show()`/`Hide()` without UI dependency)
* `PlayerPilotingService` – Player piloting state queries (decouples from Player assembly)

**Interfaces:**
* `IInteractionPrompt` – Implemented by `BoardingPrompt` in UI
* `IPlayerPilotingState` – Implemented by `PlayerStateController` in Player
* `IGravitySource` – Implemented by `GravityBody` in Gravity
* `IGravityAffected` – Implemented by `GravitySolver` in Gravity

### 5.4 Gravity Formula

**Inverse-square falloff:**
```
g = surfaceGravity × (surfaceRadius² / distance²)
```

**Parameters per GravitySource:**
* `surfaceGravity` (m/s²) – gravity at surface (Earth ≈ 9.8, Moon ≈ 1.6)
* `surfaceRadius` (m) – radius of the body's surface
* `minGravityThreshold` (m/s²) – below this, gravity treated as zero (0.25 default)

**Zero-G Detection:** When accumulated gravity magnitude falls below threshold, entities enter zero-g state. This creates emergent Lagrange-like points where gravity sources cancel out.

### 5.5 Visual Systems (URP)

* `PostProcessProfile` (global volume asset)
* `GateVFX` (VFX Graph or Particle System + Shader Graph) — deferred
* `PlanetAtmosphere` (Shader Graph material + parameters) — deferred
* `LODProfiles` (per major asset type) — deferred

---

## 6) AI Development Workflow

### Key Context Files

| File | Purpose |
|------|---------|
| `design_doc.md` | Vision, scope, milestones |
| `ARCHITECTURE.md` | Technical implementation (this file) |
| `CHANGELOG.md` | Current progress, what's implemented |
| `specs/*.spec.md` | Per-system specifications |
| `plans/milestone-X.plan.md` | Detailed task breakdowns |

### Spec Sheet Pattern

For each major system, maintain a spec file with:
- Purpose and design goals
- Interfaces and enums
- Component details (inspector fields, runtime behavior)
- Behaviors and edge cases
- File locations

### Session Workflow

1. Read `CHANGELOG.md` for current state
2. Read `plans/milestone-X.plan.md` for active milestone
3. Reference `specs/*.spec.md` for system details
4. Make changes incrementally, validate each step
5. Update `CHANGELOG.md` with progress

### Prompting Tips

* **Reference spec files** when asking for implementation
* **Include file paths** when discussing specific code
* **State the current milestone** for scope context
* **Ask for one system at a time** – avoid sprawling requests

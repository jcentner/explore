# Project Plan: Scaled Solar-System Exploration Game (Unity + URP)

## 1) Vision

A 3D exploration game set in a compact, handcrafted star system. Players can:

* Walk on round planets with **dynamic gravity** (local “down” toward planet center).
* Fly a ship between bodies within the system.
* Use a **stargate-like gate** to transition to other locations (scene-streamed).

Primary inspiration: *Outer Wilds* feel (learnable systemic physics + exploration), *Stargate* flavor (gate travel + mystery).

## 2) Design Pillars

1. **Consistent, learnable physics (not realistic physics)**
2. **Fast iteration**
3. **Finishable scope**
4. **Exploration-first**
5. **Stylized visuals, readable silhouettes**

## 3) Engine & Rendering Decisions

### Engine

* **Unity** 6000.3.2f1

### Render Pipeline

* **URP (Universal Render Pipeline)** for:

  * simpler production path for a first game
  * broader performance headroom
  * strong stylized rendering support via Shader Graph + post-processing

**Constraint:** Avoid pipeline switching later (treat URP as fixed).

## 4) Visual Target (URP Stylized)

### Look goals

* “Readable, cinematic stylized sci-fi” rather than realism
* Strong shape language, emissive accents, controlled palette
* Planets feel large through scale cues and atmosphere, not ultra-detail

### Key visual ingredients

* **Post-processing:** color grading + bloom + subtle vignette (style cohesion)
* **Atmosphere:** fog/height fog style effect (can be faked with shaders/volumes), simple atmospheric scattering cues
* **Rim lighting / fresnel:** for silhouettes on characters/props and planet edges
* **Emissives:** gates, ship panels, POIs for navigation readability
* **LOD + culling:** keep far objects cheap (space scenes balloon fast)

### Non-goals (early)

* Realistic volumetrics, path-traced lighting, ultra-high fidelity materials
* Complex portal rendering (no see-through gates in slice)

## 5) Initial Scope

### Included (MVP / Vertical Slice)

* One star system (single “play bubble” coordinate space)
* 1 planet (walkable) + 1 moon/asteroid (low grav)
* Ship flight between bodies
* One gate that transitions to a destination area (separate scene)
* Basic interactions (scan / collect / trigger)
* Basic UI (prompt, objective, minimal HUD)
* Save/load for player progression state (simple)
* URP-based stylized lighting + post baseline pass

### Explicitly Not Included (for now)

* Seamless travel between star systems (later via “hyperdrive” transition)
* True see-through portals
* Real-time orbital simulation
* Large procedural galaxy generation
* Complex NPC AI / combat systems
* Multiplayer

## 6) Scale Targets (Game Scale)

* **System radius:** ~5–20 km (Unity units as meters)
* **Planet radius:** ~200–1500 m
* **Moon radius:** ~50–400 m
* Orbits: start static; motion later

## 7) Core Technical Decisions

### 7.1 Coordinate Strategy

* Single-system “float bubble” for MVP.
* Add **floating origin** if jitter becomes noticeable:

  * When player/ship exceeds threshold (e.g., 2–5 km), shift world so player returns near (0,0,0).
  * Shift all relevant objects consistently.

### 7.2 Gravity Model

* Each gravitating body defines a field:

  * Direction: toward body center
  * Magnitude: curve by distance (tunable)
* Choose dominant body per entity at runtime.
* Ship gravity can be always-on or proximity-based (pick what feels best).

### 7.3 Controllers (Separate by Mode)

* On-foot controller: spherical gravity + grounding + step/slope handling
* Ship controller: flight model (arcade-leaning), optional auto-stabilize

### 7.4 Scene & Streaming Strategy

* Scene `Core`: player, ship, UI, managers (persistent)
* Scene `System_A`: planets, props, POIs
* Scene `Gate_Destination_A`: destination environment
* Gate transition loads/unloads scenes asynchronously.

### 7.5 Gate Transition Strategy (Stylized Load)

Not a true portal:

1. Trigger → lock input
2. VFX/audio + fade/warp
3. Async load target
4. Teleport to exit anchor
5. Fade in → restore input

## 8) URP Production Setup (baseline checklist)

This is the “don’t shoot yourself in the foot” setup for a first stylized 3D project.

### Rendering baseline

* Use the URP Renderer with:

  * **Opaque Texture** only if needed (some effects require it; otherwise keep off)
  * **Depth Texture** only if needed (fog/edge effects; otherwise keep off)
* Start with a single main directional light + limited additional lights (budget discipline)
* Prefer baked lighting for dense POIs/interiors once the layout stabilizes

### Post-processing baseline

* Global Volume:

  * Tonemapping + color adjustments (style)
  * Bloom (for emissives/gates)
  * Vignette (subtle)
  * Optional film grain (subtle)
* Keep the effect stack minimal until the slice is fun.

### Materials/shaders

* Standard URP Lit for most things initially (ship/props)
* Shader Graph only for:

  * atmospheres (planet edge glow)
  * rim/fresnel stylization
  * gate VFX shaders
  * decals/POI highlights (if needed)

### Performance guardrails (early)

* LODs for planets/large meshes
* Distance-based disabling for small props
* Avoid expensive real-time shadows for lots of lights
* Keep particle counts sane (especially during gate VFX)

## 9) Folder Structure

```
Assets/
├── _Project/                 # All game-specific content (underscore sorts to top)
│   ├── Scripts/
│   │   ├── Core/             # Interfaces, constants (Tags, Layers), service locators
│   │   ├── Gravity/          # GravityBody, GravitySolver, IGravityAffected
│   │   ├── Player/           # CharacterMotorSpherical, camera, states
│   │   ├── Ship/             # ShipController, boarding, flight model
│   │   ├── Gates/            # GateController, transition orchestration
│   │   ├── Interaction/      # InteractionSystem, prompts, collectibles
│   │   ├── Save/             # SaveSystem, serialization
│   │   └── UI/               # HUD, menus, prompts
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Ship/
│   │   ├── Environment/
│   │   └── UI/
│   ├── Materials/
│   ├── Shaders/              # Shader Graph assets
│   ├── VFX/                  # VFX Graph + Particle Systems
│   ├── Audio/
│   │   ├── SFX/
│   │   └── Music/
│   ├── Textures/
│   ├── Models/
│   ├── Animations/
│   └── Scenes/
│       ├── Core.unity        # Persistent (player, ship, UI, managers)
│       ├── System_A.unity    # Main star system
│       └── Gate_Dest_A.unity # Gate destination
├── Settings/                 # URP assets, post-processing volumes, input actions
└── ThirdParty/               # Imported packages, keep separate for upgrades
```

**Conventions:**
* Prefix project folder with `_` to sort to top in Unity
* One prefab per logical entity
* Materials named: `M_[Object]_[Variant]` (e.g., `M_Planet_Rocky`)
* Shaders named: `SH_[Purpose]` (e.g., `SH_Atmosphere`)

## 10) Assembly Definitions

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
Game.Player.asmdef        → Player/ (depends on Core, Gravity)
    ↑
Game.Ship.asmdef          → Ship/ (depends on Core, Gravity, Player)
    ↑
Game.World.asmdef         → Gates/, Interaction/, Save/ (depends on Core, Gravity)

Game.UI.asmdef            → UI/ (depends on Core) — decoupled from gameplay systems
```

**Setup:** Create `.asmdef` files in each Scripts subfolder. Reference dependencies explicitly.

## 11) Input System Strategy

Using Unity's **new Input System** (package already installed).

### Action Maps

| Action Map | Actions | Notes |
|------------|---------|-------|
| **Player** | Move, Look, Jump, Interact, Board | On-foot controls |
| **Ship** | Thrust, Pitch, Yaw, Roll, Stabilize, Exit | Flight controls |
| **UI** | Navigate, Submit, Cancel, Pause | Menu navigation |

### Implementation Pattern

* Use `PlayerInput` component with **Invoke C# Events** (not SendMessage)
* Create `InputReader.cs` ScriptableObject to decouple input from consumers
* Switch Action Maps on mode change (on-foot ↔ ship ↔ UI)
* Support rebinding via Input System's built-in rebind UI

### Control Scheme

* **Keyboard + Mouse** (primary)
* **Gamepad** (secondary, test periodically)

## 12) Proposed Architecture

### 12.1 Gameplay Systems (Components)

* `GravitySource` (surfaceRadius, surfaceGravity)
* `GravityManager` (registry, accumulated gravity queries)
* `GravitySolver` (applies gravity, zero-g detection)
* `CharacterMotorSpherical`
* `ShipController`
* `FloatingOriginManager` (optional for MVP)
* `GateController` (transition orchestration)
* `InteractionSystem` (raycast + prompt)
* `SaveSystem` (simple flags/state)

### 12.2 Gravity Formula

**Implemented approach: Inverse-square falloff**

```
g = surfaceGravity × (surfaceRadius² / distance²)
```

**Why inverse-square:**
* Physically accurate behavior - feels natural
* Natural multi-body interaction - gravity sources combine realistically
* Emergent Lagrange points where gravity cancels out

**Parameters per GravitySource:**
* `surfaceGravity` (m/s²) – gravity at surface (Earth ≈ 9.8, Moon ≈ 1.6)
* `surfaceRadius` (m) – radius of the body's surface
* `minGravityThreshold` (m/s²) – below this, gravity treated as zero (0.25 default)

**Zero-G Detection:** When accumulated gravity magnitude falls below threshold, entities enter zero-g state. This creates emergent Lagrange-like points where gravity sources cancel out.

### 12.3 Visual Systems (URP)

* `PostProcessProfile` (global volume asset)
* `GateVFX` (VFX Graph or Particle System + Shader Graph)
* `PlanetAtmosphere` (Shader Graph material + parameters)
* `LODProfiles` (per major asset type)

### 12.4 Core Utilities

Centralized in `Explorer.Core` to enable decoupled architecture:

* **`Tags`** – String constants for Unity tags (`Tags.PLAYER`, `Tags.GROUND`)
* **`Layers`** – Layer indices and pre-computed masks (`Layers.GROUND_MASK`)
* **`InteractionPromptService`** – Service locator for UI prompts (gameplay systems call `Show()`/`Hide()` without UI dependency)
* **`IInteractionPrompt`** – Interface implemented by UI systems
* **`PlayerPilotingService`** – Service locator for player piloting state (gravity/UI systems query without Player assembly dependency)
* **`IPlayerPilotingState`** – Interface implemented by `PlayerStateController`

## 13) Code Conventions

### Naming

| Element | Convention | Example |
|---------|------------|--------|
| Classes | PascalCase | `GravityBody` |
| Interfaces | I + PascalCase | `IGravityAffected` |
| Public methods | PascalCase | `ApplyGravity()` |
| Private fields | _camelCase | `_currentVelocity` |
| Serialized private | camelCase + attribute | `[SerializeField] float fallSpeed` |
| Constants | UPPER_SNAKE | `MAX_GRAVITY_SOURCES` |
| Tags | UPPER_SNAKE in Tags class | `Tags.PLAYER` |
| Events | On + PascalCase | `OnLanded`, `OnBoardedShip` |

### Structure

```csharp
public class ExampleComponent : MonoBehaviour
{
    // === Inspector Fields ===
    [Header("Configuration")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform target;
    
    // === Public Properties ===
    public bool IsActive => _isActive;
    
    // === Private Fields ===
    private bool _isActive;
    private Rigidbody _rb;
    
    // === Unity Lifecycle ===
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update() { }
    private void FixedUpdate() { }
    
    // === Public Methods ===
    public void Activate() { }
    
    // === Private Methods ===
    private void HandleMovement() { }
}
```

### Best Practices

* **Cache components** in `Awake()`, never in `Update()`
* **Use `[SerializeField] private`** over `public` fields
* **Remove empty callbacks** (`Update()`, `FixedUpdate()`) – they have overhead
* **Avoid `Find*()` at runtime** – use direct references or ServiceLocator
* **One class per file**, filename matches class name
* **Use `[Header]` and `[Tooltip]`** for inspector clarity
* **XML docs on public APIs** for Copilot context

### Unity-Specific

* Prefer **Sphere > Capsule > Box >> Mesh** colliders (performance)
* Use **object pooling** for frequently spawned objects (VFX, projectiles)
* Set **Asset Serialization to Force Text** (Edit > Project Settings > Editor) for Git
* **Never use `.material`** in loops – cache or use `.sharedMaterial`

## 14) AI Development Workflow

### Key Context Files

Maintain these files to give Copilot effective context:

| File | Purpose |
|------|--------|
| `design_doc.md` | Vision, scope, architecture (this file) |
| `CHANGELOG.md` | Track what's implemented, current status |
| `Assets/_Project/Scripts/README.md` | Code conventions quick reference |
| `specs/*.spec.md` | Per-system specifications |

### Spec Sheet Pattern

For each major system, maintain a spec file:

```markdown
# gravity-system.spec.md

## Purpose
Manage gravitational attraction toward celestial bodies.

## Interfaces
- IGravityAffected: Entities that respond to gravity
- IGravitySource: Bodies that generate gravity fields

## Components
- GravitySource: Defines a gravity source (surfaceRadius, surfaceGravity)
- GravityManager: Registry of sources, provides accumulated gravity queries
- GravitySolver: Applies gravity to entities, handles zero-g detection

## Behaviors
- Entity queries accumulated gravity from all sources (inverse-square)
- Smooth orientation blending toward dominant source (90°/s)
- Gravity below threshold (0.25 m/s²) clamped to zero

## Edge Cases
- Exactly equidistant from two bodies → emergent zero-g (Lagrange point)
- Zero-g zones → emergent where sources cancel, not explicitly defined
```

### Prompting Tips

* **Reference spec files** when asking for implementation
* **Include file paths** when discussing specific code
* **State the current milestone** for scope context
* **Ask for one system at a time** – avoid sprawling requests

### Session Workflow

1. Start session: Share `design_doc.md` + relevant spec
2. State current milestone and goal
3. Implement incrementally, test each piece
4. Update `CHANGELOG.md` with progress
5. End session: Note what's next in `## 15) Next Actions`

## 15) Milestones & Deliverables

### Milestone 0: Tooling + URP Baseline (1–2 sessions)

* Unity project set to **URP**
* Folder structure + conventions
* Global post volume with a placeholder stylized grade
* Test scene with planet sphere + lighting sanity

**Done when:** URP visuals are stable and consistent; project runs clean.

---

### Milestone 1: Core Gravity + On-foot Prototype ✅ COMPLETE

* ✅ Walkable planet sphere with local gravity
* ✅ Camera stability and "up" alignment
* ✅ Gravity switching between bodies (distance-based)
* ✅ Test asteroid for multi-body gravity testing

---

### Milestone 2: Ship Prototype + Planet-to-Space Loop ✅ COMPLETE

* ✅ Board/unboard ship (F key, fade transitions)
* ✅ Fly to moon/asteroid, land, walk
* ✅ 6DOF ship flight with physics
* ✅ Player state machine (OnFoot ↔ InShip)

---

### Milestone 3: Advanced Gravity System ✅ COMPLETE

* ✅ Multi-body gravity accumulation (inverse-square falloff)
* ✅ `GravityManager.GetAccumulatedGravity()` - Sum of all gravity sources
* ✅ Smooth orientation blending (90°/s rotation toward dominant source)
* ✅ Emergent Lagrange points (gravity < 0.25 m/s² clamped to zero)
* ✅ Gravity UI indicator (directional arrow + ZERO-G text)
* ✅ F3 debug panel (position, velocity, gravity info, contributors)
* ✅ Player zero-g thrust movement (WASD + Shift/Ctrl)
* ✅ Service locator pattern (`PlayerPilotingService`) for decoupled architecture

---

### Milestone 4: Enhanced Camera & Movement Controls

* First-person / third-person camera toggle (V key)
* Airborne roll control (Q/E) - reorient when not grounded
* Jetpack system (J to toggle):
  - Vertical thrust: Shift (up) / Ctrl (down) - matches ship controls
  - Horizontal thrust: WASD
  - 6DOF movement in zero-g or low-g environments
* Fuel system with grounded regeneration

---

### Milestone 5: Solar System Lighting

* **Problem:** Single directional light doesn't work for solar-system scale (planets on far side of sun lit incorrectly)
* **Solution:** Custom unlit shaders that calculate sun-facing illumination mathematically
* `SolarSystemLightingManager` - Sets global shader properties (_SunPosition, _SunColor, shadow caster data)
* `DistantShadowCaster` - Registers planets/moons as shadow-casting bodies
* `DistantObjectSwitcher` - LOD switch between real-lit (near) and distant-shader (far) versions
* Shader include `DistantLighting.hlsl` with:
  - `CalculateSoftTerminator()` - Per-pixel day/night boundary
  - `CalculatePhaseAngle()` - Overall brightness for small objects
  - `CalculateShadow()` - Cylindrical shadow from registered casters
* Shaders: `SH_DistantPlanet`, `SH_DistantObject`, `SH_CometTail`
* Comet tails always point away from sun

---

### Milestone 6: UI Foundation

* **Problem:** Current UI is ad-hoc (auto-creating singletons, no consistent architecture)
* **Solution:** Proper UI framework with clear patterns and reusable components
* `UIManager` - Central UI controller, manages screen stack and transitions
* `UIScreen` - Base class for full-screen UI (pause menu, inventory, map)
* `UIPanel` - Base class for HUD elements (health, prompts, indicators)
* `UIService` - Service locator replacing ad-hoc singletons
* HUD System:
  - Interaction prompts (generalized from BoardingPrompt)
  - Velocity/altitude indicator
  - Gravity direction indicator (for Milestone 3)
  - Ship status panel (when piloting)
* Screen System:
  - Pause menu with resume/settings/quit
  - Settings screen (volume, controls, graphics)
* Input integration: UI action map, proper focus management
* Prefab-based UI with consistent styling

---

### Milestone 7: Gate Transition (Styled Loading)

* Gate trigger + VFX + async scene load + exit anchors

---

### Milestone 8: Vertical Slice Content + Style Pass

* 2–3 POIs on planet, 1 on moon/asteroid
* First real stylized materials + atmosphere + emissive navigation cues

## 16) Risks & Mitigations

* **Overbuilding tech:** ship a slice first; defer fancy portal rendering
* **Spherical controller complexity:** prototype rough → iterate on feel
* **Precision/jitter:** keep system compact; add floating origin only if needed
* **Content bottleneck:** build reusable POI templates; keep art scope controlled
* **URP performance traps:** minimal post stack, cautious shadows, early LODs

## 17) Definition of Done (Vertical Slice)

* Stable on-foot spherical gravity on one planet
* Stable ship travel between two bodies
* Realistic multi-body gravity with smooth transitions
* First/third person camera with airborne rotation control
* Jetpack for 6DOF player movement
* Solar system lighting with correct illumination from any viewpoint
* Reliable gate transition to a second scene
* Objective can be completed end-to-end (15–30 minutes)
* Minimal save state persists key flags
* Stylized URP look established (post + emissives + atmosphere cues)
* Build runs outside editor

## 18) Documentation & Workflow

See `plans/milestone-X.plan.md` for detailed task breakdowns and session logs.

See `.github/copilot-instructions.md` for AI assistant workflow and current state.
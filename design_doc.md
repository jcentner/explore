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
│   │   ├── Core/             # Singletons, managers, base classes, interfaces
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
Game.Core.asmdef          → Core/, interfaces, utilities
    ↑
Game.Gravity.asmdef       → Gravity/ (depends on Core)
    ↑
Game.Player.asmdef        → Player/, Ship/ (depends on Core, Gravity)
    ↑
Game.World.asmdef         → Gates/, Interaction/, Save/ (depends on Core, Gravity)
    ↑
Game.UI.asmdef            → UI/ (depends on Core)
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

### 12.1 Gameplay Systems (Suggested Components)

* `GravityBody` (radius, curve, priority)
* `GravitySolver` (dominant body)
* `CharacterMotorSpherical`
* `ShipController`
* `FloatingOriginManager` (optional for MVP)
* `GateController` (transition orchestration)
* `InteractionSystem` (raycast + prompt)
* `SaveSystem` (simple flags/state)

### 12.2 Gravity Formula

**Recommended approach: Linear falloff with hard cutoff**

```
gravityStrength = baseStrength * (1 - (distance / maxRange))
if (distance > maxRange) gravityStrength = 0
```

**Why linear over inverse-square:**
* Predictable, tunable gameplay feel
* No infinite gravity at surface edge cases
* Easier to balance planet "pull zones"

**Parameters per GravityBody:**
* `baseStrength` (m/s²) – gravity at surface (Earth ≈ 9.8, Moon ≈ 1.6)
* `maxRange` (m) – distance where gravity reaches zero
* `priority` (int) – tie-breaker when in overlapping fields

**Alternative (arcade feel):** Constant gravity within radius, instant zero outside. Simpler but less immersive.

### 12.3 Visual Systems (URP)

* `PostProcessProfile` (global volume asset)
* `GateVFX` (VFX Graph or Particle System + Shader Graph)
* `PlanetAtmosphere` (Shader Graph material + parameters)
* `LODProfiles` (per major asset type)

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
- GravityBody: Defines a gravity source (radius, strength, priority)
- GravitySolver: Calculates dominant gravity for an entity

## Behaviors
- Entity enters gravity field → GravitySolver recalculates
- Multiple overlapping fields → highest priority wins
- Ship can toggle gravity response

## Edge Cases
- Exactly equidistant from two bodies → use priority
- Zero-g zones → explicitly defined, not emergent
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

### Milestone 1: Core Gravity + On-foot Prototype

* Walkable planet sphere with local gravity
* Camera stability and “up” alignment

---

### Milestone 2: Ship Prototype + Planet-to-Space Loop

* Board/unboard ship
* Fly to moon/asteroid, land, walk

---

### Milestone 3: Gate Transition (Styled Loading)

* Gate trigger + VFX + async scene load + exit anchors

---

### Milestone 4: Vertical Slice Content + Style Pass

* 2–3 POIs on planet, 1 on moon/asteroid
* Simple objective loop
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
* Reliable gate transition to a second scene
* Objective can be completed end-to-end (15–30 minutes)
* Minimal save state persists key flags
* Stylized URP look established (post + emissives + atmosphere cues)
* Build runs outside editor

## 18) Next Actions

* [ ] Create folder structure in Unity project
* [ ] Set up Assembly Definitions
* [ ] Configure Input Actions asset with action maps
* [ ] Create initial spec files for Gravity, Player, Ship systems
* [ ] Set Asset Serialization to Force Text
* [ ] Create test scene with planet sphere + directional light
* [ ] Implement GravityBody + GravitySolver (Milestone 1 start)
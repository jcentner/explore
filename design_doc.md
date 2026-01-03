# Project Plan: Scaled Solar-System Exploration Game (Unity3D)

## 1) Vision

A 3D exploration game set in a compact, handcrafted star system. Players can:

* Walk on round planets with **dynamic gravity** (local “down” to planet center).
* Fly a ship between bodies within the system.
* Use a **stargate-like gate** to transition to other locations (scene-streamed).

Primary inspiration: *Outer Wilds* feel (learnable systemic physics + exploration), *Stargate* flavor (gate travel + mystery).

## 2) Design Pillars

1. **Consistent, learnable physics (not realistic physics)**

   * Gravity, movement, and ship handling should be predictable and tunable.
2. **Fast iteration**

   * Content and mechanics should be testable quickly in-editor.
3. **Finishable scope**

   * Gate travel is a stylized transition, not a seamless portal.
4. **Exploration-first**

   * Minimal combat or complex AI early.

## 3) Initial Scope

### Included (MVP / Vertical Slice)

* One star system (single “play bubble” coordinate space)
* 1 planet (walkable) + 1 moon/asteroid (low grav)
* Ship flight between bodies
* One gate that transitions to a destination area (separate scene)
* Basic interactions (scan / collect / trigger)
* Basic UI (prompt, objective, minimal HUD)
* Save/load for player progression state (simple)

### Explicitly Not Included (for now)

* Seamless travel between star systems (use “hyperdrive” transition later)
* True see-through portals
* Real-time orbital simulation
* Large procedural galaxy generation
* Complex NPC AI / combat systems
* Multiplayer / networking

## 4) Scale Targets (Game Scale)

Use “game scale,” not astronomical scale.

* **System radius:** ~5–20 km (Unity units as meters)
* **Planet radius:** ~200–1500 m
* **Moon radius:** ~50–400 m
* Orbits: start static; fake motion later only if needed for feel

Goal: bodies feel big enough to explore, small enough to keep physics stable and traversal times reasonable.

## 5) Engine Choice Rationale

### Recommended: Unity

* Strong 3D workflow + ecosystem
* Mature approaches for additive scenes / streaming
* Easier path to completion for a first game of this type

Godot remains viable, but Unity reduces the odds of getting stuck on engine-level complexity.

## 6) Core Technical Decisions

### 6.1 Coordinate Strategy

* **Single-system “float bubble”** for MVP.
* Add **floating origin** only if jitter becomes noticeable:

  * When player/ship exceeds a threshold (e.g., 2–5 km from origin), shift world so player returns near (0,0,0).
  * Shift all relevant objects consistently.
  * Store “absolute” positions separately if needed (optional for MVP).

### 6.2 Gravity Model

* Each gravitating body defines a field:

  * Direction: toward body center
  * Magnitude: curve by distance (tunable)
  * Optional “surface band” where gravity is approximately constant
* At runtime:

  * Determine **dominant gravity body** for the player (highest influence).
  * Align player “up” opposite gravity direction.
* Ship:

  * Apply gravity always, or only near bodies (choose whichever feels better).

### 6.3 Controllers (Separate by Mode)

* **On-foot controller**: spherical gravity + grounding + step/slope handling
* **Ship controller**: flight model (arcade-leaning), optional auto-stabilize
* Do not try to unify these into one movement system early.

### 6.4 Scene & Streaming Strategy

Start simple, evolve as needed:

* Scene `Core`: player, ship, UI, managers (persistent)
* Scene `System_A`: planets, props, POIs
* Scene `Gate_Destination_A`: destination environment
* Transition system loads/unloads scenes asynchronously.

### 6.5 Gate Transition Strategy (Stylized Load)

Not a true portal.

1. Player enters gate trigger
2. Lock input + play VFX/audio
3. Fade/warp tunnel effect
4. Async load target scene
5. Teleport to exit anchor
6. Fade in + restore input

## 7) Proposed Architecture

### 7.1 Gameplay Systems (Suggested Components)

* `GravityBody`

  * radius, gravity curve, priority, atmosphere band (optional)
* `GravitySolver`

  * finds dominant body for an entity each frame / fixed step
* `CharacterMotorSpherical`

  * movement, grounding, alignment to local-up
* `ShipController`

  * thrust, rotation, stabilization modes
* `FloatingOriginManager` (optional for MVP)

  * monitors player distance and shifts world
* `GateController`

  * handles triggers, VFX timing, scene transitions, spawn anchors
* `InteractionSystem`

  * raycast + prompts + interact events
* `SaveSystem`

  * minimal state: current scene, spawn point, collected items, flags

### 7.2 Data Model

Use ScriptableObjects for tunables:

* Gravity presets (planet, moon, asteroid)
* Ship handling presets
* Gate destination definitions (scene name, exit anchor id, VFX profile)

## 8) Milestones & Deliverables

### Milestone 0: Tooling & Repo Setup (1 session)

* Unity project + version control
* Simple build pipeline (local)
* Coding conventions + folder structure
* Minimal CI optional (build checks later)

**Done when:** project opens cleanly, basic scene runs, repo organized.

---

### Milestone 1: Core Gravity & On-foot Prototype

* One sphere “planet”
* Player can walk around entire surface
* Camera stays stable, horizon behaves
* Jumping + falling works

**Done when:** player can circumnavigate without glitches, grounding feels acceptable.

---

### Milestone 2: Ship Prototype + Planet-to-Space Loop

* Ship spawns on/near planet
* Player can board/unboard
* Fly to moon/asteroid
* Land and walk there (even if landing is forgiving)

**Done when:** a complete loop works: walk → board → fly → land → walk.

---

### Milestone 3: Gate Transition (Styled Loading)

* Gate ring + trigger
* VFX + fade sequence
* Destination scene loads and places player at exit gate
* Return gate (optional)

**Done when:** gate travel feels like “Stargate” and is reliable.

---

### Milestone 4: Vertical Slice Content

* 2–3 POIs on planet
* 1 POI on moon/asteroid
* Simple objective:

  * scan / retrieve / activate / puzzle allowing a repeatable end-state
* Basic UI prompts + simple save flags

**Done when:** a player can complete the objective in 15–30 minutes.

## 9) Risks & Mitigations

### Risk: Overbuilding “engine tech”

**Mitigation:** Ship a vertical slice with minimal tech; add complexity only when it blocks content.

### Risk: Spherical character controller eats weeks

**Mitigation:** Prototype quickly with crude physics, then iterate only on the feel that matters (grounding + camera + slopes).

### Risk: Jitter/precision issues

**Mitigation:** Keep system scale compact; add floating origin only if needed.

### Risk: Portal ambitions

**Mitigation:** Gate = stylized load transition. True portal deferred.

### Risk: Content creation bottleneck

**Mitigation:** Build one “template POI” and reuse/iterate. Avoid bespoke art early.

## 10) Definition of Done (Vertical Slice)

* Stable on-foot spherical gravity on at least one planet
* Stable ship travel between at least two bodies
* Gate transition to a second scene works 100% reliably across many uses
* A simple objective can be completed end-to-end
* Minimal save state persists the key flags
* Build can be produced and run outside the editor

## 11) Next Actions (Concrete)

1. Create `Core` scene and `System_A` scene
2. Implement `GravityBody` + `GravitySolver`
3. Build the on-foot controller prototype (rough first)
4. Add ship controller with a simple flight model
5. Add gate transition manager with async scene load + spawn anchors
6. Add one small objective and call it the slice

If you want, I can also turn this into a **task breakdown checklist** (GitHub Issues style) with ~30–60 bite-sized tickets ordered by dependency.

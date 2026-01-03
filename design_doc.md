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

## 9) Proposed Architecture

### 9.1 Gameplay Systems (Suggested Components)

* `GravityBody` (radius, curve, priority)
* `GravitySolver` (dominant body)
* `CharacterMotorSpherical`
* `ShipController`
* `FloatingOriginManager` (optional for MVP)
* `GateController` (transition orchestration)
* `InteractionSystem` (raycast + prompt)
* `SaveSystem` (simple flags/state)

### 9.2 Visual Systems (URP)

* `PostProcessProfile` (global volume asset)
* `GateVFX` (VFX Graph or Particle System + Shader Graph)
* `PlanetAtmosphere` (Shader Graph material + parameters)
* `LODProfiles` (per major asset type)

## 10) Milestones & Deliverables

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

## 11) Risks & Mitigations

* **Overbuilding tech:** ship a slice first; defer fancy portal rendering
* **Spherical controller complexity:** prototype rough → iterate on feel
* **Precision/jitter:** keep system compact; add floating origin only if needed
* **Content bottleneck:** build reusable POI templates; keep art scope controlled
* **URP performance traps:** minimal post stack, cautious shadows, early LODs

## 12) Definition of Done (Vertical Slice)

* Stable on-foot spherical gravity on one planet
* Stable ship travel between two bodies
* Reliable gate transition to a second scene
* Objective can be completed end-to-end (15–30 minutes)
* Minimal save state persists key flags
* Stylized URP look established (post + emissives + atmosphere cues)
* Build runs outside editor

## 13) Next Actions

(stub)
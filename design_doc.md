# Project Plan: Scaled Solar-System Exploration Game (Unity + URP)

> **Technical implementation details:** See [ARCHITECTURE.md](ARCHITECTURE.md)  
> **Current progress:** See [CHANGELOG.md](CHANGELOG.md)  
> **AI workflow:** See [.github/copilot-instructions.md](.github/copilot-instructions.md)

## 1) Vision

A 3D exploration game set in a compact, handcrafted star system. Players can:

* Walk on round planets with **dynamic gravity** (local "down" toward planet center).
* Fly a ship between bodies within the system.
* Use a **stargate-like gate** to transition to other locations (scene-streamed).

Primary inspiration: *Outer Wilds* feel (learnable systemic physics + exploration), *Stargate* flavor (gate travel + mystery).

## 2) Design Pillars

1. **Consistent, learnable physics, with realism but secondary to gameplay**
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

### Look Goals

* "Readable, cinematic stylized sci-fi" rather than realism
* Strong shape language, emissive accents, controlled palette
* Planets feel large through scale cues and atmosphere, not ultra-detail

### Key Visual Ingredients

* **Post-processing:** color grading + bloom + subtle vignette (style cohesion)
* **Atmosphere:** fog/height fog style effect, simple atmospheric scattering cues
* **Rim lighting / fresnel:** for silhouettes on characters/props and planet edges
* **Emissives:** gates, ship panels, POIs for navigation readability
* **LOD + culling:** keep far objects cheap (space scenes balloon fast)

### Non-Goals (Early)

* Realistic volumetrics, path-traced lighting, ultra-high fidelity materials
* Complex portal rendering (no see-through gates in slice)

## 5) Initial Scope

### Included (MVP / Vertical Slice)

* One star system (single "play bubble" coordinate space)
* 1 planet (walkable) + 1 moon/asteroid (low grav)
* Ship flight between bodies
* One gate that transitions to a destination area (separate scene)
* Basic interactions (scan / collect / trigger)
* Basic UI (prompt, objective, minimal HUD)
* Save/load for player progression state (simple)
* URP-based stylized lighting + post baseline pass

### Explicitly Not Included (for now)

* Seamless travel between star systems (later via "hyperdrive" transition)
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

---

## 7) Technical Architecture

> **Full details:** See [ARCHITECTURE.md](ARCHITECTURE.md) and [specs/*.spec.md](specs/)

### Summary

| Area | Approach |
|------|----------|
| **Coordinates** | Single-system float bubble; floating origin if jitter occurs |
| **Gravity** | Inverse-square falloff, multi-body accumulation, emergent zero-g |
| **Controllers** | Separate on-foot (spherical gravity) and ship (6DOF physics) |
| **Scenes** | Single scene for MVP; multi-scene streaming deferred |
| **Gates** | Stylized load (fade/VFX), not true portals |
| **Input** | Unity Input System with InputReader ScriptableObject |
| **Assemblies** | Core → Gravity → Player → Ship; UI decoupled |

### Code Conventions

> **Full details:** See [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md)

## 8) Folder Structure

```
Assets/
├── _Project/                 # All game-specific content
│   ├── Scripts/              # See ARCHITECTURE.md for assembly structure
│   ├── Prefabs/
│   ├── Materials/            # M_[Object]_[Variant]
│   ├── Shaders/              # SH_[Purpose]
│   ├── VFX/
│   ├── Audio/
│   ├── Textures/
│   ├── Models/
│   ├── Animations/
│   └── Scenes/
├── Settings/                 # URP assets, post-processing, input actions
└── ThirdParty/               # Imported packages
```

---

## 9) Milestones & Deliverables

> **Detailed plans:** See [plans/milestone-X.plan.md](plans/)

### Milestone 0: Tooling + URP Baseline ✅

* Unity project set to URP
* Folder structure + conventions
* Global post volume with placeholder stylized grade
* Test scene with planet sphere + lighting sanity

---

### Milestone 1: Core Gravity + On-foot Prototype ✅

* Walkable planet sphere with local gravity
* Camera stability and "up" alignment
* Gravity switching between bodies (distance-based)
* Test asteroid for multi-body gravity testing

---

### Milestone 2: Ship Prototype + Planet-to-Space Loop ✅

* Board/unboard ship (F key, fade transitions)
* Fly to moon/asteroid, land, walk
* 6DOF ship flight with physics
* Player state machine (OnFoot ↔ InShip)

---

### Milestone 3: Advanced Gravity System ✅

* Multi-body gravity accumulation (inverse-square falloff)
* Smooth orientation blending (90°/s rotation toward dominant source)
* Emergent Lagrange points (gravity < 0.25 m/s² clamped to zero)
* Gravity UI indicator + F3 debug panel
* Player zero-g thrust movement
* Service locator pattern for decoupled architecture

---

### Milestone 4: Enhanced Camera & Movement Controls

* First-person / third-person camera toggle (V key)
* Airborne roll control (Q/E) - reorient when not grounded
* Jetpack system with fuel management

---

### Milestone 5: Solar System Lighting

* Custom unlit shaders for correct sun-facing illumination at scale
* Day/night terminator, phase angles, cylindrical shadows
* LOD switching between real-lit and distant-shader versions

---

### Milestone 6: UI Foundation

* UIManager, UIScreen, UIPanel framework
* Pause menu, settings screen
* HUD panels (velocity, gravity, ship status)

---

### Milestone 7: Gate Transition (Styled Loading)

* Gate trigger + VFX + async scene load + exit anchors

---

### Milestone 8: Vertical Slice Content + Style Pass

* 2–3 POIs on planet, 1 on moon/asteroid
* Stylized materials + atmosphere + emissive navigation cues

---

## 10) Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| **Overbuilding tech** | Ship a slice first; defer fancy portal rendering |
| **Spherical controller complexity** | Prototype rough → iterate on feel |
| **Precision/jitter** | Keep system compact; add floating origin only if needed |
| **Content bottleneck** | Build reusable POI templates; keep art scope controlled |
| **URP performance traps** | Minimal post stack, cautious shadows, early LODs |

## 11) Definition of Done (Vertical Slice)

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

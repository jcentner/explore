# Changelog

All notable changes to this project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

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



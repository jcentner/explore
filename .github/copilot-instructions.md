---
applyTo: "**"
---

# Explore Game - Copilot Instructions

## Project Overview

A 3D exploration game in Unity 6 (URP) with spherical gravity planets, ship flight, and stargate-style transitions. See `design_doc.md` for full architecture.

## Key Context Files

Always reference these when working on this project:

| File | Purpose |
|------|---------|
| `design_doc.md` | Architecture, milestones, conventions |
| `CHANGELOG.md` | Current progress, what's implemented |
| `specs/*.spec.md` | Per-system specifications |
| `plans/milestone-X.plan.md` | Detailed step-by-step plan for current milestone |

## Milestone Planning Files

**CRITICAL:** Before starting work on any milestone, create or reference a detailed planning file.

### Location & Naming
- Store in `plans/` folder at repo root
- Name format: `milestone-X.plan.md` (e.g., `milestone-2.plan.md`)

### Planning File Structure
```markdown
# Milestone X: [Title]

## Overview
Brief description of milestone goals.

## Prerequisites
- [ ] What must be complete before starting
- [ ] Required assets/scenes/dependencies

## Implementation Steps

### Phase 1: [Name]
- [ ] Step 1.1: Specific task with file paths
- [ ] Step 1.2: Another specific task
  - Substep detail if needed
  - Expected outcome

### Phase 2: [Name]
- [ ] Step 2.1: ...

## Testing Checklist
- [ ] Test case 1
- [ ] Test case 2

## Files to Create/Modify
| File | Action | Purpose |
|------|--------|---------|
| `Scripts/X/Y.cs` | Create | Description |

## Blockers & Decisions
- Decision needed: [description]
- Blocked by: [description]

## Session Log
### [Date] Session N
- Completed: steps X, Y
- In progress: step Z
- Blocked: [if any]
```

## Unity MCP Tools

This project uses Unity MCP for direct Unity Editor control. Prefer these tools over manual instructions:

### Scene Operations
- `manage_scene` - Load, save, get hierarchy, take screenshots
- `manage_gameobject` - Create, modify, delete GameObjects
- `manage_material` - Create materials, set colors/properties, assign to renderers
- `manage_asset` - Search and manage assets

### Common Patterns

**Creating a primitive with material:**
```
1. manage_gameobject(action="create", primitive_type="Sphere", name="...", position=[x,y,z], scale=[x,y,z])
2. manage_material(action="create", material_path="Assets/_Project/Materials/M_Name.mat", shader="Universal Render Pipeline/Lit")
3. manage_material(action="set_material_shader_property", property="_BaseColor", value=[r,g,b,a])
4. manage_material(action="assign_material_to_renderer", target="ObjectName", material_path="...")
```

**Enabling post-processing on camera:**
```
manage_gameobject(action="set_component_property", name="Main Camera", 
    component_name="UniversalAdditionalCameraData",
    component_properties={"UniversalAdditionalCameraData": {"renderPostProcessing": true}})
```

### Always After Changes
- `manage_scene(action="save")` - Save the scene
- `read_console` - Check for errors after script changes

## Unity 6 / URP Specifics

### Volume Profiles (Post-Processing)
- Create via: Project window → Add (+) → Rendering → Volume Profile
- Add to scene: Hierarchy → Volume → Global Volume

### Camera Post-Processing
- URP cameras have post-processing **disabled by default**
- Must enable via `UniversalAdditionalCameraData.renderPostProcessing = true`

### Materials
- Default shader: `Universal Render Pipeline/Lit`
- Base color property: `_BaseColor` (not `_Color`)
- Always use RGBA format: `[r, g, b, a]` where values are 0-1

## Code Conventions

### File Structure
```
Assets/_Project/Scripts/
├── Core/       # Game.Core.asmdef - Interfaces, utilities
├── Gravity/    # Game.Gravity.asmdef - Gravity system
├── Player/     # Game.Player.asmdef - Character, camera
├── Ship/       # Game.Ship.asmdef - Flight, boarding
├── Gates/      # Game.Gates.asmdef - Transitions
├── Interaction/# Game.Interaction.asmdef
├── Save/       # Game.Save.asmdef
└── UI/         # Game.UI.asmdef
```

### Naming
- Classes: `PascalCase`
- Interfaces: `IPascalCase`
- Private fields: `_camelCase`
- Serialized fields: `[SerializeField] private float speed`
- Materials: `M_ObjectName`
- Shaders: `SH_Purpose`

### MonoBehaviour Structure
```csharp
public class Example : MonoBehaviour
{
    // === Inspector Fields ===
    [Header("Section")]
    [SerializeField] private float value;
    
    // === Public Properties ===
    public bool IsActive => _isActive;
    
    // === Private Fields ===
    private bool _isActive;
    
    // === Unity Lifecycle ===
    private void Awake() { }
    
    // === Public Methods ===
    // === Private Methods ===
}
```

### Performance Rules
- Cache GetComponent in Awake(), never in Update()
- Remove empty Update()/FixedUpdate() methods
- Avoid Find*() at runtime
- Prefer Sphere > Capsule > Box >> Mesh colliders

## Current State

**Milestone 0: COMPLETE** ✅
- Folder structure, assembly definitions, specs

**Milestone 1: COMPLETE** ✅
- Gravity system: `GravityManager`, `GravityBody`, `GravitySolver`
- Player system: `CharacterMotorSpherical`, `PlayerCamera`, `InputReader`
- TestGravity scene with Planet_Test, Player, Asteroid_Test
- Spherical gravity walking and jumping works
- Camera aligns to gravity "up" direction

**Milestone 2: Ship Flight** (Next)
- `ShipController` for 6DOF flight
- Boarding/disembarking system
- `PlayerStateController` state machine

**Milestone 3: Advanced Gravity**
- Multi-body gravity accumulation (realistic physics)
- Gravity vector UI indicator
- Add sun gravity

**Milestone 4: Enhanced Camera & Movement**
- First/third person camera toggle
- Airborne rotation controls (Q/E)
- Jetpack 6DOF movement

**Milestone 5: Gate Transition**
- Stargate-style scene transitions

**Milestone 6: Vertical Slice**
- POIs, objectives, art pass

## Session Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                    START OF MILESTONE                           │
├─────────────────────────────────────────────────────────────────┤
│ 1. Create plans/milestone-X.plan.md with full breakdown         │
│ 2. Update "Current State" section below (1 line per milestone)  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    EACH SESSION                                 │
├─────────────────────────────────────────────────────────────────┤
│ 1. Read plans/milestone-X.plan.md first                         │
│ 2. Work through phases, check boxes as you go                   │
│ 3. Update specs/*.spec.md implementation tables if needed       │
│ 4. Add session log entry to plan file at end                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    END OF MILESTONE                             │
├─────────────────────────────────────────────────────────────────┤
│ 1. Mark plan file as ✅ COMPLETE                                 │
│ 2. Add version entry to CHANGELOG.md                            │
│ 3. Update specs/*.spec.md with final status                     │
│ 4. Update "Current State" below                                 │
│ 5. Mark milestone ✅ in design_doc.md §15                        │
└─────────────────────────────────────────────────────────────────┘
```

### Doc Responsibilities

| Document | Updates When |
|----------|-------------|
| `plans/milestone-X.plan.md` | Every session (checkboxes, session log) |
| `specs/*.spec.md` | When implementation status changes |
| `CHANGELOG.md` | Milestone complete (release notes) |
| `design_doc.md` | Milestone complete (✅ marker only) |
| `copilot-instructions.md` | Milestone start/complete (summary) |

## Workflow

1. Check `CHANGELOG.md` for current state
2. Reference relevant `specs/*.spec.md` for implementation details
3. Make changes incrementally, test each piece
4. Save scenes after Unity MCP operations
5. Update `CHANGELOG.md` with progress
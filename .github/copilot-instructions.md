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

## Milestone Planning

Before starting any milestone, reference or create `plans/milestone-X.plan.md`. See `.github/instructions/planning.instructions.md` for the full template.

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

See `.github/instructions/csharp.instructions.md` for full C# conventions.

### Quick Reference
- Namespace: `Explorer.[System]` (e.g., `Explorer.Gravity`)
- Private fields: `_camelCase`
- Materials: `M_ObjectName`
- Shaders: `SH_Purpose`

### Script Folder Structure
```
Assets/_Project/Scripts/
├── Core/       # Game.Core.asmdef
├── Gravity/    # Game.Gravity.asmdef
├── Player/     # Game.Player.asmdef
├── Ship/       # Game.Ship.asmdef
├── Gates/      # Game.Gates.asmdef
├── Interaction/
├── Save/
└── UI/
```

## Current State

**Always read `CHANGELOG.md` for the latest project status and completed milestones.**

For milestone requirements and roadmap, see `design_doc.md` §15.

## Validation Requirements

**After creating/modifying C# scripts:**
1. Use `read_console` to check for compilation errors
2. Wait for domain reload before using new types
3. Only proceed if no errors appear

**After scene changes:**
1. `manage_scene(action="save")` — Save immediately
2. `read_console` — Check for warnings/errors

**After Unity MCP failures:**
1. `manage_scene(action="get_hierarchy")` — Refresh state
2. `refresh_unity(mode="if_dirty")` — Force refresh
3. `read_console` — Diagnose underlying issue

## External Links & Asset Store

- **Never fabricate Unity Asset Store URLs** — Asset slugs/IDs cannot be reliably guessed
- When suggesting external assets, provide **search terms** instead of direct links:
  - ✅ "Search Unity Asset Store for 'spaceship free 3D'"
  - ❌ `https://assetstore.unity.com/packages/3d/vehicles/space/spaceship-free-76512`
- If you must reference a specific asset, explicitly state: "This URL is unverified — please search manually"
- The Asset Store blocks automated verification, so links cannot be validated

## Workflow

1. Read `CHANGELOG.md` for current state
2. Read `plans/milestone-X.plan.md` for active milestone
3. Reference `specs/*.spec.md` for system details
4. Make changes incrementally, validate each step
5. Save scenes and check console after every change
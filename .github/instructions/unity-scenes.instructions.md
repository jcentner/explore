---
applyTo: "**/*.unity"
---

# Unity Scene Instructions for Explore Game

## Scene Layout (TestGravity.unity)

Current scene structure with sun-centric layout:

| Object | Position | Purpose |
|--------|----------|---------|
| Sun | (0, 0, 0) | Visual sphere at origin |
| SunDirectionalLight | (0, 50, 0) | Points toward planet (+X) |
| Planet_Test | (2000, 0, 0) | Main planet, radius 200 |
| Player | (2000, 205, 0) | On planet surface |
| Moon_Test | (2350, 250, 0) | Small moon, radius 30 |

## Unity MCP Scene Operations

### Reading Scene State
```
manage_scene(action="get_hierarchy") — Get full hierarchy with pagination
manage_scene(action="get_active") — Get active scene info
manage_gameobject(action="find", search_term="...", search_method="by_name")
```

### Modifying GameObjects
```
manage_gameobject(action="create", primitive_type="Sphere", name="...", position=[x,y,z], scale=[x,y,z])
manage_gameobject(action="modify", name="...", position=[x,y,z], rotation=[x,y,z])
manage_gameobject(action="add_component", name="...", component_name="...")
manage_gameobject(action="set_component_property", name="...", component_name="...", component_properties={...})
```

### Materials
```
manage_material(action="create", material_path="Assets/_Project/Materials/M_Name.mat", shader="Universal Render Pipeline/Lit")
manage_material(action="set_material_shader_property", material_path="...", property="_BaseColor", value=[r,g,b,a])
manage_material(action="assign_material_to_renderer", target="ObjectName", material_path="...")
```

## URP-Specific Notes

### Camera Post-Processing
URP cameras have post-processing **disabled by default**. Enable via:
```
manage_gameobject(action="set_component_property", name="Main Camera", 
    component_name="UniversalAdditionalCameraData",
    component_properties={"UniversalAdditionalCameraData": {"renderPostProcessing": true}})
```

### Material Properties
- Default shader: `Universal Render Pipeline/Lit`
- Base color property: `_BaseColor` (not `_Color`)
- Color format: `[r, g, b, a]` where values are 0-1

### Volume Profiles
- Create via: Project window → Add (+) → Rendering → Volume Profile
- Add to scene: Hierarchy → Volume → Global Volume

## Validation After Scene Changes

**Always perform these steps after modifying scenes:**

1. `manage_scene(action="save")` — Save the scene immediately
2. `read_console` — Check for any errors or warnings
3. Verify changes with `manage_gameobject(action="find", ...)` if needed

## Error Recovery

If a scene operation fails:
1. `manage_scene(action="get_hierarchy")` — Refresh scene state
2. `refresh_unity(mode="if_dirty")` — Force asset refresh
3. `read_console` — Check for underlying errors

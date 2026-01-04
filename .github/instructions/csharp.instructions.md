---
applyTo: "**/*.cs"
---

# C# Code Conventions for Explore Game

## Namespace
All scripts use namespace matching folder structure: `Explorer.[System]` (e.g., `Explorer.Gravity`, `Explorer.Player`)

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `GravityBody` |
| Interfaces | I + PascalCase | `IGravityAffected` |
| Public methods | PascalCase | `ApplyGravity()` |
| Private fields | _camelCase | `_currentVelocity` |
| Serialized private | camelCase + attribute | `[SerializeField] float fallSpeed` |
| Constants | UPPER_SNAKE | `MAX_GRAVITY_SOURCES` |
| Events | On + PascalCase | `OnLanded`, `OnBoardedShip` |

## MonoBehaviour Structure

```csharp
public class Example : MonoBehaviour
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

## Performance Rules

- Cache `GetComponent<T>()` in `Awake()`, never in `Update()`
- Remove empty `Update()` / `FixedUpdate()` methods — they have overhead even when empty
- Avoid `Find*()` methods at runtime — use direct references or dependency injection
- Never use `.material` in loops — cache it or use `.sharedMaterial`
- Prefer collider types: Sphere > Capsule > Box >> Mesh

## Best Practices

- One class per file, filename matches class name
- Use `[SerializeField] private` over `public` fields
- Use `[Header("Section")]` and `[Tooltip("...")]` for inspector clarity
- Add XML docs on public APIs for better Copilot context
- Use object pooling for frequently spawned objects (VFX, projectiles)

## Assembly Definitions

Scripts are organized into assemblies to speed compilation and enforce dependencies:

```
Game.Core.asmdef          → Core/, interfaces, utilities
    ↑
Game.Gravity.asmdef       → Gravity/ (depends on Core)
    ↑
Game.Player.asmdef        → Player/ (depends on Core, Gravity)
    ↑
Game.Ship.asmdef          → Ship/ (depends on Core, Gravity, Player)
```

## Validation After Changes

After creating or modifying C# scripts:
1. Use `read_console` to check for compilation errors
2. Wait for Unity domain reload to complete before using new types
3. Check `editor_state.isCompiling` if needed

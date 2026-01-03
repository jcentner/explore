# Scripts Directory

Quick reference for code conventions. See `design_doc.md` §13 for full details.

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `GravityBody` |
| Interfaces | I + PascalCase | `IGravityAffected` |
| Public methods | PascalCase | `ApplyGravity()` |
| Private fields | _camelCase | `_currentVelocity` |
| Serialized private | camelCase | `[SerializeField] float fallSpeed` |
| Constants | UPPER_SNAKE | `MAX_GRAVITY_SOURCES` |
| Events | On + PascalCase | `OnLanded` |

## Class Structure Order

1. Inspector Fields (`[SerializeField]`)
2. Public Properties
3. Private Fields
4. Unity Lifecycle (`Awake`, `Start`, `Update`, `FixedUpdate`)
5. Public Methods
6. Private Methods

## Key Rules

- **Cache components** in `Awake()`, never in `Update()`
- **Use `[SerializeField] private`** over `public` fields
- **Remove empty callbacks** – they have overhead
- **One class per file** – filename matches class name
- **XML docs on public APIs** – helps Copilot understand intent

## Assembly Definitions

Each subfolder has its own `.asmdef`:

```
Game.Core      → Core/
Game.Gravity   → Gravity/       (depends: Core)
Game.Player    → Player/, Ship/ (depends: Core, Gravity)
Game.World     → Gates/, Interaction/, Save/ (depends: Core, Gravity)
Game.UI        → UI/            (depends: Core)
```

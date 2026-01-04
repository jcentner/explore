# UI System Specification

**Namespace:** `Explorer.UI`  
**Assembly:** `Game.UI`  
**Status:** 🔲 Not Started

---

## 1. Overview

The UI system provides a consistent, scalable framework for all user interface elements in the game. It separates concerns between gameplay code and UI presentation through interfaces defined in `Game.Core`.

### Design Goals
- **Decoupled:** Gameplay assemblies never reference UI directly
- **Consistent:** All UI uses shared base classes and styling
- **Extensible:** Easy to add new screens and panels
- **Performant:** Proper Canvas usage, no per-frame allocations

---

## 2. Architecture

### Component Hierarchy

```
UIManager (singleton)
├── Screen Stack (LIFO)
│   └── UIScreen instances (PauseScreen, SettingsScreen, etc.)
└── Panel Registry
    └── UIPanel instances (InteractionPromptPanel, VelocityPanel, etc.)
```

### Assembly Dependencies

```
Game.Core (interfaces: IInteractionPrompt, IUIService)
    ↑
Game.UI (implementations: UIManager, screens, panels)
    
Game.Ship / Game.Player
    ↓
Uses: UIService.Get<IInteractionPrompt>() from Game.Core
```

---

## 3. Core Components

### UIService (Game.Core)

Static service locator replacing `InteractionPromptService`.

```csharp
namespace Explorer.Core
{
    public static class UIService
    {
        public static void Register<T>(T instance) where T : class;
        public static void Unregister<T>() where T : class;
        public static T Get<T>() where T : class;
        public static bool TryGet<T>(out T service) where T : class;
    }
}
```

**Usage:**
```csharp
// In UI assembly (registers)
UIService.Register<IInteractionPrompt>(this);

// In Ship assembly (consumes)
if (UIService.TryGet<IInteractionPrompt>(out var prompt))
    prompt.Show("Press F to board");
```

### UIManager

Central controller for all UI. Scene-based singleton (not auto-created).

**Responsibilities:**
- Manages screen stack (push/pop)
- Manages panel visibility
- Handles pause input
- Provides cursor lock management

**Public API:**
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; }
    
    // Screen management
    public void PushScreen<T>() where T : UIScreen;
    public void PopScreen();
    public void PopAllScreens();
    public bool IsScreenOpen<T>() where T : UIScreen;
    
    // Panel management
    public T GetPanel<T>() where T : UIPanel;
    public void ShowPanel<T>() where T : UIPanel;
    public void HidePanel<T>() where T : UIPanel;
    
    // State
    public bool IsPaused { get; }
    public bool IsAnyScreenOpen { get; }
}
```

### UIScreen (Base Class)

Abstract base for full-screen UI elements (menus, settings, etc.).

```csharp
public abstract class UIScreen : MonoBehaviour
{
    protected CanvasGroup CanvasGroup { get; }
    
    public virtual void Open();
    public virtual void Close();
    
    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
    protected virtual void OnBackPressed() => UIManager.Instance.PopScreen();
}
```

**Characteristics:**
- Covers full screen (or significant portion)
- Stacks (new screen covers previous)
- Handles back/escape navigation
- Typically pauses game when open

### UIPanel (Base Class)

Abstract base for HUD elements (always-visible or contextual).

```csharp
public abstract class UIPanel : MonoBehaviour
{
    protected CanvasGroup CanvasGroup { get; }
    
    public virtual void Show();
    public virtual void Hide();
    public bool IsVisible { get; }
    
    protected virtual void OnShown() { }
    protected virtual void OnHidden() { }
}
```

**Characteristics:**
- Partial screen coverage
- Can be shown/hidden independently
- Does not pause game
- Multiple can be visible simultaneously

---

## 4. Input Integration

### Action Maps

| Map | Active When | Actions |
|-----|-------------|---------|
| Player | On foot, not paused | Move, Jump, Interact, Pause |
| Ship | Piloting, not paused | Throttle, Pitch/Yaw/Roll, Exit, Pause |
| UI | Any menu open | Navigate, Submit, Cancel |

### Pause Flow

```
InputReader.OnPause fired
    ↓
UIManager receives event
    ↓
If no screen open → PushScreen<PauseScreen>()
If screen open → PopScreen() (or delegate to screen's OnBackPressed)
    ↓
UIManager sets Time.timeScale (0 or 1)
UIManager toggles Player/Ship input maps
```

---

## 5. UI Elements

### Screens

| Screen | Purpose | Access |
|--------|---------|--------|
| PauseScreen | Pause menu with resume/settings/quit | Escape key |
| SettingsScreen | Audio/graphics/controls settings | Pause menu button |

### Panels

| Panel | Purpose | Visibility |
|-------|---------|------------|
| InteractionPromptPanel | "Press F to board" style prompts | When near interactable |
| VelocityPanel | Speed and altitude display | When piloting ship |
| ShipStatusPanel | Throttle, fuel (future) | When piloting ship |

---

## 6. Visual Standards

### Typography
- **Font:** TextMeshPro default or custom space-themed font
- **Sizes:** Title (48), Header (32), Body (24), Small (18)
- **Colors:** White text, 80% opacity for secondary

### Colors
- **Primary:** #4A90D9 (blue accent)
- **Background:** #1A1A2E (dark navy) at 90% opacity
- **Text:** #FFFFFF
- **Highlight:** #7EC8E3

### Layout
- **Safe area:** Respect screen edges (especially for consoles)
- **Spacing:** 16px standard padding
- **Alignment:** Left-align body text, center titles

---

## 7. Scene Setup

### Canvas Hierarchy

```
Canvas (Screen Space - Overlay)
├── UIManager
├── Panels/
│   ├── InteractionPromptPanel
│   ├── VelocityPanel
│   └── ShipStatusPanel
└── Screens/
    ├── PauseScreen (inactive by default)
    └── SettingsScreen (inactive by default)

EventSystem
```

### Canvas Settings
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920x1080
- Match: 0.5 (width/height balance)

---

## 8. Performance Guidelines

1. **Use CanvasGroup** for show/hide (alpha + blocksRaycasts) instead of SetActive
2. **Avoid Layout Groups** in frequently-updated panels
3. **Cache component references** in Awake, not every frame
4. **Disable raycast targets** on non-interactive elements
5. **Use object pooling** for dynamically-created UI elements

---

## 9. Testing Scenarios

### Interaction Prompts
- [ ] Prompt appears when entering trigger zone
- [ ] Prompt disappears when exiting zone
- [ ] Prompt disappears when interaction completed
- [ ] Multiple overlapping triggers show correct prompt

### Pause System
- [ ] Escape opens pause menu
- [ ] Game freezes (Time.timeScale = 0)
- [ ] Escape again closes menu
- [ ] Resume button closes menu
- [ ] Settings button opens settings
- [ ] Back from settings returns to pause
- [ ] Quit button exits to desktop (or main menu)

### HUD
- [ ] Velocity shows when piloting
- [ ] Velocity hides when on foot
- [ ] Values update in real-time
- [ ] No visual jitter or flicker

---

## 10. Future Extensions

| Feature | Milestone | Notes |
|---------|-----------|-------|
| Inventory UI | TBD | Item grid, equipment slots |
| Map/Navigation | TBD | Solar system view, waypoints |
| Dialogue System | TBD | NPC conversations, choices |
| Quest Tracker | TBD | Objectives, progress |
| Control Rebinding | TBD | Read from Input System |
| Localization | TBD | TextMeshPro localization |

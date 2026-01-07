# UI System Specification

**Namespace:** `Explorer.UI`  
**Assembly:** `Game.UI`  
**Status:** 🔲 Not Started

---

## 1. Overview

The UI system provides a consistent, scalable framework for all user interface elements using **Unity UI Toolkit**. It separates concerns between gameplay code and UI presentation through interfaces defined in `Game.Core`.

### Design Goals
- **Decoupled:** Gameplay assemblies never reference UI directly
- **Consistent:** All UI uses shared USS stylesheets and UXML templates
- **Extensible:** Easy to add new screens and panels
- **Performant:** UI Toolkit retained mode, no per-frame allocations

### Technology Choice: UI Toolkit

| Aspect | UGUI (Canvas) | UI Toolkit |
|--------|---------------|------------|
| Layout | Anchor-based, manual | Flexbox-inspired, responsive |
| Styling | Per-element configuration | USS stylesheets (CSS-like), reusable |
| Performance | Canvas rebuild on changes | Retained mode, efficient updates |
| Workflow | Prefabs, scene hierarchy | UXML templates, visual preview |
| Future | Legacy (maintenance mode) | Unity's recommended path |

---

## 2. Architecture

### Component Hierarchy

```
UIManager (MonoBehaviour + UIDocument)
├── Screen Stack (LIFO)
│   └── UIScreen instances (PauseScreen, SettingsScreen, etc.)
└── Panel Registry
    └── UIPanel instances (InteractionPromptPanel, VelocityPanel, etc.)
```

### Assembly Dependencies

```
Game.Core (interfaces: IInteractionPrompt + UIService<T>)
    ↑
Game.UI (implementations: UIManager, screens, panels)
    
Game.Ship / Game.Player
    ↓
Uses: UIService<IInteractionPrompt>.Instance from Game.Core
```

---

## 3. Core Components

### UIService<T> (Game.Core)

Generic static service locator replacing `InteractionPromptService`.

```csharp
namespace Explorer.Core
{
    public static class UIService<T> where T : class
    {
        private static T _instance;
        
        public static T Instance => _instance;
        
        public static void Register(T instance);
        public static void Unregister(T instance);
        public static bool IsRegistered => _instance != null;
    }
}
```

**Usage:**
```csharp
// In UI assembly (registers)
UIService<IInteractionPrompt>.Register(this);

// In Ship assembly (consumes)
UIService<IInteractionPrompt>.Instance?.Show("Press F to board");
```

### UIManager

Central controller for all UI. Scene-based singleton with `UIDocument`.

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
public abstract class UIScreen
{
    public VisualElement Root { get; }
    
    public virtual void Show();
    public virtual void Hide();
    
    protected virtual void OnShown() { }
    protected virtual void OnHidden() { }
    protected virtual void HandleBack() => UIManager.Instance.PopScreen();
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
public abstract class UIPanel
{
    public VisualElement Root { get; }
    public bool IsVisible { get; }
    
    public virtual void Show();
    public virtual void Hide();
    
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
| Player | On foot, not paused | Move, Jump, Interact, **Pause** |
| Ship | Piloting, not paused | Throttle, Pitch/Yaw/Roll, Exit, **Pause** |
| UI | Any menu open | Navigate, Submit, Cancel |

### Pause Flow

```
InputReader.OnPause fired
    ↓
UIManager receives event
    ↓
If no screen open → PushScreen<PauseScreen>()
If screen open → PopScreen() (or delegate to screen's HandleBack)
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
| GravityIndicatorPanel | Gravity direction arrow | Always (switches player↔ship) |
| GravityDebugPanel | F3 debug overlay | Toggle with F3 key |

---

## 6. Visual Standards

### Typography (UI Toolkit)
- **Font:** Unity default or custom space-themed font asset
- **Sizes:** Title (48px), Header (32px), Body (24px), Small (18px)
- **Colors:** White text, 80% opacity for secondary

### USS Variables (Core.uss)
```css
:root {
    --color-primary: #4A90D9;
    --color-background: rgba(26, 26, 46, 0.9);
    --color-text: #FFFFFF;
    --color-text-secondary: rgba(255, 255, 255, 0.8);
    --color-highlight: #7EC8E3;
    
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;
    
    --font-size-small: 18px;
    --font-size-body: 24px;
    --font-size-header: 32px;
    --font-size-title: 48px;
    
    --transition-fast: 0.15s;
    --transition-normal: 0.25s;
}
```

### Layout
- **Safe area:** Respect screen edges (especially for consoles)
- **Spacing:** 16px standard padding (use `--spacing-md`)
- **Alignment:** Left-align body text, center titles

---

## 7. Scene Setup

### UI Toolkit Hierarchy

```
UI (GameObject)
├── UIDocument (component)
│   └── PanelSettings: UIPanelSettings.asset
│   └── Source Asset: MainUI.uxml
└── UIManager (component)

MainUI.uxml (root)
├── Panels/
│   ├── InteractionPromptPanel
│   ├── VelocityPanel
│   └── ShipStatusPanel
└── Screens/
    ├── PauseScreen (hidden by default)
    └── SettingsScreen (hidden by default)
```

### Panel Settings
- Scale Mode: Scale With Screen Size
- Reference Resolution: 1920x1080
- Match: 0.5 (width/height balance)

---

## 8. Performance Guidelines

1. **Use USS transitions** for animations instead of script-based lerp
2. **Avoid per-frame queries** — cache VisualElement references
3. **Use USS classes** for state changes instead of inline styles
4. **Batch updates** — modify multiple properties then call MarkDirtyRepaint once
5. **Use display:none** instead of opacity:0 when element is fully hidden

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

## 10. File Structure

```
Assets/_Project/
├── Scripts/UI/
│   ├── Game.UI.asmdef
│   ├── UIManager.cs
│   ├── Base/
│   │   ├── UIScreen.cs
│   │   └── UIPanel.cs
│   ├── Panels/
│   │   ├── InteractionPromptPanel.cs
│   │   ├── VelocityPanel.cs
│   │   ├── ShipStatusPanel.cs
│   │   ├── GravityIndicatorPanel.cs
│   │   └── GravityDebugPanel.cs
│   └── Screens/
│       ├── PauseScreen.cs
│       └── SettingsScreen.cs
└── UI/
    ├── MainUI.uxml
    ├── Templates/
    │   ├── InteractionPrompt.uxml
    │   ├── PauseScreen.uxml
    │   └── SettingsScreen.uxml
    └── Styles/
        ├── Core.uss
        ├── InteractionPrompt.uss
        ├── PauseScreen.uss
        └── HUD.uss
```

---

## 11. Future Extensions

| Feature | Milestone | Notes |
|---------|-----------|-------|
| Loading Screen | Milestone 7 | Gate transition overlay |
| Inventory UI | TBD | Item grid, equipment slots |
| Map/Navigation | TBD | Solar system view, waypoints |
| Dialogue System | TBD | NPC conversations, choices |
| Quest Tracker | TBD | Objectives, progress |
| Control Rebinding | TBD | Read from Input System |
| Localization | TBD | UI Toolkit localization support |

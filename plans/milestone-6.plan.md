# Milestone 6: UI Foundation

**Status:** 🔲 Not Started  
**Goal:** Establish a robust, scalable UI framework using **UI Toolkit** with consistent patterns for HUD elements, screens, and interaction prompts.

## 1. Context & Problem Statement

### Current State
- `BoardingPrompt` auto-creates Canvas in code (~50 lines of procedural UI) — hard to style/preview
- `InteractionPromptService` is a prompt-specific static locator, not generic
- `InteractionPrompt.cs` exists but doesn't implement `IInteractionPrompt` (orphaned/dead code)
- `GravityDebugPanel.cs` is in `Game.Gravity` assembly instead of `Game.UI` (breaks architecture)
- `GravityIndicatorPanel` uses UGUI with `PlayerPilotingService` (well-designed, needs migration)
- `InputReader.OnPause` event declared but no `Pause` action wired in input asset
- No consistent UI architecture, base classes, or styling
- Project uses UGUI (`com.unity.ugui` 2.0.0) but will migrate to **UI Toolkit**

### Desired State
- **UI Toolkit** for all new UI (UXML layouts, USS styling, C# controllers)
- Centralized `UIManager` with clear ownership of UI lifecycle
- Base classes (`UIScreen`, `UIPanel`) adapted for UI Toolkit patterns
- Generic `UIService<T>` for any UI contract (not just prompts)
- Proper Input System integration with UI action map
- Extensible prompt system supporting multiple interaction types
- Clean architecture: all UI code in `Game.UI` assembly

---

## 2. Architecture

### Why UI Toolkit?

| Aspect | UGUI (Canvas) | UI Toolkit |
|--------|---------------|------------|
| Layout | Anchor-based, manual positioning | Flexbox-inspired, responsive |
| Styling | Per-element configuration | USS stylesheets (CSS-like), reusable |
| Performance | Canvas rebuild on changes | Retained mode, efficient updates |
| Workflow | Prefabs, scene hierarchy | UXML files, visual preview |
| Future | Legacy (maintenance mode) | Unity's recommended path |

### Assembly Structure
```
Game.UI.asmdef
├── References: Game.Core, Unity.InputSystem, UnityEngine.UIElementsModule
├── No references FROM gameplay assemblies (Ship, Player, etc.)
└── Communication via Core interfaces only
```

### Namespace: `Explorer.UI`

### Core Components

```
UIManager (MonoBehaviour + UIDocument)
├── Manages UIScreen stack (push/pop)
├── Manages UIPanel visibility
├── Handles UI input (pause, back navigation)
├── Provides static access: UIManager.Instance
└── Listens to InputReader.OnPause event

UIScreen (abstract class)
├── VisualElement Root property
├── Show() / Hide() with USS transitions
├── OnShown() / OnHidden() hooks
├── HandleBack() for escape/back handling
└── Examples: PauseScreen, SettingsScreen

UIPanel (abstract class)
├── VisualElement Root property
├── Show() / Hide() with USS transitions
├── Always-visible HUD elements
└── Examples: InteractionPromptPanel, VelocityPanel, ShipStatusPanel

UIService<T> (static generic class in Game.Core)
├── Replaces InteractionPromptService
├── Generic service locator for ANY UI interface
├── UIService<IInteractionPrompt>.Instance
└── UIService<IInteractionPrompt>.Register(instance)
```

### Dependency Flow
```
Game.Core (defines interfaces + UIService<T>)
    ↑
Game.UI (implements interfaces, registers with UIService<T>)
    
Game.Ship/Player (calls UIService<IInteractionPrompt>.Instance?.Show())
```

---

## 3. Implementation Tasks

### Phase 0: Cleanup & Preparation ✅

#### Task 0.1: Delete Orphaned Code ✅
- [x] Delete `Assets/_Project/Scripts/UI/InteractionPromptUI.cs` (doesn't implement interface, unused)
- [x] Verify no references exist before deletion

#### Task 0.2: Move GravityDebugPanel to UI Assembly ✅
- [x] Move `GravityDebugPanel.cs` from `Scripts/Gravity/` to `Scripts/UI/Panels/`
- [x] Update namespace from `Explorer.Gravity` to `Explorer.UI`
- [x] Update `Game.UI.asmdef` if needed (already references Game.Gravity)
- [x] Test F3 debug toggle still works

#### Task 0.3: Update Game.UI.asmdef References ✅
- [x] Verify `UnityEngine.UIElementsModule` is available (built-in to Unity 6)
- [x] Keep reference to `Unity.InputSystem`
- [x] Keep reference to `Unity.TextMeshPro` (existing panels still use TMP)
- [x] Keep reference to `Game.Core`
- [x] Create folder structure: `Scripts/UI/Panels/`, `Scripts/UI/Screens/`, `Scripts/UI/Base/`
- [x] Create UI Toolkit asset folders: `Assets/_Project/UI/Templates/`, `Assets/_Project/UI/Styles/`

---

### Phase 1: Foundation (Core Framework) ✅

#### Task 1.1: Create Generic UIService<T> in Game.Core ✅
- [x] Create `UIService.cs` with generic static class pattern
- [x] Migrate `InteractionPromptService` usage to `UIService<IInteractionPrompt>`
- [x] Update `ShipBoardingTrigger` to use `UIService<IInteractionPrompt>.Instance`
- [x] Remove old `InteractionPromptService` from IInteractionPrompt.cs

**File:** `Assets/_Project/Scripts/Core/UIService.cs`
```csharp
namespace Explorer.Core
{
    /// <summary>
    /// Generic service locator for UI interfaces.
    /// Allows gameplay code to access UI without direct assembly references.
    /// </summary>
    public static class UIService<T> where T : class
    {
        private static T _instance;
        
        public static T Instance => _instance;
        
        public static void Register(T instance)
        {
            if (_instance != null && instance != null)
                Debug.LogWarning($"UIService<{typeof(T).Name}> already registered. Overwriting.");
            _instance = instance;
        }
        
        public static void Unregister(T instance)
        {
            if (_instance == instance)
                _instance = null;
        }
        
        public static bool IsRegistered => _instance != null;
    }
}
```

#### Task 1.2: Create UIManager with UIDocument ✅
- [x] Create `UIManager.cs` with singleton pattern (scene-based, not auto-create)
- [x] Add `UIDocument` component for hosting UI Toolkit content
- [x] Add screen stack management (Stack<UIScreen>)
- [x] Add panel registry (Dictionary<Type, UIPanel>)
- [x] Expose `HandlePauseInput()` method (InputReader wiring deferred to Phase 3)
- [x] Handle back navigation (Escape key pops screen stack)
- [x] Manage cursor lock state

**File:** `Assets/_Project/Scripts/UI/UIManager.cs`

#### Task 1.3: Create Base Classes for UI Toolkit ✅
- [x] Create `UIScreen.cs` - Abstract base for full-screen UI
- [x] Create `UIPanel.cs` - Abstract base for HUD panels
- [x] Use `VisualElement` as root, not `CanvasGroup`
- [x] Add USS transition support for show/hide animations
- [x] Add lifecycle hooks (`OnShown`/`OnHidden`)

**Files:** 
- `Assets/_Project/Scripts/UI/Base/UIScreen.cs`
- `Assets/_Project/Scripts/UI/Base/UIPanel.cs`

```csharp
// UIPanel.cs sketch
namespace Explorer.UI
{
    public abstract class UIPanel
    {
        public VisualElement Root { get; protected set; }
        public bool IsVisible => Root?.style.display == DisplayStyle.Flex;
        
        protected UIPanel(VisualElement root) => Root = root;
        
        public virtual void Show()
        {
            Root.style.display = DisplayStyle.Flex;
            Root.AddToClassList("panel--visible");
            OnShown();
        }
        
        public virtual void Hide()
        {
            Root.RemoveFromClassList("panel--visible");
            Root.style.display = DisplayStyle.None;
            OnHidden();
        }
        
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
```

#### Task 1.4: Create Core USS Stylesheet ✅
- [x] Create `Assets/_Project/UI/Styles/Core.uss` with shared variables and base classes
- [x] Define color variables, typography, spacing
- [x] Create `.panel`, `.screen`, `.btn` base classes
- [x] Create transition classes for animations

**File:** `Assets/_Project/UI/Styles/Core.uss`

#### Task 1.5: Scene Setup ✅
- [x] Create `UI` GameObject in scene with `UIDocument` component
- [x] Add `UIManager` component to UI GameObject
- [x] Create main UXML file that hosts panels and screens
- [ ] **Manual step:** Assign `MainUI.uxml` to UIDocument in Inspector
- [ ] **Manual step:** Create PanelSettings asset if needed

**Files:**
- `Assets/_Project/UI/MainUI.uxml` (root document)
- `Assets/Settings/UIPanelSettings.asset` (panel settings - create via Project window → Create → UI Toolkit → Panel Settings Asset)

---

### Phase 2: Interaction Prompts (Refactor existing) ✅

#### Task 2.1: Create InteractionPromptPanel ✅
- [x] Create `InteractionPromptPanel.cs` extending `UIPanel`
- [x] Implement `IInteractionPrompt` interface
- [x] Support multiple prompt types (board, interact, pickup, etc.)
- [x] Register with `UIService<IInteractionPrompt>` on initialization
- [x] Unregister on disposal

**File:** `Assets/_Project/Scripts/UI/Panels/InteractionPromptPanel.cs`

#### Task 2.2: Extend IInteractionPrompt Interface ✅
- [x] Add `Show(string action, string description)` overload
- [x] Add `Show(InteractionPromptData data)` for complex prompts
- [ ] Add key rebinding support (show actual bound key from Input System) — deferred
- [ ] Consider adding icon support for future gamepad prompts — deferred

**File:** `Assets/_Project/Scripts/Core/IInteractionPrompt.cs` (update)
```csharp
namespace Explorer.Core
{
    public interface IInteractionPrompt
    {
        void Show(string message);
        void Show(string action, string context); // "F", "Board Ship"
        void Show(InteractionPromptData data);
        void Hide();
        bool IsVisible { get; }
    }
    
    public struct InteractionPromptData
    {
        public string ActionKey;      // "F" or icon name
        public string ActionVerb;     // "Board", "Pick up", "Open"
        public string TargetName;     // "Ship", "Artifact", "Gate"
        public bool UseInputBinding;  // If true, resolve ActionKey from Input System
    }
}
```

#### Task 2.3: Create UXML Template ✅
- [x] Create `Assets/_Project/UI/Templates/InteractionPrompt.uxml`
- [x] Add key indicator element (styled box)
- [x] Add action text element
- [x] Add USS classes for visibility transitions

**File:** `Assets/_Project/UI/Templates/InteractionPrompt.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="interaction-prompt panel--hidden">
        <ui:VisualElement class="interaction-prompt__key">
            <ui:Label name="KeyLabel" text="F" />
        </ui:VisualElement>
        <ui:Label name="ActionLabel" text="Board Ship" class="interaction-prompt__action" />
    </ui:VisualElement>
</ui:UXML>
```

#### Task 2.4: Create Interaction Prompt USS ✅
- [x] Create `Assets/_Project/UI/Styles/InteractionPrompt.uss`
- [x] Style key indicator (rounded box, accent color)
- [x] Style action text
- [x] Add fade transition

#### Task 2.5: Migrate BoardingPrompt ✅
- [x] Update `ShipBoardingTrigger` to use `UIService<IInteractionPrompt>.Instance` (done in Phase 1)
- [x] Delete old `BoardingPrompt.cs` and remove from scene
- [ ] Test boarding flow end-to-end — requires manual UIDocument setup

---

### Phase 3: Pause Menu ✅

#### Task 3.1: Wire Pause Input Action ✅
- [x] Add `Pause` action to **Player** action map (Escape key binding)
- [x] Add `Pause` action to **Ship** action map (Escape key binding)
- [x] Wire `OnPause` callback in `InputReader.cs` to invoke event
- [x] Created `IPauseHandler` interface in Core for decoupled access
- [x] `PlayerStateController` subscribes to `OnPause` and calls `UIService<IPauseHandler>.Instance?.HandlePause()`

**Note:** Input System auto-generates C# wrapper when .inputactions file changes.

#### Task 3.2: Create PauseScreen ✅
- [x] Create `PauseScreen.cs` extending `UIScreen`
- [x] Pause game (`Time.timeScale = 0`) on show
- [x] Resume game (`Time.timeScale = 1`) on hide
- [x] Unlock cursor when paused, re-lock on resume
- [x] Buttons: Resume, Settings (placeholder), Quit

**File:** `Assets/_Project/Scripts/UI/Screens/PauseScreen.cs`

#### Task 3.3: Create Pause Screen UXML ✅
- [x] Create `Assets/_Project/UI/Templates/PauseScreen.uxml`
- [x] Add title "PAUSED"
- [x] Add button container with Resume, Settings, Quit
- [x] Add dark overlay background

**File:** `Assets/_Project/UI/Templates/PauseScreen.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="screen pause-screen">
        <ui:VisualElement class="pause-screen__overlay" />
        <ui:VisualElement class="pause-screen__content">
            <ui:Label text="PAUSED" class="pause-screen__title" />
            <ui:VisualElement class="pause-screen__buttons">
                <ui:Button name="ResumeButton" text="Resume" class="btn btn--primary" />
                <ui:Button name="SettingsButton" text="Settings" class="btn" />
                <ui:Button name="QuitButton" text="Quit" class="btn btn--danger" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

#### Task 3.4: Create Pause Screen USS ✅
- [x] Create `Assets/_Project/UI/Styles/PauseScreen.uss`
- [x] Style overlay (semi-transparent dark)
- [x] Style title (large, centered)
- [x] Style buttons with hover states
- [x] Added `.btn--large` and `.btn--danger` classes

#### Task 3.5: Implement Time Management ✅
- [x] PauseScreen sets `Time.timeScale = 0` on show, restores on hide
- [x] HandleBack() resumes game (Escape while paused = resume)
- [x] Cursor unlocked for menu interaction, re-locked on resume
- [ ] **Manual step:** Assign `MainUI.uxml` to UIDocument to test
- [ ] Unlock cursor when paused

---

### Phase 4: HUD Panels

#### Task 4.1: Velocity/Altitude Panel
- [ ] Create `VelocityPanel.cs` extending `UIPanel`
- [ ] Query `Rigidbody` velocity from ship (via interface or direct reference)
- [ ] Calculate altitude above nearest gravity source
- [ ] Only visible when piloting ship (subscribe to `PlayerPilotingService` or state events)
- [ ] Update on fixed interval (not every frame) to reduce jitter

**File:** `Assets/_Project/Scripts/UI/Panels/VelocityPanel.cs`

#### Task 4.2: Create Velocity Panel UXML
- [ ] Create `Assets/_Project/UI/Templates/VelocityPanel.uxml`
- [ ] Speed readout with units (m/s)
- [ ] Altitude readout with units (m)
- [ ] Compact layout for bottom-left corner

#### Task 4.3: Ship Status Panel
- [ ] Create `ShipStatusPanel.cs` extending `UIPanel`
- [ ] Show throttle percentage
- [ ] Placeholder for fuel (future milestone)
- [ ] Only visible when piloting

**File:** `Assets/_Project/Scripts/UI/Panels/ShipStatusPanel.cs`

#### Task 4.4: Create Ship Status Panel UXML
- [ ] Create `Assets/_Project/UI/Templates/ShipStatusPanel.uxml`
- [ ] Throttle bar or percentage
- [ ] Compact layout for bottom-right corner

#### Task 4.5: Migrate GravityIndicatorPanel to UI Toolkit
- [ ] Refactor `GravityIndicatorPanel.cs` to use UI Toolkit (optional, can defer)
- [ ] Keep UGUI version working if migration is complex
- [ ] Ensure gravity arrow and zero-g indicator still function

**Note:** This panel is 432 lines and complex. Consider keeping as UGUI for now and migrating in a later pass.

#### Task 4.6: Create HUD Layout
- [ ] Position panels in main UXML:
  - Bottom-left: Velocity/Altitude
  - Bottom-right: Ship Status (when piloting)
  - Top-center: Interaction prompts
  - Bottom-center or corner: Gravity indicator
- [ ] Ensure safe area margins for different screen sizes

---

### Phase 5: Settings Foundation

#### Task 5.1: Create SettingsScreen
- [ ] Create `SettingsScreen.cs` extending `UIScreen`
- [ ] Back button returns to pause menu (pop screen)
- [ ] Tab navigation: Audio, Graphics, Controls (placeholder)
- [ ] Persist settings via PlayerPrefs (simple for now)

**File:** `Assets/_Project/Scripts/UI/Screens/SettingsScreen.cs`

#### Task 5.2: Create Settings Screen UXML
- [ ] Create `Assets/_Project/UI/Templates/SettingsScreen.uxml`
- [ ] Tab bar at top
- [ ] Content area that switches per tab
- [ ] Back button

#### Task 5.3: Audio Settings Tab
- [ ] Master volume slider (0-100)
- [ ] SFX volume slider
- [ ] Music volume slider
- [ ] Persist via PlayerPrefs keys: `Audio_Master`, `Audio_SFX`, `Audio_Music`

#### Task 5.4: Graphics Settings Tab (Basic)
- [ ] Quality preset dropdown (Low, Medium, High, Ultra)
- [ ] Fullscreen toggle
- [ ] Resolution dropdown (if windowed)
- [ ] Apply button for resolution changes

#### Task 5.5: Controls Tab (Placeholder)
- [ ] Display current key bindings (read-only for now)
- [ ] Placeholder text: "Rebinding coming soon"
- [ ] Future: integrate Input System rebinding UI

---

## 4. Validation Checklist

### Functionality
- [ ] Interaction prompt appears when near ship (via `UIService<IInteractionPrompt>`)
- [ ] Pressing F boards ship (prompt disappears)
- [ ] Pressing Escape opens pause menu
- [ ] Pressing Escape again (or Resume) closes pause menu
- [ ] Game pauses (`Time.timeScale = 0`) when paused
- [ ] Cursor unlocks when paused
- [ ] Settings screen accessible from pause menu
- [ ] Volume sliders affect audio (if audio system exists)
- [ ] HUD shows velocity/altitude when piloting
- [ ] HUD panels hide when on foot
- [ ] F3 debug panel still works after moving to UI assembly

### Code Quality
- [ ] No direct references from Ship/Player to UI assembly
- [ ] All UI components use `UIService<T>` for cross-assembly communication
- [ ] Base classes (`UIScreen`, `UIPanel`) reduce boilerplate
- [ ] USS stylesheets used consistently (no inline styles)
- [ ] UXML templates are reusable and well-structured

### Performance
- [ ] UI Toolkit document updates efficiently (no layout thrashing)
- [ ] No per-frame string allocations in UI updates
- [ ] USS transitions used instead of script-based animations where possible

---

## 5. File Checklist

### Scripts to Create
- [ ] `Assets/_Project/Scripts/Core/UIService.cs` (generic `UIService<T>`)
- [ ] `Assets/_Project/Scripts/UI/UIManager.cs`
- [ ] `Assets/_Project/Scripts/UI/Base/UIScreen.cs`
- [ ] `Assets/_Project/Scripts/UI/Base/UIPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/InteractionPromptPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/VelocityPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/ShipStatusPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Screens/PauseScreen.cs`
- [ ] `Assets/_Project/Scripts/UI/Screens/SettingsScreen.cs`

### Scripts to Modify
- [ ] `Assets/_Project/Scripts/Core/IInteractionPrompt.cs` — Extend interface with overloads
- [ ] `Assets/_Project/Scripts/Ship/ShipBoardingTrigger.cs` — Use `UIService<IInteractionPrompt>`
- [ ] `Assets/_Project/Scripts/Input/InputReader.cs` — Wire `OnPause` to actual input action
- [ ] `Assets/_Project/Scripts/UI/Game.UI.asmdef` — Update references for UI Toolkit

### Scripts to Move
- [ ] `Assets/_Project/Scripts/Gravity/GravityDebugPanel.cs` → `Assets/_Project/Scripts/UI/Panels/GravityDebugPanel.cs`

### Scripts to Delete
- [ ] `Assets/_Project/Scripts/Core/InteractionPromptService.cs` (replaced by `UIService<T>`)
- [ ] `Assets/_Project/Scripts/UI/BoardingPrompt.cs` (replaced by `InteractionPromptPanel`)
- [ ] `Assets/_Project/Scripts/UI/InteractionPromptUI.cs` (orphaned, doesn't implement interface)

### UI Toolkit Assets to Create
- [ ] `Assets/_Project/UI/MainUI.uxml` (root document)
- [ ] `Assets/_Project/UI/Templates/InteractionPrompt.uxml`
- [ ] `Assets/_Project/UI/Templates/PauseScreen.uxml`
- [ ] `Assets/_Project/UI/Templates/SettingsScreen.uxml`
- [ ] `Assets/_Project/UI/Templates/VelocityPanel.uxml`
- [ ] `Assets/_Project/UI/Templates/ShipStatusPanel.uxml`
- [ ] `Assets/_Project/UI/Styles/Core.uss` (shared variables, base classes)
- [ ] `Assets/_Project/UI/Styles/InteractionPrompt.uss`
- [ ] `Assets/_Project/UI/Styles/PauseScreen.uss`
- [ ] `Assets/_Project/UI/Styles/SettingsScreen.uss`
- [ ] `Assets/_Project/UI/Styles/HUD.uss` (shared HUD panel styles)
- [ ] `Assets/Settings/UIPanelSettings.asset`

### Input System
- [ ] Add `Pause` action to **Player** action map (Escape key)
- [ ] Add `Pause` action to **Ship** action map (Escape key)
- [ ] Ensure `UI` action map exists for menu navigation

---

## 6. Dependencies & Risks

### Dependencies
- Unity UI Toolkit (included in Unity 6)
- Input System package (already present)
- Functioning ship boarding (Milestone 2 ✅)
- `PlayerPilotingService` for HUD visibility (Milestone 3 ✅)

### Risks
| Risk | Mitigation |
|------|------------|
| UI Toolkit learning curve | Start with simple panels, reference Unity docs |
| UI breaks input when paused | Use separate UI action map, disable Player/Ship maps |
| Service locator pattern misuse | Document patterns clearly, use generic `UIService<T>` |
| `GravityIndicatorPanel` migration complexity | Keep UGUI version initially, migrate later |
| USS transitions not working | Fall back to script-based lerp if needed |
| Time.timeScale affects physics | Ensure resume restores exactly 1.0f |

### UI Toolkit Considerations
- **No TextMeshPro** — UI Toolkit uses its own text rendering
- **No prefabs** — Use UXML templates instead
- **No CanvasGroup** — Use USS opacity and display properties
- **Different event system** — Use `RegisterCallback<T>()` instead of `UnityEvent`

---

## 7. Definition of Done

- [ ] `UIManager` exists in scene with `UIDocument` and manages screen/panel lifecycle
- [ ] Generic `UIService<T>` replaces `InteractionPromptService`
- [ ] Interaction prompts work via `UIService<IInteractionPrompt>` (not direct reference)
- [ ] Pause menu opens/closes with Escape key
- [ ] Settings screen has working audio sliders
- [ ] HUD panels show velocity/ship status when piloting
- [ ] `GravityDebugPanel` moved to UI assembly, F3 still works
- [ ] Orphaned `InteractionPrompt.cs` deleted
- [ ] No compilation errors or console warnings
- [ ] All UI uses USS stylesheets with consistent visual styling
- [ ] Documentation updated (CHANGELOG, specs/ui-system.spec.md)

---

## 8. Folder Structure (Final)

```
Assets/_Project/
├── Scripts/
│   ├── Core/
│   │   ├── UIService.cs              # Generic UIService<T>
│   │   └── IInteractionPrompt.cs     # Extended interface
│   └── UI/
│       ├── Game.UI.asmdef
│       ├── UIManager.cs
│       ├── Base/
│       │   ├── UIScreen.cs
│       │   └── UIPanel.cs
│       ├── Panels/
│       │   ├── InteractionPromptPanel.cs
│       │   ├── VelocityPanel.cs
│       │   ├── ShipStatusPanel.cs
│       │   ├── GravityIndicatorPanel.cs  # (existing, UGUI for now)
│       │   └── GravityDebugPanel.cs      # (moved from Gravity/)
│       └── Screens/
│           ├── PauseScreen.cs
│           └── SettingsScreen.cs
└── UI/
    ├── MainUI.uxml                   # Root document
    ├── Templates/
    │   ├── InteractionPrompt.uxml
    │   ├── PauseScreen.uxml
    │   ├── SettingsScreen.uxml
    │   ├── VelocityPanel.uxml
    │   └── ShipStatusPanel.uxml
    └── Styles/
        ├── Core.uss                  # Variables, base classes
        ├── InteractionPrompt.uss
        ├── PauseScreen.uss
        ├── SettingsScreen.uss
        └── HUD.uss
```

---

## 9. Future Considerations (Out of Scope)

These are noted for future milestones:
- Migrate `GravityIndicatorPanel` from UGUI to UI Toolkit
- Inventory UI
- Map/navigation UI
- Dialogue system
- Quest/objective tracker
- Control rebinding UI (via Input System)
- Localization support
- Loading screen for gate transitions (Milestone 7)

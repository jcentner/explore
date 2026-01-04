# Milestone 6: UI Foundation

**Status:** 🔲 Not Started  
**Goal:** Establish a robust, scalable UI framework with consistent patterns for HUD elements, screens, and interaction prompts.

## 1. Context & Problem Statement

### Current State
- `BoardingPrompt` auto-creates itself via singleton pattern
- `InteractionPromptService` is a static service locator with no lifecycle management
- No consistent UI architecture or base classes
- No pause menu, settings, or HUD elements beyond the boarding prompt
- UI elements scattered without consistent styling

### Desired State
- Centralized `UIManager` with clear ownership of UI lifecycle
- Base classes (`UIScreen`, `UIPanel`) for consistent behavior
- Prefab-based UI with TextMeshPro and consistent styling
- Proper Input System integration with UI action map
- Extensible prompt system supporting multiple interaction types

---

## 2. Architecture

### Assembly Structure
```
Game.UI.asmdef
├── References: Game.Core, Unity.TextMeshPro, Unity.InputSystem
├── No references FROM gameplay assemblies (Ship, Player, etc.)
└── Communication via Core interfaces only
```

### Namespace: `Explorer.UI`

### Core Components

```
UIManager (MonoBehaviour, singleton via scene)
├── Manages UIScreen stack (push/pop)
├── Manages UIPanel visibility
├── Handles UI input (pause, back navigation)
├── Provides static access: UIManager.Instance
└── Listens to InputReader.OnPause event

UIScreen (abstract MonoBehaviour)
├── Open() / Close() with optional animations
├── OnScreenOpened() / OnScreenClosed() hooks
├── BackAction() for escape/back handling
└── Examples: PauseScreen, SettingsScreen

UIPanel (abstract MonoBehaviour)  
├── Show() / Hide() with optional animations
├── Always-visible HUD elements
└── Examples: InteractionPromptPanel, VelocityPanel, ShipStatusPanel

UIService (static class in Game.Core)
├── Replaces InteractionPromptService
├── Generic service locator for UI interfaces
├── UIService.Get<IInteractionPrompt>()
└── UIService.Register<T>(T instance)
```

### Dependency Flow
```
Game.Core (defines interfaces)
    ↑
Game.UI (implements interfaces, registers with UIService)
    
Game.Ship/Player (calls UIService.Get<IInterface>())
```

---

## 3. Implementation Tasks

### Phase 1: Foundation (Core framework)

#### Task 1.1: Create UIService in Game.Core
- [ ] Create `UIService.cs` - Generic service locator
- [ ] Migrate from `InteractionPromptService` to `UIService`
- [ ] Update `ShipBoardingTrigger` to use `UIService.Get<IInteractionPrompt>()`
- [ ] Remove old `InteractionPromptService.cs`

**File:** `Assets/_Project/Scripts/Core/UIService.cs`
```csharp
namespace Explorer.Core
{
    public static class UIService
    {
        private static readonly Dictionary<Type, object> _services = new();
        
        public static void Register<T>(T instance) where T : class
        public static void Unregister<T>() where T : class
        public static T Get<T>() where T : class
        public static bool TryGet<T>(out T service) where T : class
    }
}
```

#### Task 1.2: Create UIManager
- [ ] Create `UIManager.cs` with singleton pattern (scene-based, not auto-create)
- [ ] Add screen stack management (List<UIScreen>)
- [ ] Add panel registry (Dictionary<Type, UIPanel>)
- [ ] Subscribe to `InputReader.OnPause` for pause menu toggle
- [ ] Handle back navigation (Escape key pops screen stack)

**File:** `Assets/_Project/Scripts/UI/UIManager.cs`

#### Task 1.3: Create Base Classes
- [ ] Create `UIScreen.cs` - Abstract base for full-screen UI
- [ ] Create `UIPanel.cs` - Abstract base for HUD panels
- [ ] Add animation hooks (virtual methods for open/close)
- [ ] Add CanvasGroup for fade transitions

**Files:** 
- `Assets/_Project/Scripts/UI/Base/UIScreen.cs`
- `Assets/_Project/Scripts/UI/Base/UIPanel.cs`

#### Task 1.4: Scene Setup
- [ ] Create `UI` Canvas in scene (Screen Space - Overlay)
- [ ] Add `UIManager` GameObject under Canvas
- [ ] Create folder structure under Canvas: Screens/, Panels/
- [ ] Add EventSystem if not present

---

### Phase 2: Interaction Prompts (Refactor existing)

#### Task 2.1: Create InteractionPromptPanel
- [ ] Create `InteractionPromptPanel.cs` extending `UIPanel`
- [ ] Implement `IInteractionPrompt` interface
- [ ] Support multiple prompt types (board, interact, pickup, etc.)
- [ ] Register with `UIService` on Awake
- [ ] Unregister on OnDestroy

**File:** `Assets/_Project/Scripts/UI/Panels/InteractionPromptPanel.cs`

#### Task 2.2: Extend IInteractionPrompt Interface
- [ ] Add `Show(string action, string description)` overload
- [ ] Add `Show(InteractionPromptData data)` for complex prompts
- [ ] Add key rebinding support (show actual bound key)

**File:** `Assets/_Project/Scripts/Core/IInteractionPrompt.cs` (update)

#### Task 2.3: Create Prefab
- [ ] Create `P_InteractionPrompt.prefab`
- [ ] Add TextMeshPro text element
- [ ] Add background panel with consistent styling
- [ ] Add fade animation via CanvasGroup

#### Task 2.4: Migrate BoardingPrompt
- [ ] Update `ShipBoardingTrigger` to use new panel
- [ ] Remove old `BoardingPrompt.cs`
- [ ] Test boarding flow

---

### Phase 3: Pause Menu

#### Task 3.1: Add Pause Input Action
- [ ] Add `Pause` action to Player action map (Escape key)
- [ ] Add `OnPause` event to `InputReader`
- [ ] UIManager subscribes to pause event

#### Task 3.2: Create PauseScreen
- [ ] Create `PauseScreen.cs` extending `UIScreen`
- [ ] Pause game (Time.timeScale = 0) on open
- [ ] Resume game on close
- [ ] Button: Resume, Settings, Quit

**File:** `Assets/_Project/Scripts/UI/Screens/PauseScreen.cs`

#### Task 3.3: Create Prefab
- [ ] Create `P_PauseScreen.prefab`
- [ ] Add title text "PAUSED"
- [ ] Add buttons with consistent styling
- [ ] Add dark overlay background

#### Task 3.4: Implement Time Management
- [ ] Ensure pause works correctly with physics
- [ ] Handle edge cases (pausing while boarding, etc.)
- [ ] Disable player/ship input while paused

---

### Phase 4: HUD Panels

#### Task 4.1: Velocity/Altitude Panel
- [ ] Create `VelocityPanel.cs` extending `UIPanel`
- [ ] Show current velocity (m/s)
- [ ] Show altitude above nearest gravity source
- [ ] Only visible when piloting ship

**File:** `Assets/_Project/Scripts/UI/Panels/VelocityPanel.cs`

#### Task 4.2: Ship Status Panel
- [ ] Create `ShipStatusPanel.cs` extending `UIPanel`
- [ ] Show throttle percentage
- [ ] Placeholder for fuel (future)
- [ ] Only visible when piloting

**File:** `Assets/_Project/Scripts/UI/Panels/ShipStatusPanel.cs`

#### Task 4.3: Create HUD Layout
- [ ] Bottom-left: Velocity/Altitude
- [ ] Bottom-right: Ship Status (when piloting)
- [ ] Top-center: Interaction prompts
- [ ] Consistent font, colors, spacing

---

### Phase 5: Settings Foundation

#### Task 5.1: Create SettingsScreen
- [ ] Create `SettingsScreen.cs` extending `UIScreen`
- [ ] Back button returns to pause menu
- [ ] Tabs or sections: Audio, Graphics, Controls (placeholder)

**File:** `Assets/_Project/Scripts/UI/Screens/SettingsScreen.cs`

#### Task 5.2: Audio Settings
- [ ] Master volume slider
- [ ] SFX volume slider
- [ ] Music volume slider
- [ ] Persist via PlayerPrefs (simple for now)

#### Task 5.3: Graphics Settings (Basic)
- [ ] Quality preset dropdown
- [ ] Fullscreen toggle
- [ ] Resolution dropdown (if windowed)

---

## 4. Validation Checklist

### Functionality
- [ ] Boarding prompt appears when near ship
- [ ] Pressing F boards ship (prompt disappears)
- [ ] Pressing Escape opens pause menu
- [ ] Pressing Escape again (or Resume) closes pause
- [ ] Game pauses (Time.timeScale = 0) when paused
- [ ] Settings screen accessible from pause menu
- [ ] Volume sliders affect audio
- [ ] HUD shows velocity when piloting

### Code Quality
- [ ] No direct references from Ship/Player to UI assembly
- [ ] All UI components use UIService for cross-assembly communication
- [ ] Base classes reduce boilerplate
- [ ] Prefabs use consistent styling

### Performance
- [ ] UI Canvas uses appropriate render mode
- [ ] CanvasGroup used for batched alpha changes
- [ ] No per-frame allocations in UI updates

---

## 5. File Checklist

### Scripts to Create
- [ ] `Assets/_Project/Scripts/Core/UIService.cs`
- [ ] `Assets/_Project/Scripts/UI/UIManager.cs`
- [ ] `Assets/_Project/Scripts/UI/Base/UIScreen.cs`
- [ ] `Assets/_Project/Scripts/UI/Base/UIPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/InteractionPromptPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/VelocityPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Panels/ShipStatusPanel.cs`
- [ ] `Assets/_Project/Scripts/UI/Screens/PauseScreen.cs`
- [ ] `Assets/_Project/Scripts/UI/Screens/SettingsScreen.cs`

### Scripts to Modify
- [ ] `Assets/_Project/Scripts/Core/IInteractionPrompt.cs` - Extend interface
- [ ] `Assets/_Project/Scripts/Ship/ShipBoardingTrigger.cs` - Use UIService
- [ ] `Assets/_Project/Scripts/Input/InputReader.cs` - Add OnPause event

### Scripts to Remove
- [ ] `Assets/_Project/Scripts/Core/InteractionPromptService.cs` (merged into UIService)
- [ ] `Assets/_Project/Scripts/UI/BoardingPrompt.cs` (replaced by InteractionPromptPanel)

### Prefabs to Create
- [ ] `Assets/_Project/Prefabs/UI/P_InteractionPrompt.prefab`
- [ ] `Assets/_Project/Prefabs/UI/P_PauseScreen.prefab`
- [ ] `Assets/_Project/Prefabs/UI/P_SettingsScreen.prefab`
- [ ] `Assets/_Project/Prefabs/UI/P_VelocityPanel.prefab`
- [ ] `Assets/_Project/Prefabs/UI/P_ShipStatusPanel.prefab`

### Input System
- [ ] Add `Pause` action to Player action map
- [ ] Add `UI` action map for menu navigation

---

## 6. Dependencies & Risks

### Dependencies
- TextMeshPro package (already present)
- Input System package (already present)
- Functioning ship boarding (Milestone 2 ✅)

### Risks
| Risk | Mitigation |
|------|------------|
| UI breaks input when paused | Use separate UI action map, disable Player/Ship maps |
| Service locator pattern misuse | Document patterns, code review |
| Canvas performance | Use appropriate render mode, batch draws |
| Time.timeScale affects physics | Ensure resume restores exactly 1.0 |

---

## 7. Definition of Done

- [ ] UIManager exists in scene and manages screen/panel lifecycle
- [ ] Interaction prompts work via UIService (not direct reference)
- [ ] Pause menu opens/closes with Escape
- [ ] Settings screen has working audio sliders
- [ ] HUD panels show velocity/ship status when appropriate
- [ ] No compilation errors or console warnings
- [ ] All UI uses consistent visual styling
- [ ] Documentation updated (CHANGELOG, specs)

---

## 8. Future Considerations (Out of Scope)

These are noted for future milestones:
- Inventory UI
- Map/navigation UI
- Dialogue system
- Quest/objective tracker
- Control rebinding UI
- Localization support

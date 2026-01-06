# Milestone 4: Camera Perspective Toggle

**Status:** 🔲 Not Started  
**Goal:** Add first-person / third-person camera toggle for flexible exploration views.

## 1. Context & Problem Statement

### Current State (Milestone 1-3)
- Third-person camera only
- Player rotation locked to gravity direction
- Zero-g zones (from M3) leave player helpless (intentional for now)

### Desired State
- Toggle between first-person and third-person views (V key)
- Smooth transitions between perspectives
- Player model hidden in first-person (shadows preserved)

### Design Pillar Alignment
> "Exploration-first"

Camera perspective options support exploration:
- First-person for tight spaces, detail examination, immersion
- Third-person for navigation, situational awareness

### Out of Scope (Deferred)
- Jetpack system (removed from M4)
- Airborne roll control (removed from M4)
- Player zero-g thrust (intentionally helpless in space)

---

## 2. Feature Breakdown

### Feature A: Camera Perspective Toggle
| Aspect | Third-Person | First-Person |
|--------|--------------|--------------|
| View | Behind/above player | From player's eyes |
| Player model | Visible | Hidden (shadow caster only) |
| Use case | Navigation, awareness | Detail examination, tight spaces |
| Default | Yes | No |

### Transition Behavior
- V key toggles between perspectives
- Smooth position/rotation interpolation (0.3s)
- No instant snapping

### First-Person Model Handling
- Hide mesh renderers on player model
- Keep shadow casters enabled (player still casts shadow)
- Toggle via `Renderer.enabled` or shadow-only layer

---

## 3. Architecture

### PlayerCamera Extensions
```
PlayerCamera (existing)
├── CameraPerspective enum { ThirdPerson, FirstPerson }
├── _currentPerspective
├── _firstPersonOffset (head height)
├── _perspectiveTransitionTime
├── TogglePerspective()
└── SetPlayerModelVisibility(bool visible)
```

### Input Action Updates
```
Player Action Map (additions)
└── ToggleCameraView  → V key (button)
```

### No New Components Needed
- All changes within existing `PlayerCamera.cs`
- Input binding via existing `InputReader.cs` pattern

---

## 4. Implementation Tasks

### Phase 1: Input Setup ✅

#### Task 1.1: Add ToggleCameraView Action
- [x] Add `ToggleCameraView` action to Player action map (V key)
- [x] Add `OnToggleCameraView` event to `InputReader.cs`
- [x] Wire up performed callback

**Files Modified:**
- `Assets/_Project/Resources/InputSystem_Actions.inputactions`
- `Assets/Settings/InputSystem_Actions.inputactions`
- `Assets/_Project/Scripts/Player/InputReader.cs`

---

### Phase 2: Camera Perspective Toggle ✅

#### Task 2.1: Create CameraPerspective Enum
- [x] Define `CameraPerspective { ThirdPerson, FirstPerson }`
- [x] Add `_currentPerspective` field to `PlayerCamera`

#### Task 2.2: Add First-Person Configuration
- [x] Add `_firstPersonOffset` field (default: 0, 1.6, 0)
- [x] Add `_perspectiveTransitionTime` field (default: 0.3s)

```csharp
[Header("First Person")]
[SerializeField] private Vector3 _firstPersonOffset = new Vector3(0f, 1.6f, 0f);
[SerializeField] private float _perspectiveTransitionTime = 0.3f;
```

#### Task 2.3: Implement Perspective Toggle
- [x] Add `TogglePerspective()` method
- [x] Smooth transition between offsets/distances
- [x] Handle Look input differently per perspective

#### Task 2.4: Wire Input to Toggle
- [x] Subscribe to `InputReader.OnToggleCameraView`
- [x] Call `TogglePerspective()` on event

**Files Modified:**
- `Assets/_Project/Scripts/Player/PlayerCamera.cs`

---

### Phase 3: First-Person Model Handling ✅

#### Task 3.1: Player Model Visibility
- [x] Add reference to player model renderers
- [x] Create `SetPlayerModelVisibility(bool visible)` method
- [x] Hide mesh renderers but preserve shadow casting

```csharp
[Header("Model Visibility")]
[SerializeField] private Renderer[] _playerRenderers;
```

#### Task 3.2: Shadow-Only Mode
- [x] When hiding model, set `renderer.shadowCastingMode = ShadowsOnly`
- [x] When showing model, restore to `On`

#### Task 3.3: Integration
- [x] Call visibility toggle during perspective transition
- [x] Added `ResetToThirdPerson()` for ship boarding edge case
- [x] Auto-find renderers in PlayerInitializer if not assigned

**Files Modified:**
- `Assets/_Project/Scripts/Player/PlayerCamera.cs`
- `Assets/_Project/Scripts/Player/PlayerInitializer.cs`

---

### Phase 4: Remove Zero-G Thrust ✅

#### Task 4.1: Remove HandleZeroGMovement
- [x] Delete zero-g thrust code from `CharacterMotorSpherical`
- [x] Player floats helplessly in zero-g (intentional design)
- [x] Keep grounding detection and gravity alignment
- [x] Keep IsInZeroG property and events (used by UI)
- [x] Keep slight drag (0.1) so player doesn't drift forever

**Files Modified:**
- `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs`

---

### Phase 5: Polish & Testing

#### Task 5.1: Edge Cases
- [ ] Test on curved surfaces (planet curvature)
- [ ] Test during gravity transitions
- [ ] Test perspective during ship boarding/exit
- [ ] Verify camera collision in both perspectives

#### Task 5.2: Feel Tuning
- [ ] Adjust transition timing
- [ ] Adjust first-person offset for different player scales
- [ ] Test Look sensitivity in both modes

**Status:** Ready for testing

---

## 5. Input Mapping Summary

### New Actions (Player Map)

| Action | Type | Binding | Notes |
|--------|------|---------|-------|
| ToggleCameraView | Button | V | Toggle 1st/3rd person |

---

## 6. Validation Checklist

### Camera Perspective
- [ ] V key toggles between perspectives
- [ ] Smooth transition animation (no snap)
- [ ] Player model hidden in first-person
- [ ] Player shadow still visible in first-person
- [ ] Both perspectives work on curved surfaces
- [ ] Camera collision works in both modes
- [ ] Perspective resets to third-person on ship boarding

### Zero-G Behavior
- [ ] Player floats helplessly in zero-g zones (no thrust)
- [ ] Gravity alignment still works when gravity resumes
- [ ] Ship is only means of zero-g traversal

---

## 7. File Checklist

### Scripts to Modify
- [ ] `Assets/_Project/Scripts/Player/PlayerCamera.cs` - Add first-person mode
- [ ] `Assets/_Project/Scripts/Player/InputReader.cs` - Add ToggleCameraView event
- [ ] `Assets/_Project/Scripts/Player/CharacterMotorSpherical.cs` - Remove zero-g thrust

### Input System
- [ ] `Assets/_Project/Resources/InputSystem_Actions.inputactions`

### No New Scripts Required
Camera perspective is an extension of existing `PlayerCamera.cs`

---

## 8. Dependencies & Risks

### Dependencies
- Milestone 1-3 complete ✅
- Player model must have accessible renderers for visibility toggle

### Risks

| Risk | Mitigation |
|------|------------|
| First-person causes motion sickness | Add FOV options, limit rotation speed |
| First-person offset wrong for player scale | Make offset configurable per-prefab |
| Shadow-only mode not working in URP | Test early, fallback to layer-based hiding |
| Camera collision too aggressive in FP | Use smaller collision sphere in first-person |

---

## 9. Definition of Done

- [ ] Camera toggles between first and third person smoothly (V key)
- [ ] Player model hidden in first-person, shadow preserved
- [ ] Transition is smooth (no instant snapping)
- [ ] Zero-g thrust removed (player helpless in space)
- [ ] All existing gameplay still works
- [ ] Spec updated with camera perspective details
- [ ] CHANGELOG updated

---

## 10. Future Considerations (Out of Scope)

| Feature | Notes |
|---------|-------|
| First-person arms | Visible hands/tools in first-person |
| VR support | First-person mode is VR-ready foundation |
| Jetpack system | Deferred, may revisit in future milestone |
| Airborne roll | Deferred, may revisit if needed |
| Camera mode UI indicator | Can add in M6 (UI Foundation) |

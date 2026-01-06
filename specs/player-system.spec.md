# Player System Specification

**Status: ✅ IMPLEMENTED (Milestones 1-3, M4 in progress)**

## Implementation Notes

| Component | File | Status |
|-----------|------|--------|
| `CharacterMotorSpherical` | `Scripts/Player/CharacterMotorSpherical.cs` | ✅ Complete |
| `PlayerCamera` | `Scripts/Player/PlayerCamera.cs` | 🔄 M4: Adding perspective toggle |
| `InputReader` | `Scripts/Player/InputReader.cs` | 🔄 M4: Adding ToggleCameraView |
| `PlayerInitializer` | `Scripts/Player/PlayerInitializer.cs` | ✅ Complete |
| `PlayerStateController` | `Scripts/Player/PlayerStateController.cs` | ✅ Complete |
| `PlayerState` | `Scripts/Player/PlayerState.cs` | ✅ Complete |
| `IPilotable` | `Scripts/Player/IPilotable.cs` | ✅ Complete |

### Known Limitations
- Camera collision avoidance fields exist but not fully utilized
- No slope sliding logic
- Player is helpless in zero-g (intentional design)

---

## Purpose

Handle on-foot player movement, camera control, and state management on spherical surfaces with dynamic gravity.

## Design Goals

- **Spherical gravity support** – "Down" is always toward planet center
- **Responsive feel** – Snappy movement, no floaty controls
- **Clear state machine** – Grounded, falling, jumping are distinct
- **Camera stability** – No nauseating rotation, smooth "up" alignment

## Components

### CharacterMotorSpherical : MonoBehaviour

Core movement controller for on-foot gameplay.

**Inspector Fields:**
```csharp
[Header("Movement")]
[SerializeField] float walkSpeed = 5f;
[SerializeField] float runSpeed = 8f;
[SerializeField] float acceleration = 10f;
[SerializeField] float deceleration = 12f;

[Header("Jumping")]
[SerializeField] float jumpForce = 8f;
[SerializeField] float airControl = 0.3f;

[Header("Grounding")]
[SerializeField] float groundCheckDistance = 0.2f;
[SerializeField] LayerMask groundLayers;

[Header("Rotation")]
[SerializeField] float upAlignmentSpeed = 10f;
```

**Dependencies:**
- Requires `GravitySolver` component
- Requires `Rigidbody` (set to kinematic: false, no gravity)
- Requires `CapsuleCollider`

**Key Methods:**
```csharp
public void SetMoveInput(Vector2 input);
public void Jump();
public bool IsGrounded { get; }
public bool IsMoving { get; }
public Vector3 LocalUp { get; } // Opposite of gravity direction
```

### PlayerStateController : MonoBehaviour

Manages high-level player states.

**States:**
| State | Description |
|-------|-------------|
| `OnFoot` | Walking on a surface, full movement control |
| `Falling` | In air, reduced control |
| `InShip` | Controlling ship, player hidden/disabled |
| `Interacting` | In dialogue/interaction, input locked |
| `Transitioning` | Gate travel, input locked |

**Events:**
```csharp
public event Action<PlayerState> OnStateChanged;
public event Action OnBoarded;    // Entered ship
public event Action OnDisembarked; // Exited ship
```

### PlayerCamera : MonoBehaviour

Camera with perspective toggle (first-person / third-person) that handles spherical gravity orientation.

**Camera Perspectives:**
| Perspective | Position | Player Model | Use Case |
|-------------|----------|--------------|----------|
| Third-Person | Behind/above player | Visible | Navigation, awareness |
| First-Person | At head height | Shadow-only | Detail examination, immersion |

**Inspector Fields:**
```csharp
[Header("Follow (Third-Person)")]
[SerializeField] Transform target;
[SerializeField] float followDistance = 5f;
[SerializeField] float followHeight = 2f;
[SerializeField] float followSmoothing = 10f;

[Header("First-Person")]
[SerializeField] Vector3 _firstPersonOffset = new Vector3(0f, 1.6f, 0f);
[SerializeField] float _perspectiveTransitionTime = 0.3f;

[Header("Model Visibility")]
[SerializeField] Renderer[] _playerRenderers;

[Header("Look")]
[SerializeField] float lookSensitivity = 2f;
[SerializeField] float minPitch = -40f;
[SerializeField] float maxPitch = 70f;

[Header("Alignment")]
[SerializeField] float upAlignmentSpeed = 5f;
```

**Behavior:**
- Camera "up" smoothly aligns to player's `LocalUp`
- Horizontal rotation orbits around player (third-person) or rotates player (first-person)
- Vertical rotation pitches within limits
- No roll (prevents nausea)
- V key toggles between perspectives with smooth transition
- Player model hidden in first-person but shadow preserved

### InputReader : ScriptableObject

Decouples input from consumers. Single source of truth for input state.

**Player Properties:**
```csharp
public Vector2 MoveInput { get; }      // WASD/Left Stick (normalized)
public Vector2 LookInput { get; }      // Mouse/Right Stick
public bool SprintHeld { get; }        // Shift held
```

**Ship Properties:**
```csharp
public Vector2 ShipThrustInput { get; }   // X=strafe, Y=forward
public float ShipVerticalInput { get; }    // -1 down, +1 up
public Vector2 ShipLookInput { get; }      // X=yaw, Y=pitch
public float ShipRollInput { get; }        // Q/E roll
public bool ShipBrakeHeld { get; }
public bool ShipBoostHeld { get; }
```

**Player Events:**
```csharp
public event Action OnJump;
public event Action OnInteract;           // Used for boarding/interactions
public event Action OnPause;
public event Action OnToggleCameraView;   // V key - perspective toggle
```

**Ship Events:**
```csharp
public event Action OnShipExit;      // Tab to disembark
```

**Action Map Switching:**
```csharp
public void EnablePlayerInput();
public void EnableShipInput();
public void DisableAllInput();
```

## Movement Algorithm

### Grounding

```
1. Cast sphere downward along -LocalUp
2. If hit within groundCheckDistance → grounded
3. Store ground normal for slope handling
4. If slope angle > maxSlopeAngle → sliding state
```

### Movement (Grounded)

```
1. Get move input in camera-relative space
2. Project onto ground plane (tangent to LocalUp)
3. Apply acceleration toward desired velocity
4. Clamp to walkSpeed or runSpeed
5. Move via Rigidbody.MovePosition (kinematic-like control)
```

### Movement (Airborne)

```
1. Reduced input influence (airControl multiplier)
2. Gravity applied via GravitySolver
3. No ground snapping
4. No thrust control (player helpless in zero-g)
```

### Zero-G Behavior

Player is intentionally helpless in zero-g zones:
- No thrust or movement control
- Must use ship for zero-g traversal
- Gravity alignment resumes when entering gravity field

### Up Alignment

```
1. Get gravity direction from GravitySolver
2. Target rotation: current forward, new up = -gravityDir
3. Slerp toward target at upAlignmentSpeed
4. Apply to both player and camera (smoothed differently)
```

## State Transitions

```
OnFoot ─────────────────────────────────────────┐
  │ (lose ground)                               │
  ▼                                             │
Falling ───────────────────────────────────────┤
  │ (touch ground)                              │
  └─────────────────────────────────────────────┘
  
OnFoot ────(board ship)───────▶ InShip
InShip ────(exit ship)────────▶ OnFoot

OnFoot ────(interact)─────────▶ Interacting
Interacting ─(complete)───────▶ OnFoot

OnFoot ────(enter gate)───────▶ Transitioning
Transitioning ─(arrive)───────▶ OnFoot
```

## Edge Cases

1. **Walking over planet horizon** – LocalUp changes gradually, no pop
2. **Jumping near gravity boundary** – May float if leaving field (intended)
3. **Landing on steep slope** – Slide down, don't snap to surface
4. **Camera during fast rotation** – Smoothing prevents whip
5. **Boarding ship while falling** – Allowed, snaps to ship interior
6. **Zero-g zones** – Player floats helplessly, must use ship
7. **Perspective toggle during transition** – Blocked until transition completes
8. **Ship boarding in first-person** – Resets to third-person for ship camera

## Testing Checklist

- [x] Walk in all directions relative to camera
- [x] Jump and land smoothly
- [x] Walk over planet curvature without popping
- [x] Camera follows without nausea-inducing rotation
- [x] State machine transitions work correctly
- [x] Input is disabled during transitions (fade in/out)
- [x] Boarding/disembarking works with fade transitions
- [ ] V key toggles camera perspective (M4)
- [ ] Smooth perspective transition (M4)
- [ ] Player model hidden in first-person, shadow preserved (M4)
- [ ] Player helpless in zero-g zones (M4)

## Performance Notes

- Ground check uses single SphereCast, not multiple raycasts
- Up alignment uses Slerp, not Quaternion.LookRotation per frame
- No per-frame allocations in movement code

## Dependencies

- `Game.Core` (base classes)
- `Game.Gravity` (GravitySolver)

## Files

```
Scripts/Player/
├── CharacterMotorSpherical.cs
├── PlayerStateController.cs
├── PlayerCamera.cs
└── InputReader.cs (ScriptableObject)
```

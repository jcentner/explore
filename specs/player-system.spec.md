# Player System Specification

**Status: 🟡 PARTIALLY IMPLEMENTED (Milestone 1)**

## Implementation Notes

| Component | File | Status |
|-----------|------|--------|
| `CharacterMotorSpherical` | `Scripts/Player/CharacterMotorSpherical.cs` | ✅ Complete |
| `PlayerCamera` | `Scripts/Player/PlayerCamera.cs` | ✅ Complete |
| `InputReader` | `Scripts/Player/InputReader.cs` | ✅ Complete |
| `PlayerInitializer` | `Scripts/Player/PlayerInitializer.cs` | ✅ Complete |
| `PlayerStateController` | - | ❌ Deferred (Milestone 2+) |

### Known Limitations
- No state machine yet (OnFoot/InShip/etc.) - deferred until ship system
- Camera collision avoidance fields exist but not fully utilized
- No slope sliding logic

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

Third-person camera that handles spherical gravity orientation.

**Inspector Fields:**
```csharp
[Header("Follow")]
[SerializeField] Transform target;
[SerializeField] float followDistance = 5f;
[SerializeField] float followHeight = 2f;
[SerializeField] float followSmoothing = 10f;

[Header("Look")]
[SerializeField] float lookSensitivity = 2f;
[SerializeField] float minPitch = -40f;
[SerializeField] float maxPitch = 70f;

[Header("Alignment")]
[SerializeField] float upAlignmentSpeed = 5f;
```

**Behavior:**
- Camera "up" smoothly aligns to player's `LocalUp`
- Horizontal rotation orbits around player
- Vertical rotation pitches within limits
- No roll (prevents nausea)

### InputReader : ScriptableObject

Decouples input from consumers. Single source of truth for input state.

**Properties:**
```csharp
public Vector2 MoveInput { get; }
public Vector2 LookInput { get; }
public bool JumpPressed { get; }
public bool InteractPressed { get; }
public bool BoardPressed { get; }
```

**Events:**
```csharp
public event Action OnJump;
public event Action OnInteract;
public event Action OnBoard;
public event Action OnPause;
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
```

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

## Testing Checklist

- [x] Walk in all directions relative to camera
- [x] Jump and land smoothly
- [x] Walk over planet curvature without popping
- [x] Camera follows without nausea-inducing rotation
- [ ] State machine transitions work correctly (not yet implemented)
- [ ] Input is ignored during Transitioning state (not yet implemented)

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

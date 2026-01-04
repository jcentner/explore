# Ship System Specification

**Implementation Status: ✅ Core Complete (Milestone 2)**

## Purpose

Handle ship flight, boarding/disembarking, and space traversal between celestial bodies.

## Design Goals

- **Arcade feel** – Responsive, not simulation-heavy ✅
- **Smooth transitions** – Boarding/exiting feels seamless ✅
- **Optional stabilization** – Auto-level for casual play, manual for control (deferred)
- **Gravity interaction** – Can enable/disable gravity response ✅

## Components

### ShipController : MonoBehaviour

Core flight controller.

**Inspector Fields:**
```csharp
[Header("Thrust")]
[SerializeField] float mainThrustForce = 20f;
[SerializeField] float boostMultiplier = 2f;
[SerializeField] float reverseMultiplier = 0.5f;

[Header("Rotation")]
[SerializeField] float pitchSpeed = 60f;   // degrees/sec
[SerializeField] float yawSpeed = 45f;
[SerializeField] float rollSpeed = 90f;

[Header("Stabilization")]
[SerializeField] bool autoStabilize = true;
[SerializeField] float stabilizeSpeed = 2f;
[SerializeField] float velocityDamping = 0.98f; // per FixedUpdate when idle

[Header("Gravity")]
[SerializeField] bool respondToGravity = true;
[SerializeField] float gravityMultiplier = 0.5f; // Ships less affected
```

**Dependencies:**
- Requires `Rigidbody` (useGravity: false)
- Requires `GravitySolver` (if gravity response enabled)

**Key Methods:**
```csharp
public void SetThrustInput(float input);      // -1 to 1 (reverse to forward)
public void SetRotationInput(Vector3 input);  // pitch, yaw, roll
public void ToggleBoost(bool active);
public void ToggleStabilize(bool active);
public void ToggleGravity(bool active);

public bool IsLanded { get; }
public bool IsBoosting { get; }
public Vector3 Velocity { get; }
```

### ShipBoardingZone : MonoBehaviour

Trigger zone for boarding the ship.

**Inspector Fields:**
```csharp
[SerializeField] ShipController ship;
[SerializeField] Transform exitPoint;      // Where player appears on exit
[SerializeField] Transform pilotSeat;      // Where player sits (visual)
[SerializeField] float boardingDuration = 0.5f;
```

**Behavior:**
1. Player enters trigger + presses Board
2. Disable player controller, play boarding animation
3. Parent player to ship (or hide)
4. Enable ship controls
5. On exit: reverse process, place at exitPoint

### ShipCamera : MonoBehaviour

Camera behavior when piloting ship.

**Inspector Fields:**
```csharp
[Header("Follow")]
[SerializeField] Transform target;  // Ship transform
[SerializeField] Vector3 offset = new Vector3(0, 3, -10);
[SerializeField] float followSmoothing = 5f;

[Header("Look")]
[SerializeField] bool freeLook = false;  // Future: look around while flying
[SerializeField] float lookRange = 60f;

[Header("Effects")]
[SerializeField] float fovBoostAmount = 10f;  // FOV increase when boosting
[SerializeField] float fovSmoothSpeed = 5f;
```

### ShipLandingGear : MonoBehaviour

Handles landing detection and surface attachment.

**Inspector Fields:**
```csharp
[SerializeField] Transform[] landingPoints;  // Raycast origins
[SerializeField] float landingCheckDistance = 2f;
[SerializeField] LayerMask landableSurfaces;
[SerializeField] float maxLandingAngle = 30f;  // Degrees from surface normal
```

**States:**
| State | Description |
|-------|-------------|
| `Flying` | In space, full flight control |
| `Hovering` | Near surface, gear deployed |
| `Landed` | On surface, engines off, can exit |

## Flight Model

### Thrust

```
1. Get thrust input (-1 to 1)
2. If boosting: multiply by boostMultiplier
3. If negative: multiply by reverseMultiplier
4. Apply force in ship's forward direction
5. Clamp max velocity (optional, for feel)
```

### Rotation

```
1. Get rotation input (pitch, yaw, roll)
2. Multiply by respective speeds * deltaTime
3. Apply as local rotation
4. If autoStabilize and no input:
   - Gradually align ship "up" with dominant gravity "up"
   - Dampen angular velocity
```

### Gravity Response

```
if (respondToGravity && GravitySolver.CurrentGravity != Vector3.zero)
{
    Vector3 gravityForce = GravitySolver.CurrentGravity * gravityMultiplier;
    Rigidbody.AddForce(gravityForce, ForceMode.Acceleration);
}
```

### Auto-Stabilization

```
if (autoStabilize && rotationInput == Vector3.zero)
{
    // Dampen angular velocity
    angularVelocity *= stabilizeDamping;
    
    // Optionally align up with gravity
    if (respondToGravity)
    {
        Vector3 targetUp = -GravitySolver.CurrentGravity.normalized;
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, stabilizeSpeed * dt);
    }
}
```

## Boarding Sequence

```
Player presses Board near ship:
1. Lock player input
2. Play boarding animation/lerp
3. Disable PlayerStateController (set to InShip)
4. Enable ShipController input
5. Activate ShipCamera
6. Ship is now controlled

Player presses Exit while landed:
1. Lock ship input
2. Check exit point is clear
3. Play exit animation/lerp
4. Enable PlayerStateController (set to OnFoot)
5. Position player at exitPoint
6. Deactivate ShipCamera, activate PlayerCamera
```

## State Transitions

```
Flying ────(approach surface)────▶ Hovering
Hovering ─(touch down gently)───▶ Landed
Landed ───(thrust up)───────────▶ Hovering
Hovering ─(gain altitude)───────▶ Flying

Landed ───(player exits)────────▶ (Ship idle, player OnFoot)
(Ship idle) ──(player boards)───▶ Flying/Landed (depends on state)
```

## Edge Cases

1. **Board while ship moving** – Not allowed, ship must be landed
2. **Exit while in space** – Not allowed (no EVA in MVP)
3. **Ship enters different gravity field** – Smooth transition, may need to re-stabilize
4. **Crash into surface** – Take damage (future), bounce/slide for now
5. **Ship destroyed** – Respawn system (future), for now just don't crash hard

## Testing Checklist

- [x] Ship responds to thrust in all directions
- [x] Rotation feels responsive but not twitchy
- [ ] Auto-stabilize levels ship when no input (deferred)
- [x] Can land on planet surface smoothly
- [x] Boarding/exiting works seamlessly
- [x] Camera transitions between player and ship views
- [x] Ship responds to planet gravity when near surface

## Performance Notes

- Landing gear raycasts only when near surfaces (distance check first)
- Physics interpolation enabled for smooth visuals
- No per-frame allocations

## Dependencies

- `Game.Core` (base classes)
- `Game.Gravity` (GravitySolver)
- `Game.Player` (for boarding integration)

## Files

```
Scripts/Ship/
├── ShipController.cs       ✅ Implemented (~270 lines)
├── ShipBoardingTrigger.cs  ✅ Implemented (~300 lines, was ShipBoardingZone)
├── ShipCamera.cs           ✅ Implemented (~100 lines)
├── ShipInput.cs            ✅ Implemented (~160 lines, implements IPilotable)
└── Game.Ship.asmdef        ✅ References: Core, Gravity, Player, UI, InputSystem

Scripts/Player/
├── PlayerState.cs          ✅ Implemented (enum)
├── PlayerStateController.cs ✅ Implemented (~280 lines)
└── IPilotable.cs           ✅ Implemented (interface)

Scripts/UI/
├── BoardingPrompt.cs       ✅ Implemented (~140 lines)
└── InteractionPromptUI.cs  ✅ Implemented (~110 lines)
```

## Future Considerations (Not MVP)

- Ship damage and repair
- Multiple ship types
- Ship upgrades/customization
- Fuel system
- Autopilot/waypoints

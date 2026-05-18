# Tiny Wizard

Third-person controller with Unity's NEW Input System, Rigidbody physics, third-person camera, and a giant kitchen environment.

**Unity 6000.3.11f1** | URP | NEW Input System

---

## Setup

1. Open `Assets/Scenes/SampleScene.unity`
2. Menu: **Tiny Wizard** → **Setup Scene** (creates Player hierarchy, floor, camera, light, and CameraPivot)
3. Menu: **Tiny Wizard** → **Create Animator Controller** (creates `Assets/Animations/PlayerController.controller`)
4. Select Player in Hierarchy → assign `Assets/Input/TinyWizardControls.inputactions` to PlayerInput's Actions field
5. Select Player → child Model → assign `PlayerController.controller` to its Animator
6. Press Play

---

## Controls

| Control | Action |
|---------|--------|
| W/A/S/D | Move / Strafe |
| Shift | Sprint (hold) |
| Space | Jump |
| Mouse | Look around |
| Scroll Wheel | Zoom camera |

---

## Project Structure

```
Assets/
├── Input/
│   └── TinyWizardControls.inputactions    ← All input bindings
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs            ← Movement, rotation, animator integration
│   │   ├── PlayerInputHandler.cs          ← Event-driven input bridge
│   │   └── GroundChecker.cs               ← Multi-raycast ground detection
│   ├── Camera/
│   │   └── ThirdPersonCamera.cs           ← Smooth follow, mouse orbit, zoom
│   └── Editor/
│       ├── TinyWizardSceneSetup.cs        ← Auto-setup scene hierarchy
│       ├── AnimatorSetupHelper.cs         ← Creates animator controller
│       └── GiantKitchenBuilder.cs         ← Spawns ~50 kitchen props
├── Animations/
│   └── PlayerController.controller        ← Animator state machine
├── Prefabs/
│   └── Player.prefab                      ← After scene setup
└── Scenes/
    └── SampleScene.unity                  ← Main scene
```

---

## Architecture

The controller follows a modular, event-driven architecture:

- **PlayerInputHandler** — single dependency on `PlayerInput`. Caches 8 InputAction references. Dispatches continuous input (Move, Look, Sprint, Zoom) as properties and one-shot actions (Jump, Interact, CastSpell, Pause) as C# events.
- **PlayerController** — subscribes to input events. Uses Rigidbody physics (no CharacterController). Camera-relative movement. SmoothDamp rotation. Ground check via GroundChecker.
- **GroundChecker** — 3 raycasts in a circle, `rb.position`-based, 50ms grounded buffer, visual gizmos.
- **ThirdPersonCamera** — Standalone CameraPivot GameObject (not child of camera). Configurable sensitivity, distance, zoom range, and follow damping.

No component reads `Input` or `InputActions` directly — all input flows through PlayerInputHandler.

---

## Key Features

- **Rigidbody physics** — proper gravity, forces, interpolation, freeze rotation
- **Camera-relative movement** — WASD moves relative to camera facing direction
- **NEW Input System** — event-driven, not polling. Keyboard, mouse, and gamepad
- **Ground detection** — multi-raycast with configurable count, radius, and distance
- **Animator integration** — smooth speed/velocity blending, jump/fall states
- **Modular architecture** — zero coupling between PlayerController, PlayerInputHandler, and GroundChecker
- **Giant Kitchen Builder** — editor tool to spawn a low-poly kitchen (player is mouse-scale)

---

## Inspector Settings

### PlayerController
| Field | Default | Description |
|-------|---------|-------------|
| Move Speed | 5 | Normal walk speed |
| Sprint Speed | 8 | Sprint multiplier |
| Acceleration | 20 | How quickly target speed is reached |
| Jump Force | 5 | Jump impulse |
| Rotation Smooth Time | 0.1 | Character rotation smoothing |
| Rotate To Face Movement | true | Auto-rotate toward movement direction |
| Air Control | 0.3 | Movement control while airborne |

### GroundChecker
| Field | Default | Description |
|-------|---------|-------------|
| Raycast Distance | 0.2 | How far down to detect ground |
| Raycast Count | 3 | Number of raycasts in a circle |
| Raycast Radius | 0.3 | Spread of raycast positions |
| Ground Layer | Everything | Layers considered ground |

### ThirdPersonCamera
| Field | Default | Description |
|-------|---------|-------------|
| Sensitivity | (4, 2) | Mouse look sensitivity (X, Y) |
| Distance | 5 | Camera distance from pivot |
| Zoom Smooth | 10 | Zoom interpolation speed |
| Follow Smooth | 10 | Position follow damping |
| Min/Max Zoom | 2 / 10 | Camera distance limits |
| Y Clamp | -20 / 80 | Vertical look angle limits |

---

## Troubleshooting

### Character tips over
Freeze rotation must be set on Rigidbody → Constraints → Freeze Position X/Y/Z checked. PlayerController does this automatically in `OnEnable`.

### Won't move with WASD
- Verify PlayerInput has `TinyWizardControls.inputactions` assigned
- Check PlayerInputHandler logs a warning at startup if actions are null

### Jump doesn't work
- Verify GroundChecker shows green spheres near the ground
- Ensure ground is in the configured Ground Layer mask (default = Everything)

### No animations
- Ensure Animator Controller is assigned to the child **Model** GameObject, not the Player root
- Animator must have `speed`, `velocity`, `isGrounded`, `jump`, `land` parameters

### Small Y-position oscillation while moving
Known micro-bounce from capsule-continuous collision. Not visually noticeable at gameplay distance.

---

## What's Next

- **Spell casting system** — add CastSpell action binding
- **Stamina / mana** — resource bars with UI
- **Enemy AI** — simple chase and attack
- **Footstep SFX** — AnimationEvent-driven sounds
- **Dust particles** — landing / sprinting VFX

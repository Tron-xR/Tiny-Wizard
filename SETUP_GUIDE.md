# Tiny Wizard Third-Person Controller Setup Guide

## Overview
Complete modular third-person controller system using Rigidbody physics and the NEW Unity Input System.

### Architecture
```
Player (Root)
├── Model (Visual representation)
├── CameraTarget (Camera follow point)
└── GroundCheck (Ground detection point)
```

### Components
- **PlayerController.cs** - Core movement and physics logic
- **PlayerInputHandler.cs** - Input System integration
- **GroundChecker.cs** - Ground detection using raycasts
- **PlayerControls.inputactions** - Input mapping configuration

---

## Step 1: Import Assets

1. Create these scripts in your project:
   - `GroundChecker.cs`
   - `PlayerInputHandler.cs`
   - `PlayerController.cs`

2. Import the Input Actions asset:
   - Create file: `PlayerControls.inputactions`
   - Copy the JSON content provided

3. **IMPORTANT**: Open `PlayerControls.inputactions` in Unity:
   - The asset will auto-generate a C# class named `PlayerControls`
   - Close and reopen the file if needed (Unity will serialize it)

---

## Step 2: Scene Setup

### Create Player Hierarchy

1. Create an empty GameObject named `Player`
2. Add child GameObjects:
   - `Model` - Contains your wizard mesh/sprites
   - `CameraTarget` - Position at eye level (0, 0.6, 0)
   - `GroundCheck` - Position at base of model (0, 0.1, 0)

3. Adjust positions in Inspector:
   - **Model**: Position (0, 0, 0), Scale (1, 1, 1)
   - **CameraTarget**: Position (0, 0.6, 0) - Adjust height to eye level
   - **GroundCheck**: Position (0, 0.1, 0) - Slightly above ground

---

## Step 3: Rigidbody Configuration

### Player Object Settings

1. Add **Rigidbody** component to `Player` root
2. Configure in Inspector:

```
Mass: 1
Drag: 5 (will be overridden dynamically)
Angular Drag: 0.05
Use Gravity: ✓ (checked)
Freeze Rotation: X, Y, Z (IMPORTANT - prevents tipping)
Collision Detection: Dynamic (or Continuous if fast movement)
Body Type: Dynamic
```

**Key Points:**
- **DO freeze Y rotation** - Character rotation handled by script
- **DO freeze X and Z rotation** - Prevents tipping over
- Drag is set dynamically (5 on ground, 0.1 in air)

---

## Step 4: Collider Setup

### Add Capsule Collider

1. Add **Capsule Collider** to `Player` root
2. Configure:

```
Radius: 0.3
Height: 1.8
Direction: Y-Axis
Center: (0, 0.9, 0) - Adjust so bottom touches ground
Material: Friction 0, Bounciness 0
```

**Optional: Additional Colliders**

- Add smaller colliders to `Model` for better hit detection (set as triggers)
- Keep main capsule as non-trigger for physics

---

## Step 5: Animator Configuration

### Create Animator Controller

1. Create Animator Controller: `Assets/Animations/PlayerController.controller`
2. Create Animation States:
   - **Idle** - Standing still animation
   - **Walk** - Movement animation
   - **Run** - Sprint animation
   - **Jump** - Jump takeoff animation
   - **Fall** - Falling animation

3. Add Parameters (right panel):

| Parameter | Type | Default |
|-----------|------|---------|
| Speed | Float | 0 |
| IsGrounded | Bool | true |
| JumpTrigger | Trigger | - |

### Animation Transitions

**Idle ↔ Walk/Run**
- Idle → Walk/Run: `Speed > 0.1`
- Walk/Run → Idle: `Speed < 0.05`

**Walk → Run**
- Walk → Run: `Speed > 4`
- Run → Walk: `Speed < 3`

**Jump State**
- Any → Jump: `JumpTrigger` (is true, auto reset)
- Jump → Fall: `!IsGrounded`

**Fall State**
- Jump → Fall: `!IsGrounded`
- Fall → Idle: `IsGrounded == true`

**Transitions Settings:**
- Transition Duration: 0.1s
- Exit Time: 0.8-0.9s for locomotion

---

## Step 6: PlayerInput Component Setup

### Add PlayerInput to Player

1. Add **PlayerInput** component to `Player` root object
2. Configure:

```
Input Actions: PlayerControls (drag the .inputactions asset)
Default Control Scheme: Keyboard&Mouse
Default Action Map: Player
UI Input Module: (leave empty if no UI)
Camera: (optional - for reference)
Notification Behavior: Invoke Unity Events (if debugging)
```

**OnDeviceLost/OnControlsChanged:**
- Leave as default or handle in your pause menu

---

## Step 7: Script Component Setup

### PlayerController Script

Add **PlayerController** to `Player` root:

| Field | Type | Default |
|-------|------|---------|
| **References** | | |
| Rb | Rigidbody | (auto-cached) |
| Animator | Animator | (auto-cached) |
| Ground Checker | GroundChecker | (drag from below) |
| Input Handler | PlayerInputHandler | (auto-cached) |
| Camera Transform | Transform | (auto-cached) |
| **Movement** | | |
| Move Speed | float | 5 |
| Acceleration | float | 20 |
| Deceleration | float | 15 |
| Air Acceleration | float | 8 |
| Air Deceleration | float | 3 |
| **Sprint** | | |
| Sprint Speed | float | 8 |
| Sprint Acceleration | float | 25 |
| **Jump** | | |
| Jump Force | float | 5 |
| Jump Cooldown | float | 0.1 |
| Air Drag | float | 0.1 |
| Ground Drag | float | 5 |
| **Rotation** | | |
| Rotation Smooth Time | float | 0.1 |
| Use Character Rotation | bool | true |
| **Animation** | | |
| Speed Animation Smooth Time | float | 0.1 |

### PlayerInputHandler Script

Add **PlayerInputHandler** to `Player` root:

| Field | Type | Default |
|-------|------|---------|
| Player Input | PlayerInput | (drag PlayerInput component) |

### GroundChecker Script

Add **GroundChecker** to `Player` root:

| Field | Type | Default |
|-------|------|---------|
| **Ground Detection** | | |
| Raycast Distance | float | 0.2 |
| Ground Layer | LayerMask | Ground (layer 6) |
| Raycast Offset | Vector3 | (0, 0, 0) |
| **Ground Settings** | | |
| Raycast Count | int | 3 |
| Raycast Radius | float | 0.3 |

---

## Step 8: Layer Setup

### Create Ground Layer

1. Create new Layer: `Ground` (typically Layer 6)
2. Assign to:
   - All floor/platform objects
   - Terrain colliders
   - Static environment

3. Set GroundChecker Ground Layer to `Ground`

---

## Step 9: Camera Setup

### Option A: Third-Person Follow Camera (Recommended)

Create a separate camera controller script or use existing follow camera system:

```csharp
public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform target; // CameraTarget child
    [SerializeField] private float distance = 3f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float smoothTime = 0.3f;
    
    private void LateUpdate()
    {
        // Position camera behind and above target
        Vector3 targetPos = target.position - transform.forward * distance + Vector3.up * height;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / smoothTime);
        transform.LookAt(target);
    }
}
```

### Option B: First-Person Camera

1. Create `Camera` child under `CameraTarget`
2. Position at (0, 0, 0) relative to CameraTarget
3. Attach a mouse look script (separate from controller)

---

## Step 10: Testing

### Play Mode Verification

1. **Movement**: WASD moves character smoothly
2. **Rotation**: Character faces movement direction
3. **Sprint**: Shift key increases speed
4. **Jump**: Space key causes jump, gravity applies
5. **Grounding**: Check Gizmos in Scene view (green = grounded, red = airborne)
6. **Animation**: Speed parameter animates, IsGrounded toggles

### Debug Output

Add console logging to verify:
```csharp
Debug.Log($"Speed: {playerController.GetCurrentSpeed()}, Grounded: {playerController.IsGrounded}");
```

---

## Step 11: Prefab Creation

### Convert to Prefab

1. Right-click `Player` in Hierarchy
2. Select **Prefab** → **Create Original**
3. Configure overrides if needed
4. Save to: `Assets/Prefabs/Player.prefab`

---

## Performance Optimization Tips

1. **Use Continuous Collision Detection** only for fast-moving objects
2. **Optimize Ground Layer**: Use single static collider instead of many
3. **Animation Blending**: Use 1D blending for Speed parameter
4. **Animator Caching**: References are cached in OnEnable()
5. **Input Actions**: Uses cached action references (no string lookups each frame)

---

## Troubleshooting

### Character Falls Through Ground
- Verify Rigidbody Collision Detection is `Dynamic` or `Continuous`
- Check Collider radius/height encompasses visual model
- Ensure ground objects have colliders
- Test: Add a simple cube as ground, assign Ground layer

### Movement Not Working
- Check PlayerInput component is on Player root
- Verify Input Actions asset is assigned to PlayerInput
- Ensure Keyboard&Mouse control scheme is active
- Check Player action map in Input Actions has Move bound to WASD

### Character Tips Over
- **CRITICAL**: Freeze Rotation X, Y, Z on Rigidbody
- Increase collider radius if too narrow

### Jump Not Working
- Verify GroundChecker shows green Gizmo when standing
- Check Jump layer in GroundChecker points to correct ground
- Ensure jump cooldown hasn't been exceeded
- Verify "Space" binding in Input Actions

### Animation Not Playing
- Confirm Animator assigned to PlayerController
- Verify animation parameters exist in Animator Controller
- Check transitions have correct conditions
- Set Animator Update Mode: `Normal` (not Unscaled)

### Camera Issues
- Ensure `cameraTransform` reference is set (or Main Camera exists)
- For third-person: Create separate camera follow script
- Verify CameraTarget child is positioned correctly (0, 0.6, 0)

### Input System Not Responding
- **NEW Input System must be enabled**: Edit → Project Settings → Input System Package
- PlayerInput component must use NEW Input System, not Old
- Verify Action Map name matches: "Player"
- Check control scheme is active in device

---

## Inspector Quick Reference

### Recommended Defaults

**For Fast-Paced Movement:**
- Move Speed: 7
- Sprint Speed: 11
- Acceleration: 25
- Sprint Acceleration: 30

**For Slow, Deliberate Movement:**
- Move Speed: 3
- Sprint Speed: 5
- Acceleration: 15
- Jump Force: 4

**For High-Flying Jumps:**
- Jump Force: 8
- Air Deceleration: 1.5

**For Responsive Controls:**
- Rotation Smooth Time: 0.05
- Speed Animation Smooth Time: 0.05

---

## Input System Debugging

### View Active Inputs (Editor Only)

Windows → Analysis → Input Debugger

This shows real-time input values and helps debug control issues.

---

## Next Steps

1. Add footstep sounds to GroundChecker.IsGrounded state change
2. Implement stamina system for sprint
3. Add particle effects on landing
4. Create interaction system for NPCs/objects
5. Add ledge climbing or wall slides
6. Implement damage/knockback
7. Add ability system for spell casting

---

## File Locations

```
Assets/
├── Scripts/
│   ├── PlayerController.cs
│   ├── PlayerInputHandler.cs
│   └── GroundChecker.cs
├── Input/
│   └── PlayerControls.inputactions
├── Animations/
│   └── PlayerController.controller
├── Prefabs/
│   └── Player.prefab
└── Scenes/
    └── GameScene.unity
```

---

## Version Info

- Unity Version: 2021.3 LTS or newer
- Input System: 1.4.0 or newer
- Physics: Built-in
- Scripting Backend: Mono or IL2CPP

---

**Created for Tiny Wizard Controller System**

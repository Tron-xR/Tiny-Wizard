# Quick Start Checklist

Complete this checklist in order to get your Tiny Wizard controller working in 15 minutes.

---

## ✓ Phase 1: Scripts & Assets (2 minutes)

- [ ] Copy `GroundChecker.cs` to your project
- [ ] Copy `PlayerInputHandler.cs` to your project
- [ ] Copy `PlayerController.cs` to your project
- [ ] Create `PlayerControls.inputactions` with provided JSON
- [ ] Double-click `PlayerControls.inputactions` to open editor
- [ ] Close the editor (Unity auto-serializes it)

**Status**: Scripts imported ✓

---

## ✓ Phase 2: Scene Hierarchy (3 minutes)

1. Create GameObject hierarchy:
   ```
   Player (root)
   ├── Model (cube or your mesh)
   ├── CameraTarget
   └── GroundCheck
   ```

2. Set positions:
   - Player: (0, 0, 0)
   - Model: (0, 0, 0), scale (1, 1, 1)
   - CameraTarget: (0, 0.6, 0)
   - GroundCheck: (0, 0.1, 0)

3. Create ground floor:
   - Plane positioned at Y: -1
   - Scale: (10, 1, 10) or larger
   - Assign Ground layer

**Status**: Hierarchy created ✓

---

## ✓ Phase 3: Physics Setup (2 minutes)

**On Player Root:**

1. Add Rigidbody:
   - Mass: 1
   - Drag: 5
   - Angular Drag: 0.05
   - Use Gravity: ✓
   - **Freeze Rotation: X, Y, Z** ← IMPORTANT

2. Add Capsule Collider:
   - Radius: 0.3
   - Height: 1.8
   - Center: (0, 0.9, 0)

3. Create Ground Layer:
   - Layer 6: "Ground"
   - Assign to floor plane

**Status**: Physics configured ✓

---

## ✓ Phase 4: Components (3 minutes)

**Add to Player root, in this order:**

1. **PlayerInput** component:
   - Input Actions: PlayerControls asset
   - Control Scheme: Keyboard&Mouse
   - Action Map: Player

2. **PlayerInputHandler** component:
   - Player Input: (drag PlayerInput from above)

3. **GroundChecker** component:
   - Ground Layer: Ground
   - Raycast Distance: 0.2
   - Raycast Count: 3

4. **PlayerController** component:
   - Rigidbody: (auto-cached)
   - Animator: (leave empty for now)
   - Ground Checker: (drag GroundChecker)
   - Input Handler: (auto-cached)
   - Camera Transform: (auto-cached)

**Status**: All components added ✓

---

## ✓ Phase 5: Animator Setup (4 minutes)

1. Create Animator Controller: `PlayerController.controller`
2. Assign to Model's Animator component
3. Add parameters:
   - Speed (Float, default 0)
   - IsGrounded (Bool, default true)
   - JumpTrigger (Trigger)

4. Create states:
   - Idle → Walk → Run
   - Jump
   - Fall

5. Basic transitions:
   - Idle ↔ Walk: Speed > 0.1 / Speed < 0.05
   - Walk ↔ Run: Speed > 0.6 / Speed < 0.5
   - Any → Jump: JumpTrigger
   - Jump → Fall: !IsGrounded
   - Fall → Idle: IsGrounded

6. Assign animations (or use placeholder cubes)

**Status**: Animator configured ✓

---

## ✓ Phase 6: Test! (1 minute)

1. Press Play in editor
2. Test controls:
   - [ ] WASD moves character
   - [ ] Character faces movement direction
   - [ ] Shift makes character move faster
   - [ ] Space makes character jump
   - [ ] Character falls with gravity
3. Check ground detection:
   - [ ] In Scene view, ground check shows green gizmo
   - [ ] Stays green while touching ground
   - [ ] Turns red while in air

**Status**: Basic movement working ✓

---

## ✓ Advanced Setup (Optional)

Once basic movement works, add:

- [ ] Third-person camera follow script
- [ ] Footstep sound effects
- [ ] Jump particle effects
- [ ] Landing impact effects
- [ ] Stamina/energy system
- [ ] Spell casting system
- [ ] Enemy interactions

---

## ⚠️ If Something Isn't Working

### "Character won't move"
```
1. Check PlayerInput component exists
2. Verify Input Actions asset assigned
3. Play scene and check Input Debugger (Windows → Analysis)
4. Add debug logs to PlayerInputHandler.OnMovePerformed()
```

### "Character tips over"
```
1. CRITICAL: Check Rigidbody Freeze Rotation X, Y, Z
2. Increase capsule collider radius
```

### "Jump doesn't work"
```
1. Check ground detection shows green gizmo
2. Verify GroundChecker Ground Layer is set
3. Ensure Space key binding exists in Input Actions
```

### "Animations don't play"
```
1. Verify Animator assigned to PlayerController
2. Check animator parameters exist (Speed, IsGrounded, JumpTrigger)
3. Verify transitions have correct conditions
4. Add debug to UpdateAnimations() to verify values change
```

### "Input System errors"
```
1. Edit → Project Settings → Input System Package
2. MUST use NEW Input System, not old Input Manager
3. Verify PlayerInput component uses new system
4. Regenerate PlayerControls.inputactions
```

---

## 📁 File Checklist

All files should be in your project:

```
Assets/
├── Scripts/
│   ├── PlayerController.cs
│   ├── PlayerInputHandler.cs
│   └── GroundChecker.cs
│
├── Input/
│   └── PlayerControls.inputactions
│
└── Documentation/
    ├── SETUP_GUIDE.md ← Full detailed guide
    ├── ANIMATOR_SETUP.md ← Animation reference
    ├── INPUT_ACTIONS_GUIDE.md ← Input System details
    └── QUICK_START.md ← This file
```

---

## 🎮 Control Reference

| Action | Input |
|--------|-------|
| Move Forward | W |
| Move Left | A |
| Move Right | D |
| Move Backward | S |
| Sprint | Hold Shift |
| Jump | Space |
| Look | Mouse Move (if implemented) |

---

## 📊 Performance Targets

- **CPU**: ~0.2ms per update (bare minimum)
- **Physics**: ~0.5ms (Rigidbody + ground check)
- **Animation**: ~0.3ms (parameter updates)
- **Input**: ~0.01ms (cached actions)

**Total**: ~1.0ms per frame (on modern hardware)

---

## 🔧 Inspector Defaults (Copy These Values)

### PlayerController
```
Move Speed: 5
Acceleration: 20
Deceleration: 15
Air Acceleration: 8
Air Deceleration: 3
Sprint Speed: 8
Sprint Acceleration: 25
Jump Force: 5
Jump Cooldown: 0.1
Air Drag: 0.1
Ground Drag: 5
Rotation Smooth Time: 0.1
Speed Animation Smooth Time: 0.1
```

### GroundChecker
```
Raycast Distance: 0.2
Raycast Count: 3
Raycast Radius: 0.3
```

---

## 🎯 Next Steps After Basic Setup

1. **Camera System**
   - Create camera follow script
   - Position behind character
   - Smooth damping

2. **Animation Polish**
   - Import humanoid animations
   - Add blend trees
   - Refine transitions

3. **Sound Design**
   - Add footsteps
   - Add jump sound
   - Add landing impact

4. **Visual Effects**
   - Dust particles on landing
   - Sprint trail effect
   - Jump impact dust

5. **Game Systems**
   - Add abilities/spells
   - Enemy AI targeting
   - Interaction system

---

## 💡 Tips & Tricks

- **Tuning Movement**: Increase `acceleration` for snappier feel
- **Floaty Jump**: Increase `Jump Force` for higher jumps
- **Slippery Ground**: Decrease `Ground Drag` (ice effect)
- **Responsive Rotation**: Decrease `Rotation Smooth Time` (0.05 = super snappy)
- **Smooth Animation**: Use 1D Blend Tree instead of discrete Walk/Run states
- **Camera Shake**: Attach CameraTarget shake script for impact feedback

---

## ❓ FAQ

**Q: Can I use a CharacterController instead?**
A: No, this system uses Rigidbody exclusively for better physics and multiplayer support.

**Q: Can I use old Input.GetAxis?**
A: No, this requires NEW Input System. Project Settings must enable it.

**Q: How do I add gamepad support?**
A: Add control scheme to PlayerControls.inputactions with Gamepad bindings. Code unchanged.

**Q: Can I modify movement speed at runtime?**
A: Yes! All SerializeField values can be changed in code:
```csharp
playerController.moveSpeed = 10f;
```

**Q: How do I implement a stamina system?**
A: Track sprint time, reduce moveSpeed based on stamina, regenerate over time.

**Q: How do I add a dash/dodge ability?**
A: Add DashAction to Input Actions, apply impulse force in PlayerController.

---

## 📞 Support Resources

- **Full Setup Guide**: SETUP_GUIDE.md (comprehensive walkthrough)
- **Animator Details**: ANIMATOR_SETUP.md (animation blending & parameters)
- **Input System**: INPUT_ACTIONS_GUIDE.md (input system debugging)
- **Unity Input System Docs**: docs.unity3d.com/Manual/InputSystem.html
- **Input Debugger**: Windows → Analysis → Input Debugger

---

## ✅ Final Verification

Before marking complete:

- [ ] Play scene, press W → character moves
- [ ] Character rotates to face movement direction
- [ ] Press Shift → movement speed increases
- [ ] Press Space → character jumps and falls
- [ ] Multiple jumps are prevented mid-air
- [ ] Animation Speed parameter updates (watch Inspector)
- [ ] IsGrounded parameter toggles (watch Inspector)
- [ ] Scene view shows green gizmo on ground, red in air
- [ ] No console errors
- [ ] Scene saves without warnings

**Estimated time to completion: 15 minutes**

---

Created for **Tiny Wizard Controller System**

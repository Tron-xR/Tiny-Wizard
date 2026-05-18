# Animator Setup Quick Reference

## Parameters Required

Create these parameters in your Animator Controller:

1. **Speed** (Float)
   - Default: 0
   - Purpose: Controls walk/run blending
   - Updated every frame by PlayerController

2. **IsGrounded** (Bool)
   - Default: true
   - Purpose: Determines if in air/falling
   - Updated every frame by PlayerController

3. **JumpTrigger** (Trigger)
   - Purpose: Triggers jump animation
   - Automatically resets after fired

---

## Animation States & Transitions

### State Machine Structure

```
┌─────────────────────────────────────────┐
│              Any State                  │
│ (Higher layer priority)                 │
│                                         │
│  Jump (Trigger) ──→ Jump Animation     │
│  !IsGrounded ──→ Fall Animation        │
└─────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────┐
│         Grounded Movement Layer         │
│                                         │
│   Idle ←─→ Walk ←─→ Run                │
│                                         │
│ Idle: Speed = 0                         │
│ Walk: 0 < Speed ≤ 0.6                  │
│ Run: Speed > 0.6                        │
└─────────────────────────────────────────┘
```

### Detailed Transitions

#### Entry → Idle
- **Conditions**: None (default state)
- **Transition Duration**: 0.1s
- **Exit Time**: N/A

#### Idle → Walk
- **Condition**: `Speed > 0.1`
- **Transition Duration**: 0.1s
- **Exit Time**: 0.8
- **Interruption**: Can be interrupted

#### Walk → Run
- **Condition**: `Speed > 0.6`
- **Transition Duration**: 0.15s
- **Exit Time**: 0.8

#### Run → Walk
- **Condition**: `Speed < 0.5`
- **Transition Duration**: 0.15s
- **Exit Time**: 0.8

#### Walk → Idle
- **Condition**: `Speed < 0.05`
- **Transition Duration**: 0.2s
- **Exit Time**: 0.9

#### Run → Idle
- **Condition**: `Speed < 0.05`
- **Transition Duration**: 0.2s
- **Exit Time**: 0.9

#### Any State → Jump
- **Condition**: `JumpTrigger`
- **Transition Duration**: 0.1s
- **Exit Time**: N/A
- **Interruption**: Cannot interrupt jump in progress

#### Jump → Fall
- **Condition**: `!IsGrounded` (NOT IsGrounded)
- **Transition Duration**: 0.05s
- **Exit Time**: 0.7

#### Fall → Idle
- **Condition**: `IsGrounded == true` AND `Speed < 0.1`
- **Transition Duration**: 0.1s
- **Exit Time**: 0.9

#### Fall → Walk
- **Condition**: `IsGrounded == true` AND `Speed > 0.1`
- **Transition Duration**: 0.1s
- **Exit Time**: 0.9

---

## Animation Clips Required

Create or import these animations:

1. **Idle** (Loop)
   - Standing still pose
   - Duration: 1-2 seconds (looping)

2. **Walk** (Loop)
   - Walking forward animation
   - Duration: 0.6-1s per stride (looping)
   - Speed parameter affects playback speed

3. **Run** (Loop)
   - Running/sprinting animation
   - Duration: 0.4-0.6s per stride (looping)

4. **Jump** (Non-Loop)
   - Jump takeoff
   - Duration: 0.5-0.7s
   - Should NOT loop (exits to Fall)

5. **Fall** (Loop)
   - Falling pose
   - Duration: 1s loop (looping)
   - Idle falling pose repeated

---

## Animation Blending Strategy

### 1D Blending (Recommended for Speed)

Instead of separate Walk/Run states, you can use a Blend Tree:

1. Create new **1D Blend Tree** under Idle state
2. Name it "MovementBlend"
3. Add parameters:
   - Position 0: Idle clip (Speed = 0)
   - Position 0.5: Walk clip (Speed = 0.5)
   - Position 1.0: Run clip (Speed = 1.0)
4. Connect Speed parameter
5. Smooth transitions automatic based on Speed value

**Benefits:**
- Smoother animation transitions
- Fewer states to manage
- Better for varied speeds

### 2D Blending (For Directional Movement)

For strafing/directional movement:

```
Create 2D Blend Tree with parameters:
- Horizontal: InputX value
- Vertical: InputY value (Speed)

Add animations in 4 corners:
- Idle (0, 0)
- Walk Forward (0, 0.5)
- Run Forward (0, 1.0)
- Strafe Left (-1, 0)
- Strafe Right (1, 0)
```

---

## Animator Settings

### Configuration

- **Update Mode**: Normal
- **Culling Type**: Based on Renderers (optional optimization)
- **Animator Driver**: ✓ (Optimize Game Objects)

### Performance Tips

1. Use simplified skeletons for fall/idle states
2. Bake animations into optimal curves
3. Use Animator.SetFloat() instead of SetBool() for smooth blending
4. Disable unneeded animator layers

---

## Code Integration Verification

### What PlayerController Does

```csharp
// Every frame in UpdateAnimations():
animator.SetFloat("Speed", animationSpeed);           // 0-1 value
animator.SetBool("IsGrounded", groundChecker.IsGrounded);  // true/false

// On jump:
animator.SetTrigger("JumpTrigger");                  // Fires once
```

### Verify in Inspector

1. Play scene
2. Select Player in Hierarchy
3. Look at Animator component
4. Watch parameters update in real-time
5. Verify animations play correctly

---

## Common Animation Issues & Fixes

### Animation Doesn't Play
- **Fix**: Check condition logic is correct (use `Speed >` not `Speed ==`)
- **Fix**: Verify animation is assigned to state (drag clip into state)
- **Fix**: Check transition duration isn't too long (0.1-0.2s typical)

### Animation Stutters
- **Fix**: Increase transition duration smoothing
- **Fix**: Check animation loop settings (should loop for Walk/Run/Idle)
- **Fix**: Verify animation clip hasn't been edited with broken keyframes

### Smooth Locomotion Not Working
- **Fix**: Use 1D Blend Tree instead of separate Walk/Run states
- **Fix**: Ensure animation clips have smooth curves (no sharp transitions)
- **Fix**: Enable motion matching if available (2022 LTS+)

### Jump Animation Cuts Short
- **Fix**: Don't set Exit Time too low (default 0.7 is good)
- **Fix**: Jump clip duration should match jump arc timing
- **Fix**: Verify Fall state has proper entry from Jump

### Character Gets Stuck in Fall Loop
- **Fix**: Add transition from Fall → Idle/Walk with `IsGrounded == true`
- **Fix**: Ensure GroundChecker is detecting ground (check Gizmos)
- **Fix**: Set Fall loop to shorter duration (0.5-1s)

---

## Animation Event Setup (Optional)

Add footstep sounds or particle effects:

1. Open animation clip in Dopesheet/Timeline
2. Right-click on timeline → Add Event
3. Create script with method:
   ```csharp
   public void PlayFootstep()
   {
       audioSource.PlayOneShot(footstepClip);
   }
   ```
4. Assign method to animation event

---

## Exporting Animations from Blender

If creating custom animations:

1. **Export Settings**:
   - Format: FBX
   - Bake Animation: ✓
   - Action to All Pushed to NLA: ✓
   - Add Leaf Bones: ✗ (usually)

2. **Import into Unity**:
   - Animation Type: Humanoid or Generic
   - Animation Compression: Optimal
   - Loop Time: ✓ (for looping animations)

3. **Split Animations**:
   - In Animation tab, create clips from Timeline frames
   - Set frame ranges for each animation

---

## Humanoid vs Generic

### Use Humanoid if:
- Using humanoid skeleton (human-like proportions)
- Want muscle/curve editing
- Using humanoid animation clips
- Standard biped character

### Use Generic if:
- Custom non-humanoid skeleton (wizard animals, robots)
- Tiny Wizard is stylized/non-standard
- Animation retargeting not needed
- Custom bone setup

---

## Final Checklist

- [ ] All 3 parameters exist: Speed (Float), IsGrounded (Bool), JumpTrigger (Trigger)
- [ ] All animation clips created/imported
- [ ] Transitions have correct conditions
- [ ] Transition durations set to 0.1-0.2s
- [ ] Jump → Fall transition has `!IsGrounded` condition
- [ ] Fall → Ground transitions check IsGrounded
- [ ] Walk/Run states check Speed thresholds (not exact values)
- [ ] Idle is set as default state
- [ ] Any State layer has Jump trigger
- [ ] Test in Play mode: animations change with input
- [ ] Test falling: animations play correctly
- [ ] Test jump: animation fires then transitions to Fall

---

**Reference: PlayerController Updates Parameters in UpdateAnimations()**

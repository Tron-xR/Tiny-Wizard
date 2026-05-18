# Tiny Wizard - Step-by-Step Integration (In Unity)

Follow this exact process in the Unity Editor.

---

## Phase 1: Open Project (1 minute)

1. Open Unity Hub
2. Select "Tiny Wizard" project
3. Wait for project to load
4. You should see `Assets` folder with:
   - `Input/` folder (with PlayerControls.inputactions)
   - `Scripts/Player/` folder (with 3 controller scripts)
   - `Scripts/Editor/` folder (with setup helpers)
   - `Scenes/` folder (with SampleScene.unity)

✅ **Project loaded**

---

## Phase 2: Create Animator Controller (2 minutes)

### Step 1: Open Menu
1. Top menu bar → **Tiny Wizard** dropdown
2. Click **Create Animator Controller**

```
Expected result:
- Dialog says "Animator Controller created at: Assets/Animations/PlayerController.controller"
- Click OK
```

✅ **Animator Controller created**

### Step 2: Verify File
1. In Project window, expand `Assets` → `Animations`
2. You should see: `PlayerController.controller`

✅ **File verified**

---

## Phase 3: Setup Scene (3 minutes)

### Step 1: Open Scene
1. Go to `Assets` → `Scenes` → double-click `SampleScene.unity`
2. The scene will open in Hierarchy and Scene view

### Step 2: Run Scene Setup
1. Top menu bar → **Tiny Wizard** dropdown
2. Click **Setup Scene**

```
Expected result:
- Many GameObjects appear in Hierarchy:
  • Player (root)
  • Player/Model (cube visual)
  • Player/CameraTarget
  • Player/GroundCheck
  • Ground (floor plane)
  • Directional Light
  • Main Camera
- Dialog says setup complete
- Click OK
```

✅ **Scene setup complete**

### Step 3: Verify Hierarchy
In Hierarchy window, you should see:

```
SampleScene
├── Player
│   ├── Model (cube)
│   ├── CameraTarget
│   └── GroundCheck
├── Ground (plane, Y: -1)
├── Directional Light
└── Main Camera (Main Camera tag)
```

✅ **Hierarchy verified**

---

## Phase 4: Assign Input Actions (2 minutes)

### Step 1: Select Player
1. In Hierarchy, click on **Player** (the root object)
2. In Inspector, you'll see many components

### Step 2: Find PlayerInput Component
1. Scroll down in Inspector until you find **PlayerInput**
2. It should be near the bottom of the component list

### Step 3: Assign Input Actions Asset
1. In PlayerInput component, find the field: **Input Actions**
2. Click the circle icon next to it (object picker)
3. Search for "PlayerControls"
4. Click on `PlayerControls` (in Assets/Input/)
5. Click Open/Select

```
Expected result:
- Input Actions field now shows: PlayerControls
```

### Step 4: Verify Settings
In PlayerInput component, verify:

| Setting | Should Be |
|---------|-----------|
| Input Actions | PlayerControls ✓ |
| Default Control Scheme | Keyboard&Mouse ✓ |
| Default Action Map | Player ✓ |

✅ **Input Actions assigned and verified**

---

## Phase 5: Assign Animator Controller (2 minutes)

### Step 1: Select Model
1. In Hierarchy, expand **Player**
2. Click on **Player → Model** (the cube)
3. In Inspector, find **Animator** component

### Step 2: Assign Controller
1. In Animator component, find field: **Controller**
2. It currently shows "None"
3. Click the circle icon (object picker)
4. Search for "PlayerController"
5. Select `PlayerController.controller` (in Assets/Animations/)
6. Click Select

```
Expected result:
- Animator Controller field now shows: PlayerController
- The field is no longer empty
```

✅ **Animator Controller assigned**

---

## Phase 6: Configure Ground Layer (2 minutes)

### Step 1: Select Player
1. In Hierarchy, click **Player** (root)

### Step 2: Find GroundChecker
1. In Inspector, scroll down to find **GroundChecker** component
2. Expand it if needed

### Step 3: Set Ground Layer
1. Find the field: **Ground Layer**
2. Click the dropdown (currently blank or "Nothing")
3. Select **Ground** from the list

```
Expected result:
- Ground Layer now shows: Ground
```

### Step 4: Verify Other Settings
In GroundChecker, verify these values:

| Setting | Value |
|---------|-------|
| Raycast Distance | 0.2 |
| Raycast Count | 3 |
| Raycast Radius | 0.3 |

✅ **GroundChecker configured**

---

## Phase 7: Verify All Components (3 minutes)

### Select Player and Verify Each Component

**Rigidbody:**
- [x] Mass: 1
- [x] Drag: 5
- [x] Angular Drag: 0.05
- [x] Use Gravity: ✓ (checked)
- [x] Freeze Rotation: X, Y, Z all ✓ (THIS IS CRITICAL!)

**Capsule Collider:**
- [x] Radius: 0.3
- [x] Height: 1.8
- [x] Center: (0, 0.9, 0)

**PlayerInput:**
- [x] Input Actions: PlayerControls
- [x] Default Control Scheme: Keyboard&Mouse
- [x] Default Action Map: Player

**PlayerInputHandler:**
- [x] Player Input: PlayerInput (component)

**GroundChecker:**
- [x] Ground Layer: Ground
- [x] Raycast Distance: 0.2
- [x] Raycast Count: 3
- [x] Raycast Radius: 0.3

**PlayerController:**
- [x] Most fields auto-assigned
- [x] Move Speed: 5
- [x] Sprint Speed: 8
- [x] Jump Force: 5

✅ **All components verified!**

---

## Phase 8: Test in Play Mode (2 minutes)

### Step 1: Save Scene
1. File → Save (or Ctrl+S)
2. You should see no errors in Console

### Step 2: Start Play Mode
1. Click **Play** button (top of editor)
2. Scene starts running

### Step 3: Test Controls

| Control | Action | Result |
|---------|--------|--------|
| **W** | Press | Character moves forward |
| **A** | Press | Character strafes left |
| **D** | Press | Character strafes right |
| **S** | Press | Character moves backward |
| **Shift** | Hold | Movement speed increases (sprint) |
| **Space** | Press | Character jumps up and falls |
| **Shift+Space** | Hold+Press | Sprint + Jump (combined) |

### Step 4: Verify Gizmos
1. Look at **Scene** view (top right toggle)
2. When character on ground:
   - Green line shows at character feet ✓
3. When character in air (after jump):
   - Red line shows ✓

### Step 5: Watch Inspector
1. Select Player in Hierarchy
2. Watch PlayerController component in Inspector:
   - Ground Checker → Is Grounded toggles true/false
   - Player Controller → Current Speed changes with movement

✅ **All working!**

### Step 6: Exit Play Mode
1. Click **Play** button again to stop
2. Return to edit mode

✅ **Testing complete!**

---

## Phase 9: Create Player Prefab (1 minute)

### Step 1: Select Player
1. In Hierarchy, right-click on **Player** (root)
2. Select → **Prefabs** → **Create Original**

```
Expected result:
- Player object turns blue in Hierarchy
- Indicates it's now a prefab instance
```

### Step 2: Verify Prefab Created
1. In Project window, go to `Assets` → `Prefabs`
2. You should see: `Player.prefab`

✅ **Prefab created!**

---

## Phase 10: Final Checklist

Before considering setup complete, verify:

### Scene Setup
- [x] Player hierarchy exists (Player → Model, CameraTarget, GroundCheck)
- [x] Ground plane exists at Y: -1
- [x] Directional Light exists
- [x] Main Camera exists

### Physics
- [x] Player has Rigidbody with freeze rotation
- [x] Player has Capsule Collider
- [x] Ground plane has Collider
- [x] Ground layer created and assigned

### Input System
- [x] PlayerInput component assigned Input Actions
- [x] Control Scheme is Keyboard&Mouse
- [x] Action Map is Player

### Animator
- [x] PlayerController.controller assigned to Model's Animator
- [x] Parameters exist: Speed, IsGrounded, JumpTrigger
- [x] States exist: Idle, Walk, Run, Jump, Fall

### Functionality
- [x] WASD moves character
- [x] Shift increases speed
- [x] Space makes character jump
- [x] No console errors
- [x] Gizmos show ground detection

### Files
- [x] Scripts in Assets/Scripts/Player/
- [x] Input Actions in Assets/Input/
- [x] Animator in Assets/Animations/
- [x] Prefab in Assets/Prefabs/

✅ **All items checked - Implementation complete!**

---

## Troubleshooting During Setup

### "Can't find Tiny Wizard menu"
- **Fix**: Scripts might not have compiled
- Close and reopen Unity
- Check Console for errors (red text)

### "Input Actions dropdown is empty"
- **Fix**: PlayerControls.inputactions might not be imported
- Double-click PlayerControls.inputactions in Project
- Close the editor window
- Wait 1-2 seconds, try again

### "Character tips over when moving"
- **CRITICAL**: Rigidbody Freeze Rotation not set!
- Select Player → Rigidbody
- Check: Freeze Rotation X, Y, Z (all THREE must be checked)
- Save and test again

### "No movement when pressing WASD"
- Check PlayerInput component has Input Actions assigned
- Open Input Debugger: Windows → Analysis → Input Debugger
- Press W, should see values change in debugger
- If no values: Input System may not be enabled (rare)

### "Jump doesn't work"
- Ground Checker might not see ground
- In Scene view, look at character feet
- Should see green gizmo when touching ground
- If red: Ground layer might not be set correctly

---

## Next Steps After Setup

1. **Test with actual player model**
   - Replace Model cube with your wizard mesh
   - Keep same transforms and collider size

2. **Add Camera Following**
   - Create separate CameraFollower script
   - Position camera behind player

3. **Add Sound Effects**
   - Footsteps on ground
   - Jump sound
   - Landing impact

4. **Add Visual Polish**
   - Dust particles on landing
   - Sprint effect
   - Jump effect

5. **Balance Gameplay**
   - Adjust movement speed for feel
   - Fine-tune jump height
   - Tweak acceleration/deceleration

---

## Console Verification

After setup, Console should show:

```
No errors (completely clean)
```

If you see red text errors:
1. Read the error message
2. It will tell you exactly what's missing
3. Fix the mentioned component
4. Save and try again

---

## Performance Check

After setup, performance should be:

- Frame rate: 60+ FPS (modern hardware)
- CPU: <1ms per frame for controller
- No stuttering or lag

If experiencing lag:
1. Check Console for errors
2. Verify Collision Detection is set correctly
3. Reduce Raycast Count in GroundChecker (to 1)

---

**Setup Status: COMPLETE ✅**

Your Tiny Wizard controller is ready!

Next: Play around, test it out, then start adding your wizard-specific features.

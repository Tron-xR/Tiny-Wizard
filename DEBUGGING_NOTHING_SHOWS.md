# Tiny Wizard - Nothing Shows Up on Screen - Troubleshooting

## Quick Diagnostic Checklist

### Step 1: Check Scene View vs Game View

1. **In Unity Editor, look at the top right of the viewport**
2. You should see two tabs:
   - `Scene` (currently selected probably)
   - `Game` (what you see when you Press Play)

**Test this:**
- [ ] Click on **Scene** tab - do you see the Player (cube) and Ground (plane)?
- [ ] Click on **Game** tab - is it black/empty?

If Scene shows objects but Game doesn't → **Camera problem**

---

### Step 2: Check Main Camera

1. In Hierarchy, find **Main Camera**
2. In Inspector, check these settings:

```
Position: Should be around (0, 1, -5)
Rotation: Should be around (0, 0, 0)
Clear Flags: Skybox (or Solid Color)
Background Color: Not black (if Solid Color)
```

**What to verify:**
- [ ] Main Camera exists in Hierarchy
- [ ] Main Camera has "MainCamera" tag
- [ ] Main Camera position makes sense
- [ ] Position is NOT inside the Player or below ground

---

### Step 3: Check Player Position

1. In Hierarchy, select **Player**
2. In Inspector, check Transform:

```
Position X: 0
Position Y: Should be > 0 (above ground which is at Y: -1)
Position Z: 0
```

**It should be around:** (0, 1, 0) or higher

---

### Step 4: Check Ground Floor

1. In Hierarchy, select **Ground**
2. In Inspector, check:

```
Position Y: -1 (below Player)
Position X: 0
Position Z: 0
Scale: 10, 1, 10 (or larger)
```

**Should be visible as a large plane**

---

### Step 5: Check Lighting

1. In Hierarchy, look for **Directional Light**
2. If missing:
   - Right-click in Hierarchy
   - Create Empty → Light → Directional Light
   - Position it above the scene
   - Set intensity to 1.0

**Without light, everything appears black!**

---

## Most Common Causes

### Issue #1: Camera Outside Scene
**Solution:**
1. Select **Main Camera** in Hierarchy
2. In Inspector, set Transform Position to:
   - X: 0
   - Y: 2
   - Z: -5
3. Press Play

---

### Issue #2: No Lighting
**Solution:**
1. In Hierarchy, find or create **Directional Light**
2. Set Rotation to approximately:
   - X: 50
   - Y: -30
   - Z: 0
3. Set Intensity to 1
4. Press Play

---

### Issue #3: Player Below Ground or Camera
**Solution:**
1. Select **Player** in Hierarchy
2. Set Position Y to at least **1** or **2**
3. Ground should be at Y: **-1**
4. Camera should be at Y: **1 to 2**
5. Press Play

---

## Debug This Right Now

### Step-by-Step Test:

1. **Open SampleScene**
   - Go to Assets/Scenes/SampleScene.unity

2. **Switch to Scene View**
   - Top right: Click "Scene" tab
   - Do you see: Player (cube), Ground (plane), Directional Light, Camera?

3. **If yes, proceed to step 4**
   - If no, the setup didn't work - restart with Tiny Wizard → Setup Scene

4. **Select Main Camera**
   - Hierarchy → Main Camera
   - Verify position and rotation

5. **Click Game tab**
   - Should show from camera's perspective

6. **Press Play**
   - Should see the scene

---

## What You Should See

In **Scene View** (before play):
```
✓ Cube (Player/Model) - tan/beige colored
✓ Plane (Ground) - gray colored, below player
✓ Light rays from Directional Light
✓ Small camera wireframe showing camera view
```

In **Game View** (after play):
```
✓ Lit scene with Player cube visible
✓ Ground plane visible
✓ Should be able to move with WASD
✓ No errors in Console
```

---

## Emergency Reset

If nothing shows up and you can't figure it out:

1. **Delete the scene content:**
   - Select all in Hierarchy (Ctrl+A)
   - Delete (Delete key)

2. **Create fresh scene:**
   - Menu: **Tiny Wizard → Setup Scene**
   - This recreates everything

3. **Then:**
   - Select Player → PlayerInput → Assign PlayerControls.inputactions
   - Select Player/Model → Animator → Assign PlayerController.controller
   - Press Play

---

## Tell Me Specifically

When you press Play and nothing shows:

1. **Can you see ANYTHING on screen?**
   - [ ] Completely black
   - [ ] Completely white
   - [ ] Blue sky (default)
   - [ ] Something else: ___________

2. **Does the Console show any errors?**
   - [ ] Yes (tell me what they say)
   - [ ] No errors

3. **Can you switch to Scene View and see objects?**
   - [ ] Yes, I see Player, Ground, Light
   - [ ] No, scene is also empty
   - [ ] Partially (some objects missing)

4. **When you press Play, can you move with WASD?**
   - [ ] Yes, character moves (just not visible)
   - [ ] No, nothing happens
   - [ ] Don't know

---

## Most Likely Solution

**9 out of 10 times, it's one of these:**

1. **Main Camera wrong position**
   - Set to: (0, 2, -5)

2. **No Directional Light**
   - Create one, set rotation to (50, -30, 0)

3. **Player below camera or outside view**
   - Set Player Position Y to 1 or 2

4. **Scene not saved after setup**
   - File → Save (Ctrl+S)
   - Then Press Play

---

## Quick Fix - Try This Now

1. Select **Main Camera** in Hierarchy
2. In Inspector Transform, set:
   ```
   Position: X=0, Y=2, Z=-5
   Rotation: X=0, Y=0, Z=0
   ```
3. Find **Directional Light** (create if missing):
   - Right-click Hierarchy → Light → Directional Light
   - Set Rotation: X=50, Y=-30, Z=0
   - Set Intensity: 1
4. Select **Player**, set Position Y to **1**
5. File → Save
6. Press Play

---

## Still Nothing?

Tell me:
1. What color is the screen when playing? (black, white, blue, etc.)
2. Can you see the Hierarchy objects change when you press WASD?
3. Any error messages in Console (red text)?
4. Did the "Setup Scene" menu command work without errors?

I'll help you fix it! 🔧

# Tiny Wizard - Manual Setup (NO Menu Required)

## Step 1: Create Player Object

1. In Hierarchy, right-click → **Create Empty**
2. Name it: **Player**

## Step 2: Add Components to Player

Select **Player** in Hierarchy, then click **Add Component** for each:

1. **Rigidbody**
   - Mass: 1
   - Drag: 5
   - Angular Drag: 0.05
   - Use Gravity: ✓
   - Freeze Rotation: X ✓, Y ✓, Z ✓ (ALL THREE CHECKED)
   - Collision Detection: Continuous

2. **Capsule Collider**
   - Radius: 0.3
   - Height: 1.8
   - Center: (0, 0.9, 0)

3. **PlayerInput** (type "PlayerInput" in search)
   - Input Actions: (assign PlayerControls.inputactions from Assets/Input/)
   - Default Control Scheme: Keyboard&Mouse
   - Default Action Map: Player

4. **Animator**
   - Controller: (leave None for now)

5. **PlayerInputHandler** (type "PlayerInputHandler" in search)
   - Player Input: (drag the PlayerInput component here)

6. **GroundChecker** (type "GroundChecker" in search)
   - Ground Layer: Nothing (assign later)

7. **PlayerController** (type "PlayerController" in search)
   - All fields auto-assign in OnEnable

**You should see 7 components on Player when done.**

---

## Step 3: Create Player Children

Right-click **Player** in Hierarchy for each:

1. **Model** (right-click Player → 3D Object → Cube, name it "Model")
   - Position: (0, 0, 0)
   - Scale: (0.6, 1.8, 0.6)
   - REMOVE the Box Collider from Model (click gear icon → Remove Component)

2. **CameraTarget** (right-click Player → Create Empty, name it "CameraTarget")
   - Position: (0, 0.6, 0)

3. **GroundCheck** (right-click Player → Create Empty, name it "GroundCheck")
   - Position: (0, 0.1, 0)

---

## Step 4: Create Ground

1. Right-click in Hierarchy → **3D Object** → **Plane**
2. Name it: **Ground**
3. Position: (0, -1, 0)
4. Scale: (10, 1, 10)

---

## Step 5: Create Light

1. Right-click in Hierarchy → **Light** → **Directional Light**
2. Rotation: (50, -30, 0)
3. Intensity: 1

---

## Step 6: Create Camera

1. Right-click in Hierarchy → **Camera**
2. Name it: **Main Camera**
3. Position: (0, 2, -5)
4. Tag: MainCamera (dropdown at top of Inspector)

---

## Step 7: Set Up Ground Layer

1. Click **Player** → **GroundChecker** component
2. **Ground Layer**: Click the Layer dropdown
   - If "Ground" is in the list, select it
   - If not: Click Layer dropdown → Add Layer... → type "Ground" in Layer 6 or higher

3. Click the **Ground** object in Hierarchy
4. Top of Inspector → **Layer** dropdown → select **Ground**

---

## Step 8: Assign Input Actions

1. Click **Player** in Hierarchy
2. Find **PlayerInput** component
3. Click the circle next to **Input Actions**
4. Pick: **PlayerControls** (from Assets/Input/)
5. Verify:
   - Default Control Scheme: Keyboard&Mouse ✓
   - Default Action Map: Player ✓

---

## Step 9: Verify Everything

**Hierarchy should look like this:**
```
SampleScene
├── Player (Rigidbody, Capsule, PlayerInput, Animator, etc.)
│   ├── Model (cube, no collider)
│   ├── CameraTarget
│   └── GroundCheck
├── Ground (Layer: Ground)
├── Directional Light
└── Main Camera (Tag: MainCamera)
```

## Step 10: Press Play

1. File → Save Scene
2. Press Play
3. You should see a cube on a ground plane
4. Test: W, A, S, D moves the cube
5. Shift = sprint, Space = jump

---

## If It Still Doesn't Work

Tell me:
1. After pressing Play, do you see the cube?
2. Does WASD move it?
3. Any red errors in Console?

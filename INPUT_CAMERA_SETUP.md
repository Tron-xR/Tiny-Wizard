# Input System & Camera Setup Guide

## Files Added

| File | Purpose |
|------|---------|
| `Assets/Input/TinyWizardControls.inputactions` | Input bindings (7 actions) |
| `Assets/Scripts/Player/PlayerInputHandler.cs` | Event-driven input layer |
| `Assets/Camera/ThirdPersonCamera.cs` | Follow camera with orbit/zoom/collision |

---

## PlayerInput Component Setup

1. Select **Player** in Hierarchy
2. Find **PlayerInput** component
3. Set **Input Actions** → `TinyWizardControls` (from Assets/Input/)
4. Verify:
   - Default Control Scheme: `Keyboard&Mouse`
   - Default Action Map: `Player`

**Action Map — Player:**

| Action | Type | Binding | Usage |
|--------|------|---------|-------|
| Move | Vector2 | W/A/S/D | Movement direction |
| Look | Vector2 | Mouse Delta | Camera orbit |
| Jump | Button | Space | Jump |
| Sprint | Button | Left Shift | Sprint |
| Interact | Button | E | Interact with objects |
| CastSpell | Button | Q / Left Click | Cast abilities |
| Pause | Button | Escape | Pause menu |

---

## Camera Hierarchy Setup

1. Create **Main Camera** (or rename existing):
   - Right-click Hierarchy → **Camera**
   - Name: `Main Camera`
   - Tag: `MainCamera`

2. Create **CameraPivot** child:
   - Right-click `Main Camera` → **Create Empty**
   - Name: `CameraPivot`
   - Position: `(0, 0, 0)`

3. Attach **ThirdPersonCamera** script:
   - Select `Main Camera`
   - Add Component → `ThirdPersonCamera`
   - Assign references:
     - **Target**: Drag `CameraTarget` (child of Player)
     - **Camera Pivot**: Drag `CameraPivot` (child of this camera)
     - **Input Handler**: Drag `Player` (has PlayerInputHandler)

**Hierarchy should look like:**
```
Main Camera
├── CameraPivot (empty, position 0,0,0)
```

---

## ThirdPersonCamera Inspector Defaults

| Field | Default | Notes |
|-------|---------|-------|
| Distance | 4 | How far behind player |
| Min Distance | 1 | Closest zoom |
| Max Distance | 8 | Farthest zoom |
| Height | 0.5 | Pivot above target |
| Sensitivity X | 100 | Horizontal look speed |
| Sensitivity Y | 60 | Vertical look speed |
| Vertical Min | -20 | Lowest camera angle |
| Vertical Max | 60 | Highest camera angle |
| Follow Smooth | 0.15 | Camera lag |
| Zoom Speed | 5 | Scroll sensitivity |
| Collision Radius | 0.3 | Camera obstacle check |
| Obstacle Layers | Everything | What blocks camera |

---

## Input Remapping

To change any key binding:
1. Double-click `TinyWizardControls.inputactions` in Project
2. Click the binding you want to change (e.g. "E" for Interact)
3. In the right panel, click the path field
4. Press the new key on your keyboard
5. Close the editor → Unity saves automatically

---

## Event-Driven Architecture

```
Input System → PlayerInputHandler → Game Systems
                                     ↕
                               Events / Properties

PlayerInputHandler exposes:
  - Properties:  MoveInput, LookInput, IsSprinting
  - Events:      OnJump, OnInteract, OnCastSpell, OnPause
                 JumpPressed, InteractPressed, CastSpellPressed, PausePressed
```

**How to use in your scripts:**

```csharp
// Subscribe in OnEnable
void OnEnable()
{
    GetComponent<PlayerInputHandler>().JumpPressed += HandleJump;
    GetComponent<PlayerInputHandler>().OnCastSpell.AddListener(CastSpell);
}

void HandleJump() { /* your jump logic */ }
void CastSpell() { /* your spell logic */ }

// Unsubscribe in OnDisable
void OnDisable()
{
    var handler = GetComponent<PlayerInputHandler>();
    if (handler != null)
        handler.JumpPressed -= HandleJump;
}
```

---

## Testing

1. Press **Play**
2. **W/A/S/D** → Move character
3. **Mouse move** → Orbit camera
4. **Scroll wheel** → Zoom in/out
5. **Space** → Jump
6. **Shift** → Sprint
7. **E** → Fires OnInteract event
8. **Q** or **Left Click** → Fires OnCastSpell event
9. **Escape** → Fires OnPause event
10. Walk near walls → Camera pushes in

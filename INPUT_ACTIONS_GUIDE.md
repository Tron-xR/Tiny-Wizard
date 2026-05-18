# Input Actions Visual Setup Guide

## Overview

The **PlayerControls.inputactions** file defines all input bindings for keyboard/mouse.

---

## Input Actions File Structure

```
PlayerControls (Root)
│
└── Player (Action Map)
    │
    ├── Move (Action - Value Type)
    │   ├── Binding: WASD Composite
    │   │   ├── W (up)
    │   │   ├── A (left)
    │   │   ├── S (down)
    │   │   └── D (right)
    │   └── Returns: Vector2 (-1 to 1 on each axis)
    │
    ├── Jump (Action - Button Type)
    │   ├── Binding: Spacebar
    │   └── Returns: bool (pressed/released)
    │
    ├── Sprint (Action - Button Type)
    │   ├── Binding: Left Shift
    │   └── Returns: bool (pressed/released)
    │
    └── Look (Action - Value Type)
        ├── Binding: Mouse Delta
        └── Returns: Vector2 (mouse movement)

Control Scheme: Keyboard&Mouse
    └── Devices: Keyboard, Mouse
```

---

## Input Actions Editor (Visual Walkthrough)

### Step 1: Open Input Actions Asset

1. Double-click `PlayerControls.inputactions` in Project window
2. This opens the Input Actions editor (visual UI)

### Step 2: Verify Actions

The editor shows:

**Left Panel: Action Maps**
```
Player
├── Move ← Vector2 input
├── Jump ← Button input
├── Sprint ← Button input
└── Look ← Vector2 input
```

**Middle Panel: Action Details**
```
Action Name: Move
Type: Value
Expected Control Type: Vector2
Processors: (none)
Interactions: (none)
```

**Right Panel: Bindings**
```
Move
├── WASD (Composite)
│   ├── W (up) → <Keyboard>/w
│   ├── A (left) → <Keyboard>/a
│   ├── S (down) → <Keyboard>/s
│   └── D (right) → <Keyboard>/d
```

### Step 3: Verify Bindings

For each action, check:

#### Move Action
- **Type**: Value (Vector2)
- **Binding**: WASD Composite
  - up: `<Keyboard>/w` ✓
  - down: `<Keyboard>/s` ✓
  - left: `<Keyboard>/a` ✓
  - right: `<Keyboard>/d` ✓

#### Jump Action
- **Type**: Button
- **Binding**: Space
  - Path: `<Keyboard>/space` ✓

#### Sprint Action
- **Type**: Button
- **Binding**: Left Shift
  - Path: `<Keyboard>/leftShift` ✓

#### Look Action
- **Type**: Value (Vector2)
- **Binding**: Mouse Delta
  - Path: `<Mouse>/delta` ✓

### Step 4: Verify Control Scheme

**Control Schemes Panel** (bottom right)
```
Keyboard&Mouse ← Current scheme
├── Keyboard (required)
└── Mouse (required)
```

---

## How PlayerInputHandler Reads These

### Action Reference Flow

```
PlayerControls.inputactions
        ↓
  Auto-generates PlayerControls.cs
        ↓
  PlayerInput component uses it
        ↓
  PlayerInputHandler reads actions:
  
  playerInput.actions["Move"]      → Vector2 WASD input
  playerInput.actions["Jump"]      → bool space press
  playerInput.actions["Sprint"]    → bool shift press
  playerInput.actions["Look"]      → Vector2 mouse delta
```

### Event System

```
PlayerInputHandler subscribes to:

moveAction.performed   → OnMovePerformed()   → updates moveInput
moveAction.canceled    → OnMoveCanceled()    → zeros moveInput

jumpAction.performed   → OnJumpPerformed()   → sets jumpInputPressed
sprintAction.performed → OnSprintPerformed() → sets sprintInputPressed
sprintAction.canceled  → OnSprintCanceled()  → clears sprintInputPressed

lookAction.performed   → OnLookPerformed()   → updates lookInput
lookAction.canceled    → OnLookCanceled()    → zeros lookInput
```

---

## Manual Binding Setup (If Needed)

If editing bindings in the visual editor:

### Add New Binding

1. Select action (e.g., "Move")
2. Click **"+ Add Binding"** button
3. Choose:
   - **Composite** (for multi-key combinations like WASD)
   - **Single Button** (for single key)
   - **Axis** (for analog input)

### Add to WASD Composite

1. Click on "WASD" composite row
2. Click **"+ Add Binding"** within composite
3. Assign key:
   - up → W
   - left → A
   - down → S
   - right → D

### Verify in Generated Code

The `.inputactions` file generates `PlayerControls.cs` containing:

```csharp
public class PlayerControls : IInputActionCollection2
{
    public PlayerActions @Player => new PlayerActions(this);
    
    // Player map actions:
    // input.Player.Move;
    // input.Player.Jump;
    // input.Player.Sprint;
    // input.Player.Look;
}
```

You **don't edit** this—it's auto-generated.

---

## Keyboard Input Reference

### Available Keyboard Keys

```
Navigation:     <Keyboard>/upArrow
                <Keyboard>/downArrow
                <Keyboard>/leftArrow
                <Keyboard>/rightArrow

WASD Movement:  <Keyboard>/w
                <Keyboard>/a
                <Keyboard>/s
                <Keyboard>/d

Modifiers:      <Keyboard>/leftShift
                <Keyboard>/rightShift
                <Keyboard>/leftCtrl
                <Keyboard>/rightCtrl
                <Keyboard>/leftAlt

Actions:        <Keyboard>/space
                <Keyboard>/enter
                <Keyboard>/backspace
                <Keyboard>/escape

Numbers:        <Keyboard>/0 through <Keyboard>/9

F-Keys:         <Keyboard>/f1 through <Keyboard>/f12
```

### Mouse Input Reference

```
Button 0 (Left):        <Mouse>/leftButton
Button 1 (Right):       <Mouse>/rightButton
Button 2 (Middle):      <Mouse>/middleButton

Position:               <Mouse>/position
Delta (Movement):       <Mouse>/delta
Scroll:                 <Mouse>/scroll
```

---

## Extension: Adding Gamepad Support

To add controller support, add control scheme:

1. Click **"+ Add Control Scheme"** in editor
2. Name: `Gamepad`
3. Add devices: `<Gamepad>`

Then modify bindings for gamepad:

```
Move Action - Add gamepad binding:
  Path: <Gamepad>/leftStick

Jump Action - Add gamepad binding:
  Path: <Gamepad>/buttonSouth (A button)

Sprint Action - Add gamepad binding:
  Path: <Gamepad>/leftTrigger

Look Action - Add gamepad binding:
  Path: <Gamepad>/rightStick
```

In code, PlayerInputHandler automatically receives these inputs when gamepad is connected.

---

## Debugging Input Actions

### Editor Debugging

1. Windows → Analysis → Input Debugger
2. Shows real-time input values
3. Test inputs while game running

### Console Logging

Add to PlayerInputHandler:

```csharp
private void OnMovePerformed(InputAction.CallbackContext context)
{
    moveInput = context.ReadValue<Vector2>();
    Debug.Log($"Move Input: {moveInput}");
}
```

### Visual Debugging in Scene

Add canvas UI to show input values:

```csharp
// In PlayerController or debug script:
debugText.text = $"Move: {inputHandler.MoveInput}\n" +
                 $"Sprint: {inputHandler.SprintPressed}\n" +
                 $"Jump: {inputHandler.JumpPressed}";
```

---

## Common Input Issues & Fixes

### Inputs Not Registering

1. **Check**: Input System package installed
   - Window → TextMesh Pro → Import Examples (updates Input System)
   - Or: Window → Input System Package → (latest version)

2. **Check**: NEW Input System enabled
   - Edit → Project Settings → Input System Package (not Input Manager)

3. **Check**: PlayerInput component exists on Player
   - Has Input Actions asset assigned
   - Control scheme is "Keyboard&Mouse"

4. **Check**: Action names match exactly
   - "Move" not "movement"
   - "Jump" not "jump"
   - Case-sensitive!

### WASD Not Working Specifically

1. Verify bindings in Input Actions editor:
   - Move action should be "Value" type (returns Vector2)
   - Has WASD Composite with W, A, S, D keys

2. Test individual keys:
   ```csharp
   if (Keyboard.current.wKey.isPressed) Debug.Log("W pressed");
   ```

3. Check control scheme includes Keyboard device

### Mouse Look Not Working

1. Verify Look action:
   - Type: Value (Vector2)
   - Binding: `<Mouse>/delta`

2. Ensure mouse is not locked (if implementing free look):
   ```csharp
   Cursor.lockState = CursorLockMode.None; // Unlock to see movement
   ```

---

## Performance Notes

- Input Actions are **cached** in PlayerInputHandler
- Action references retrieved once in OnEnable()
- Event-based (performed/canceled) instead of polling
- No string lookups per frame
- ~0.01ms overhead per controller

---

## Summary: What Happens

```
User presses W key
        ↓
Keyboard hardware event
        ↓
New Input System detects <Keyboard>/w
        ↓
Maps to PlayerControls Move action
        ↓
Move action.performed event fires
        ↓
PlayerInputHandler.OnMovePerformed() called
        ↓
moveInput = (0, 1, 0) ← direction vector
        ↓
PlayerController reads moveInput
        ↓
Calculates desired velocity
        ↓
Character moves forward
```

---

## Checklists

### Setup Verification

- [ ] PlayerControls.inputactions exists in project
- [ ] File shows "PlayerControls" at top (auto-generated class name)
- [ ] PlayerInput component on Player GameObject
- [ ] PlayerInput.inputActions points to PlayerControls asset
- [ ] PlayerInput.controlScheme set to "Keyboard&Mouse"
- [ ] PlayerInput.actionMapName set to "Player"
- [ ] PlayerInputHandler.playerInput assigned (or auto-cached)
- [ ] All 4 actions appear in PlayerInputHandler code
- [ ] Actions fire in code (add debug logs to verify)

### Input Testing

- [ ] Press W → character moves forward
- [ ] Press A/D → character strafes left/right
- [ ] Press S → character moves backward
- [ ] Hold Shift → character sprints (faster movement)
- [ ] Press Space → character jumps
- [ ] Move mouse → look input registers (if using Look action)

---

**Reference: PlayerInputHandler.cs integrates actions with PlayerController.cs**

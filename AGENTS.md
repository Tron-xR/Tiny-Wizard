<!-- UNITY CODE ASSIST INSTRUCTIONS START -->
- Project name: Tiny Wizard
- Unity version: Unity 6000.3.11f1
- Active game object:
  - Name: Model Cube
  - Tag: Untagged
  - Layer: Default

## Interaction System State (session end)

### Detection
- SphereCast (radius 0.5) + OverlapCapsule fallback for small objects
- Distance checks relative to **player** position, not camera (zoom-proof)
- `GetComponentInParent<IInteractable>()` — works on parent GameObjects
- Respects per-object `InteractionDistance`
- `maxInteractionDistance`: 5, `interactionRadius`: 0.5

### UI
- Prompt text hidden via alpha (`hiddenColor`) and empty string
- Visible color cached in `OnEnable`, restored on `ShowPrompt`

### Pickup (PickupObject)
- Snaps directly to HoldPoint (`localPosition = 0`, `localRotation = identity`)
- No offset from pickup position
- Prompt cached in `originalPromptText`, restored on drop
- Cached `mainCamera` instead of `Camera.main`

### Push (PushableObject)
- Collision ignored between player/cube while pushing (`Physics.IgnoreCollision`)
- Prompt cached/restored
- Cached `mainCamera` instead of `Camera.main`

### PlayerController
- `obstacleMask` field on SlideMove (set to exclude "Interactable" layer)
- Kinematic rigidbody

## Spell System State

### Architecture
- `SpellBase` (abstract) — cooldown, mana cost, cast delay, VFX/Audio hooks, virtual `StartCast`/`ExecuteCast`
- `PushSpell` — fires projectile or applies push force in a radius via `OverlapSphere` + `AddForce`
- `FreezeSpell` — raycast target, applies freeze via `FreezeableObject.Freeze()` + creates ice platform
- `BounceSpell` — raycast ground, spawns bounce pad, applies velocity override on `Rigidbody.linearVelocity`
- `SpellManager` — owns spell list, subscribes to `CastSpellPressed`/`SpellSlotPressed`, handles cooldown/mana/anim
- `SpellProjectile` — simple kinematic projectile with launch/hit callback
- `FreezeableObject` — freezes Rigidbody constraints, swaps material, spawns ice cover
- `ISpellTarget` — interface for `OnPushSpell`, `OnFreezeSpell`, `OnBounceSpell`

### Input
- SpellSlot1/2/3 actions in `TinyWizardControls.inputactions` (keys 1/2/3)
- `PlayerInputHandler.SpellSlotPressed` event (int index)
- Scroll wheel (via `ZoomInput`) cycles spells in `SpellManager.HandleSpellSwitching()`

### Cast Flow
- `PlayerInputHandler.CastSpellPressed` → `SpellManager.OnCastSpellPressed` → `ActiveSpell.TryCast(origin, direction)`
- `TryCast` checks cooldown + mana, calls `StartCast` (VFX, SFX, mana cost, anim trigger), then `ExecuteCast` after `castDelay`
- `ExecuteCast` is abstract — each spell implements its own targeting/effect logic

### Prefabs Created (Auto-Setup)
- `Assets/Prefabs/SpellProjectile.prefab` — kinematic RB, trigger collider, SpellProjectile script
- `Assets/Prefabs/BouncePad.prefab` — flat pad with MeshFilter/Renderer (assign material)
- `Assets/Prefabs/IcePlatform.prefab` — flat pad with MeshFilter/Renderer (assign material)

### Editor Setup Script
- `Assets/Scripts/Editor/SpellSetupWindow.cs` — `Tools > Spell System Setup` menu
  - Button: "Add SpellManager to Player" — adds manager + wires inputHandler/animator
  - Button: "Create Spell Child Objects" — creates Push/Freeze/Bounce child GOs + assigns to list
  - Button: "Create CastOrigin on Model" — creates wand-tip transform at front of Model Cube

### Setup Checklist (Unity Editor)
1. Open `Tools > Spell System Setup` and click all 3 buttons
2. Assign `SpellProjectile` prefab to `PushSpell.projectilePrefab` in Inspector
3. Assign `BouncePad` prefab to `BounceSpell.bouncePadPrefab`
4. Assign `IcePlatform` prefab to `FreezeSpell.icePlatformPrefab`
5. Add animator controller to Player's Animator component (for cast animation)
6. Add `FreezeableObject` to Mug, Toaster, Bread, etc (any Rigidbody object)
7. For NPCs/enemies: implement `ISpellTarget` interface on their scripts
8. (Optional) Toggle `useMana` on SpellManager, assign VFX/SFX on each spell

### Notes
- BounceSpell reads `Rigidbody.linearVelocity` (Unity 6 API) — PlayerController uses kinematic, so Bounce only affects non-kinematic Rigidbodies unless `applyToPlayer` is true
- Mana system is opt-in (`useMana` toggle on SpellManager)
- Spell switching via scroll wheel uses ZoomInput delta — works alongside camera zoom because zoom reads the same axis

### Known issues to fix later
- No auto-stop push on look-away (commented out)
- `InteractionUI` uses legacy `Text` (not TMP)
- `SpellManager.HandleSpellSwitching()` conflicts with camera zoom — both use scroll wheel
<!-- UNITY CODE ASSIST INSTRUCTIONS END -->
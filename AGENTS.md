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

## Enemy AI System State

### Architecture
- `EnemyBase` (abstract) — common references, state machine integration, movement setup
- `EnemyStateMachine` (non-MonoBehaviour, serializable) — `Idle/Patrol/Chase/Attack/ReturnToPatrol/Dead` states, enter/exit events
- `EnemyPatrol` — waypoint array, ordered/random patrol, wait timer, gizmo visualization
- `EnemyDetection` — sphere radius + LoS raycast + optional FOV cone, chase memory timer
- `EnemyAttack` — `OverlapSphere` hit detection, cooldown, animation event hooks (`OnAttackHit()`, `OnAttackFinished()`)
- `EnemyHealth` — max health, hit flash (material instance), death event, destroy delay
- `EnemyAnimator` — manages `Speed`/`IsChasing`/`AttackTrigger`/`HitTrigger`/`IsDead` params
- `EnemyAudioHandler` — SFX + loop sources, detection/attack/hit/death/footstep sounds, pitch variation
- `EnemyVFXHandler` — hit/death/attack/detection particle systems, auto-destroy
- `FlyingEnemyMovement` — height control, hover oscillation, direct flying toward target
- `SpiderWallMovement` — surface normal alignment, wall/ceiling raycasting (visual illusion)

### Concrete Enemy Types
- `CockroachEnemy` — fast ground, chaotic movement, charge attack
- `SpiderEnemy` — ambush, lunge attack, optional web projectile
- `FlyEnemy` — flying patrol, circles player, dive-bomb attack

### Behaviour Flow
```
Idle → Patrol → DetectPlayer → Chase → Attack → LosePlayer → ReturnToPatrol → Patrol
```

### Scripts Location
- `Assets/Scripts/Enemy/` (all 14 files)

### Prefab Hierarchy
```
EnemyRoot (EnemyBase + NavMeshAgent + Rigidbody (kinematic) + Collider)
├── Visuals (child with MeshFilter/MeshRenderer + Animator)
├── DetectionPoint (empty Transform)
├── AttackPoint (empty Transform)
├── VFXRoot (empty Transform)
└── AudioRoot (empty Transform)
```

### Setup Checklist (Unity Editor)
1. Create layers: `Enemy`, `Player`, `DetectionObstacle`
2. Bake NavMesh (Ground + Environment layers, not Enemy/Player)
3. Create enemy prefabs following hierarchy above
4. Assign all script components on each prefab
5. Create patrol waypoints as empty children of the enemy
6. Assign Animator Controller with matching parameters
7. Test with a simple scene containing one enemy + player

### Layer Setup
- `Player` — player GameObject
- `Enemy` — all enemy GameObjects
- `Environment` — ground, walls, obstacles (included in NavMesh)
- `Interactable` — objects the player can interact with
- `DetectionObstacle` — objects that block enemy LoS
- `AttackHitbox` — attack hitboxes (optional)

### NavMesh Agent Defaults (Tiny Scale)
- Radius: 0.15
- Height: 0.3
- Step Height: 0.1
- Obstacle Avoidance: HighQuality
- Speed: varies per enemy type
- Stopping Distance: 0.5 patrol, 1.0 chase

### Collider Recommendations
- Cockroach: CapsuleCollider (radius 0.2, height 0.15)
- Spider: BoxCollider (0.5 x 0.2 x 0.5)
- Fly: SphereCollider (radius 0.2)
- All Rigidbodies should be kinematic (moved by NavMeshAgent)

### IDamageable Interface
- Existing `IDamageable` in `Assets/Scripts/Spells/IDamageable.cs` works with enemy system
- Enemy `IDamageable.TakeDamage` triggers: VFX, audio, hit anim, awareness, health loss

### Inspector Defaults per Enemy Type
| Stat | Cockroach | Spider | Fly |
|------|-----------|--------|-----|
| Move Speed | 4.5 | 2.5 | 3.5 |
| Chase Speed | 7 | 5 | 5 |
| Health | 30 | 50 | 20 |
| Attack Damage | 8 | 15 | 10 |
| Attack Range | 1.0 | 1.2 | 1.5 |
| Attack Cooldown | 1.0 | 1.5 | 1.0 |
| Detection Radius | 10 | 5 (ambush) | 12 |
| Chase Range | 15 | 8 | 18 |

### Animation Event Hooks
- `OnAttackHit()` — call from attack animation clip to trigger hit detection
- `OnAttackFinished()` — call from animation to end attack state
- `OnFootstep()` — call from movement animation for footstep audio
<!-- UNITY CODE ASSIST INSTRUCTIONS END -->
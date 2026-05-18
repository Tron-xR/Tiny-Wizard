# Giant Kitchen Environment — Setup Guide

## Design Concept

A low-poly stylized kitchen where every object is **gigantic** from the player's perspective.
The player is the size of a mouse/insect — a fork is as tall as they are, a sponge is a bed, a mug is a small room.

**Player Scale Reference:**
- Player capsule: 0.3 radius × 1.8 height
- A fork is ~2m long (equal to player height)
- A mug is ~1.5m tall (player can stand inside it)
- A sponge is ~1m × 0.8m (player can lie on it)

---

## Scale Reference Table

| Object | Real Size | Giant Scale | Player Relative | Primitive Placeholder |
|--------|-----------|-------------|-----------------|-----------------------|
| Fork | 20cm | **2m tall** | = player height | Cylinder (handle) + Box (prongs) |
| Spoon | 18cm | **1.8m tall** | = player height | Cylinder + Sphere (bowl) |
| Knife | 22cm | **2.2m long** | taller than player | Box (blade) + Cylinder (handle) |
| Mug | 10cm Ø | **1.2m Ø × 1.5m tall** | player fits inside | Cylinder (hollow) + Torus (handle) |
| Toaster | 18cm × 30cm | **2m × 3.5m** | 2× player width | Box + Box (slots) |
| Bread Slice | 12cm | **1.2m × 1.2m** | player can lie on it | Box (rounded) |
| Sponge | 10cm × 6cm | **1m × 0.6m** | player can sit on it | Box (rounded corners) |
| Sink Bowl | 40cm Ø | **4m Ø** | large enough to swim | Cylinder (concave) |
| Stove Burner | 20cm Ø | **2m Ø** | arena-sized | Cylinder (flat) + Rings |
| Fridge | 60cm × 180cm | **6m × 18m** | building-sized | Box (tall) |
| Countertop | 60cm deep | **6m deep** | 3× player length | Box (wide flat) |
| Cabinet Door | 40cm × 60cm | **4m × 6m** | garage door | Box (thin) |

---

## Hierarchy Structure

```
KitchenRoot
├── _Lighting
│   ├── DirectionalLight (sun)
│   ├── AmbientFill (rim light)
│   └── LightProbesGroup
│
├── _Floor
│   ├── FloorPlane (60×40, checkered tile)
│   ├── FloorTrim (edge loops)
│   └── Baseboard (wall trim)
│
├── _Walls
│   ├── BackWall (behind counters)
│   ├── LeftWall
│   ├── RightWall
│   └── WallWindow (light source)
│
├── CounterTopArea (z=0)
│   ├── Counter_Left (6×0.8×6m)
│   ├── Counter_Center (4×0.8×6m)
│   ├── Counter_Right (3×0.8×6m)
│   ├── Counter_Corner (L-shape piece)
│   └── CounterTopSurface (separate top piece)
│
├── SinkArea (z=0, on Center counter)
│   ├── Sink_Bowl (concave cylinder, 4m Ø)
│   ├── Sink_Faucet (curved pipe)
│   ├── Sink_Knobs (2 small spheres)
│   ├── Sponge (box, 1×0.6×0.4m)
│   └── SpongeBubbles (particle effect)
│
├── StoveArea (z=0, on Right counter)
│   ├── Stove_Body (box)
│   ├── Stove_Burner_1 (cylinder, front-left)
│   ├── Stove_Burner_2 (cylinder, front-right)
│   ├── Stove_Burner_3 (cylinder, back-left)
│   ├── Stove_Burner_4 (cylinder, back-right)
│   ├── Stove_Knobs (4 small cylinders)
│   └── Stove_Flame (particle effect, optional)
│
├── FridgeArea (right side, z=5)
│   ├── Fridge_Body (box, 6×18×4m)
│   ├── Fridge_Door (box, pivoted)
│   ├── Fridge_Handle (thin cylinder)
│   └── Fridge_Vents (grille pattern)
│
├── UnderCounterArea (under all counters)
│   ├── Cabinet_Doors (pair, 4×6m each)
│   ├── Cabinet_Knobs (2 spheres)
│   ├── Drawer_Front (box, 4×0.8m)
│   └── Drawer_Handle (thin cylinder)
│
├── CountertopObjects (on counter surface)
│   ├── GiantFork (2m long)
│   │   ├── Fork_Handle (cylinder, 0.15m Ø × 1.2m)
│   │   └── Fork_Prongs (4 thin boxes, curved)
│   │
│   ├── GiantSpoon (1.8m long)
│   │   ├── Spoon_Handle (cylinder, 0.12m Ø × 1.2m)
│   │   └── Spoon_Bowl (sphere, 0.5m Ø, half)
│   │
│   ├── GiantKnife (2.2m long)
│   │   ├── Knife_Blade (box, 1.4m × 0.3m × 0.05m)
│   │   └── Knife_Handle (cylinder, 0.8m × 0.12m Ø)
│   │
│   ├── GiantMug (1.5m tall)
│   │   ├── Mug_Body (cylinder, 1.2m Ø × 1.5m, hollow)
│   │   └── Mug_Handle (torus arc, 0.8m Ø)
│   │
│   ├── GiantToaster (2m × 3.5m)
│   │   ├── Toaster_Body (box, rounded)
│   │   ├── Toaster_Slot_1 (box inset)
│   │   ├── Toaster_Slot_2 (box inset)
│   │   └── Toaster_Knob (sphere)
│   │
│   ├── GiantBread (1.2m × 1.2m)
│   │   ├── Bread_Slice (rounded box, 1.2×1.2×0.15m)
│   │   └── Bread_Crust (edge strip)
│   │
│   └── CuttingBoard (4m × 2.5m)
│       ├── Board_Surface (thin box, rounded)
│       ├── KnifeScratchMark (decal, optional)
│       └── FoodScraps (small cubes)
│
├── FloorObjects
│   ├── Crumbs (small spheres, scattered)
│   ├── Droplet (sphere, 0.1m Ø)
│   └── DustBall (sphere, fuzzy)
│
└── _Props (misc)
    ├── MeasuringSpoon (0.6m, wall-hanging)
    ├── Timer (2m tall, on counter)
    ├── RecipeCard (3m × 2m, leaning on wall)
    ├── Magnet (on fridge, 0.5m)
    └── StringLight (across ceiling)
```

---

## Primitive Placeholder Reference

Build everything from Unity primitives first, then replace with low-poly models.

### Primitives to Use

| Primitive | Use For |
|-----------|---------|
| **Cube** | Counter bodies, fridge, cabinets, bread, toaster body, knife blade, cutting board |
| **Sphere** | Spoon bowl, knobs, crumbs, bubbles |
| **Capsule** | Fork/knife handles, faucet pipes |
| **Cylinder** | Mug body, burner discs, rolling pin, timer body |
| **Plane/Quad** | Decals, recipe card, wall art |
| **Torus** | Mug handle, ring burner |
| **Terrain** | (not suitable - flat kitchen) |

### Creating Hollow Shapes

**Mug interior:**
1. Cylinder (1.2m Ø × 1.5m tall)
2. Duplicate → scale down to 1.1m Ø × 1.4m tall
3. Invert normals on inner → use as subtraction
4. Or: use a cylinder with a Box as a Boolean cutter

**Sink Bowl:**
1. Cylinder (4m Ø × 1.5m tall)
2. Bottom face slightly pushed up → concave shape
3. Or: Sphere (4m Ø) → cut top 60%

---

## Prefab Setup Guide

### Folder Structure

```
Assets/
├── Kitchen/
│   ├── Prefabs/
│   │   ├── Structures/ (counters, cabinets, walls, floor)
│   │   ├── Appliances/ (fridge, stove, sink)
│   │   ├── Objects/ (fork, spoon, knife, mug, toaster, bread, sponge)
│   │   ├── Decor/ (crumbs, droplets, magnets, string lights)
│   │   └── VFX/ (stove flame, sink bubbles, light glow)
│   │
│   ├── Materials/
│   │   ├── Surfaces/ (countertop, floor, walls)
│   │   ├── Appliances/ (fridge white, stove metal, sink chrome)
│   │   ├── Objects/ (wood, bread, sponge foam, ceramic)
│   │   └── Glass/ (mug, window)
│   │
│   ├── Meshes/
│   │   ├── LowPoly/ (all custom low-poly meshes)
│   │   └── PrimitiveProxies/ (placeholder cubes/spheres)
│   │
│   ├── Scenes/
│   │   ├── Kitchen_Blank.unity (empty room)
│   │   └── Kitchen_Complete.unity (fully furnished)
│   │
│   └── Textures/
│       ├── TilePatterns/
│       ├── WoodGrain/
│       └── MetalScratches/
```

### Prefab Rules

1. **Root pivot at base center** — all prefabs pivot at (0, 0, 0) at floor level
2. **Uniform scale** — do not scale prefab instances; build at correct size
3. **Clean hierarchy** — max 3 levels deep (Root → Group → Mesh)
4. **No empty root scripts** — keep prefabs as pure meshes + colliders
5. **Material slots** — children use Materials folder, not embedded
6. **Separate collision mesh** (if using mesh colliders)

### Prefab Naming Convention

```
[Category]_[Object]_[Part]
Examples:
  Counter_Center_Body
  Counter_Center_Top
  Fridge_Body_Door
  Fork_Handle_Primary
  Fork_Prong_Tip
  Sink_Knob_Left
```

---

## Collider Recommendations

### Collider Type by Object

| Object | Collider Type | Reason |
|--------|--------------|--------|
| Counter body | **Box** | Simple, cheap |
| Counter top | **Box** | Flat surface |
| Floor | **Box** (flat) | Player walks on it |
| Walls | **Box** | Invisible walls |
| Fridge body | **Box** | Large rectangle |
| Fridge door | **Box** | Swinging door |
| Stove burners | **Cylinder** | Step up |
| Sink bowl | **Mesh** (convex) | Concave shape |
| Mug body | **Mesh** (convex) | Hollow interior |
| Mug handle | **Capsule** | Thin curved |
| Fork | **Box** × 5 | Prongs + handle |
| Spoon | **Mesh** (convex) | Curved bowl |
| Knife | **Box** × 2 | Blade + handle |
| Toaster | **Box** | Simple exterior |
| Bread | **Box** | Flat rectangle |
| Sponge | **Box** (slightly larger than mesh) | Soft collision |
| Cabinet doors | **Box** | Swinging |
| Small objects | **Sphere** or **Capsule** | Cheap |

### Collision Matrix

| Layer | Interacts With | Ignore |
|-------|---------------|--------|
| Player | Floor, Counters, Fridge, Walls, Objects | Small debris (optional) |
| Objects | Floor, Counters | Player (handled by script) |
| Debris | Floor | Player, Objects |

### Recommended Layers

| Layer | Index | Objects |
|-------|-------|---------|
| Default | 0 | (unused) |
| Ground | 6 | Floor |
| Wall | 7 | Walls, counters, fridge |
| Object | 8 | Fork, spoon, mug, toaster |
| Player | 9 | Player capsule |
| Ignore Raycast | 2 | VFX, particles |

**Physics Settings:**
- Player vs Object: detected (player can stand on objects)
- Player vs Debris: ignored (walk through crumbs)
- Object vs Object: ignored (only player pushes)

---

## Lighting Setup Guide (URP)

### Light Types

| Light | Type | Purpose |
|-------|------|---------|
| **SunLight** | Directional | Main shadow (warm) |
| **WindowLight** | Rectangular (Area) | Cool fill from window side |
| **FridgeLight** | Point | Interior glow |
| **StoveLight** | Point | Over stove area |
| **CabinetUnderglow** | Spot | Under-cabinet accent |
| **MugInteriorLight** | Point | Inside mug shadow (optional) |

### SunLight Settings

```
Light Type: Directional
Intensity: 1.2
Color: FFD9A0 (warm)
Shadow Type: Soft Shadows
Shadow Resolution: 2048
Bias: 0.05
Normal Bias: 0.4
Rotation: (50, -30, 0)
```

### WindowLight Settings (URP Baked)

```
Light Type: Area (baked)
Intensity: 0.5
Color: AODFFF (cool blue)
Shape: Rectangular (8×6m)
Size: matches window
Mode: Baked
Bounces: 2
```

### Ambient Settings (URP)

```
Environment Lighting:
  Source: Skybox (or Color)
  Ambient Color: 404060 (dark blue-gray)
  Intensity Multiplier: 0.3

Fog:
  Type: Exponential
  Density: 0.008
  Color: C0C0D0
```

### Lightmap Recommendations

| Surface | Resolution | Importance | Notes |
|---------|-----------|------------|-------|
| Floor | 50 texels/unit | High | Large visible surface |
| Countertops | 40 texels/unit | High | Casts shadows on under-cabinet |
| Fridge | 30 texels/unit | Medium | Large flat surface |
| Walls | 20 texels/unit | Low | Simple shadows |
| Small objects | 10 texels/unit | Low | Fork, spoon, etc. |

**Lightmap Settings:**
- Max Lightmap Size: 2048
- Lightmap Resolution: 20–50 (varies by importance)
- Lightmap Padding: 2
- Compress Lightmaps: ✓ (BC7 format in URP)
- Ambient Occlusion: 0.2–0.4 intensity

### Reflection Probe

Place one reflection probe at kitchen center (approximate player eye height):
```
Position: (0, 2, 0)
Radius: 15m
Type: Baked
Box Projection: ✓
Resolution: 128
```

---

## Geometry Placement

### Room Dimensions

```
Kitchen Floor: 24m × 16m (scaled for giant objects)
Ceiling Height: 12m (proportionally tall)
Wall Thickness: 0.3m
```

### Counter Placement

```
Back Wall (south, z=0):
┌────────────┬──────────┬──────────┬──────────┐
│    Sink    │  Prep    │   Stove  │  Empty   │
│    Area    │  Area    │   Area   │  Space   │
│  4m wide   │ 4m wide  │ 4m wide  │ 4m wide  │
└────────────┴──────────┴──────────┴──────────┘
                    P: z=2 (depth 6m)
                    P: y=0 (floor level)
                    H: 2m (counter height)
```

### Fridge Placement

```
Right Wall (east):
  Fridge at (16, 0, 4)
  Size: 6m wide × 18m tall × 4m deep
  Door: opens outward (hinge on right)
```

### Object Placement on Counter

```
Counter Surface (Y = 2.0m):

  ┌─────────────────────────────────────────┐
  │  [CuttingBoard]    [Mug]    [Toaster]   │  Back
  │  4×2.5m            1.2m Ø   2×3.5m     │
  │                                         │
  │  [Fork] [Spoon] [Knife]    [Bread]     │  Front
  │  2m     1.8m    2.2m       1.2m        │
  └─────────────────────────────────────────┘
```

### Floating Objects (e.g., Hanging)

```
  [StringLight]          Y=10m
  ──────────────────────────────────
  │               │               │
  [Timer]        [Sponge]      [Magnet]
  Y=3m           Y=4m          Y=16m (fridge)

  [MeasuringSpoon]  on wall hook at Y=3m, z=0
```

---

## URP Material Setup

### Material Naming

```
M_Floor_Tile_White
M_Counter_Wood_Light
M_Fridge_Metal_White
M_Stove_Metal_Black
M_Sink_Chrome_Polished
M_Mug_Ceramic_Red
M_Fork_Metal_Silver
M_Sponge_Foam_Yellow
M_Bread_Crust_Brown
M_Bread_Inside_Beige
M_Toaster_Plastic_Retro
M_Wall_Paint_Offwhite
M_Glass_Window
M_Cabinet_Wood_Dark
```

### URP Material Settings

```
Surface Type: Opaque (most)
    → Transparent (glass, water in sink)
Render Face: Both (double-sided for thin objects)
    → Front (for solids)
Alpha Clip: False
Receive Shadows: True
Cast Shadows: True
Emission: Sink interior (for dark areas)
    → Stove element glow (orange emission)
    → Fridge interior (white emission)
```

---

## Player Navigation Considerations

### Path Sizes

| Path | Min Width | Min Height |
|------|-----------|------------|
| Between counters | 2m | — |
| Under counter gap | 3.5m | 0.8m |
| Cabinet interior | 3m | 4m |
| Behind fridge | 1.5m (tight) | — |
| Under sink pipe | — | 1.2m |
| Inside mug | 1.1m Ø | 1.5m |
| On top of bread | 1.2m × 1.2m | — |

### NavMesh Settings

```
Agent Radius: 0.3m (matches player capsule)
Agent Height: 1.8m
Max Slope: 45°
Step Height: 0.4m
Area Types:
  - Walkable: floor, counters, bread, sponge
  - Not Walkable: sink bowl, stove burners (hot)
  - Jump: small gaps between objects
```

---

## Performance Notes

### Optimization by Distance

| Distance from Player | LOD | Collision | Shadow | Lightmap |
|---------------------|-----|-----------|--------|----------|
| 0–10m | Full detail | Full | ✓ Cast & Receive | Full res |
| 10–25m | Medium (decimated 50%) | Simple boxes | Receive only | Half res |
| 25m+ | Low (decimated 75%) | None | None | Quarter res |

### Object Count Target

```
Large structures: ~20 (walls, floor, counter sections)
Appliances: ~10 (fridge, stove, sink components)
Counter objects: ~15 (fork, spoon, knife, mug, toaster, bread, sponge)
Decor: ~30 (crumbs, droplets, magnets)
VFX: ~3 (stove flame, sink bubbles, light glow)
Total: ~80 objects (well within URP target)
```

---

## Construction Order

Build the scene in this sequence for best results:

```
Phase 1: Room Shell
  1. FloorPlane
  2. BackWall
  3. SideWalls
  4. Window in BackWall

Phase 2: Major Structures
  5. CounterLeft → CounterCenter → CounterRight
  6. Fridge_Body
  7. FloorTrim and Baseboard

Phase 3: Appliances
  8. Sink → Faucet → Knobs
  9. Stove → Burners → Knobs
  10. Fridge_Door → Handle → Vents

Phase 4: Cabinets & Storage
  11. UnderCounter → Doors → Knobs
  12. WallCabinets (optional)

Phase 5: Objects on Counter
  13. CuttingBoard
  14. Mug
  15. Toaster
  16. Bread
  17. Fork, Spoon, Knife
  18. Sponge (at sink)

Phase 6: Decor
  19. String Lights
  20. Crumbs and Droplets
  21. Magnets on Fridge
  22. Recipe Card

Phase 7: Lighting & Polish
  23. SunLight adjustment
  24. Light Probes
  25. Reflection Probe
  26. Post-processing
```

---

## Quick-Start Checklist

- [ ] Create Kitchen_Root with all sub-folders in hierarchy
- [ ] Place FloorPlane (24×16m) at Y=0
- [ ] Place Walls (tall, 12m)
- [ ] Create Counter sections from Box primitives
- [ ] Scale check: Player capsule next to Counter → player waist-height
- [ ] Place Fridge (huge box, 6×18m)
- [ ] Build Fork from Cylinder + 4 Box prongs (2m total)
- [ ] Build Mug from hollow Cylinder + Torus handle (1.5m tall)
- [ ] Set up Sun Directional Light (warm, shadows)
- [ ] Assign layers: Wall, Object, Ground
- [ ] Add Box Colliders to all large objects
- [ ] Create prefabs for each reusable object
- [ ] Run NavMesh bake
- [ ] Test: player walks on floor, jumps on counter, explores giant objects

---

## Visual Look & Feel

**Color Palette (low-poly stylized):**
```
Walls:     E8E0D4 (warm beige)
Floor:     D4C8B0 / B8A888 (checkered beige/brown)
Counters:  8B6914 (warm wood)
Fridge:    F0F0F0 (white)
Stove:     2A2A2A (black)
Sink:      C0C0C0 (chrome)
Mug:       CC3333 (red ceramic)
Toaster:   4488AA (retro teal)
Bread:     D4A050 (golden brown)
Sponge:    FFE040 (bright yellow)
Fork:      B0B0B0 (silver metal)
Knife:     909090 (steel)
Spoon:     B8B8B8 (silver)
Cutting Board: 8B6914 (wood)
```

**Stylization Tips:**
- Use flat shading / face-weighted normals for low-poly look
- Slightly bevel edges (0.01–0.05m) to catch light
- Avoid overly saturated colors (keep pastel/stylized)
- Use emission sparingly (stove glow, fridge interior)
- Add small decals (scratches on cutting board, water spots on sink)

---

**Ready to build! Start with primitives, then replace with low-poly meshes.**

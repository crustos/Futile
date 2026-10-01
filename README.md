
# Futile (2021 Edition)

Futile is a code-centric 2D framework for Unity. 

This is for those of you who want to do everything in code with as little editor integration as possible. 

If you've used Cocos2D or Flash you should feel right at home.

It's in development and completely undocumented... but it works. 

2021 note from Matt: Futile hasn't been updated a whole lot since I first released it, but it has been used (and continues to be used) in a bunch of real games
___

## Go to http://struct.ca/futile for UnityPackages and instructional videos

## Ask questions and share stuff you've made on http://reddit.com/r/futile

## Submit bugs and feature requests to http://github.com/MattRix/Futile/issues

## Futile works great with all versions of Unity (but let me know if you have any issues!)

## How to try the demo project: ##

#### How to open the project

- Grab the project from github and put it somewhere - [For the lazy, here's a zip of the whole repo](https://github.com/MattRix/Futile/zipball/master)
- Make sure you have Unity installed
- Go into FutileProject/Assets and open FutileScene.unity


## Third Party add-ons for Futile

- https://github.com/ManaOrb/FSceneManager (Futile Scene Manager and Parallax Scrolling Layer)
- https://github.com/mattfox12/FutileAdditionalClasses (including animated sprites and TMX tilemaps)
- https://github.com/Grizzlage/Futile-SpineSprite (for using animations made with Spine)
- https://gist.github.com/jpsarda/4573831 (FDrawingSprite.cs, for drawing lines)


## Legal stuff ##

Futile contains many ideas from Prime 31's UIToolkit: [github.com/Prime31/UIToolkit](http://github.com/Prime31/UIToolkit)

The MiniJSON parser is by http://github.com/darktable

The demo project also uses Prime31's fantastic GoKit tweening library: [github.com/Prime31/GoKit](http://github.com/Prime31/GoKit)

#### The code and art assets (except for the font) can be used for anything, however the sound effects and music are not to be used in anything else
#### GoKit's license is here: https://github.com/prime31/GoKit
#### The font is [Franchise](http://www.losttype.com/font/?name=franchise)

## MIT License ##

Source code for Futile is Copyright © 2019 Matt Rix and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS,” WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


## Crust port: plan

This fork is being ported to C with [crust](https://github.com/brentharts/crust),
for small, bounded 2D games: few objects, every count known when the game is
built. Futile is a good fit, and for the reason it was written: it is
code-centric. A Futile game is one MonoBehaviour that builds a scene graph of
plain C# objects (`FStage`, `FContainer`, `FSprite`, `FLabel`, ...) and draws it
through one renderer. So most of Futile is plain C#, and Unity is touched in a
few, well-defined places.

### How the pieces map

* **The scene graph and the library** (`Core`, `Display`, `Extras`, `Rix`):
  through crust's C# subset (CSRUST.md), as **arena classes** --
  `[MaxInstances(N)]`: a reference is a plain pointer, assignment copies it,
  `null` is 0, objects come from a statically sized arena and are released in
  bulk. The node tree's parent / child links and Futile's cycles
  (`FAtlas` <-> `FAtlasElement`, `FTouch` <-> `FTouchSlot`) need exactly that;
  crust's default single-owner class cannot hold them.
* **The renderer**: `FFacetRenderLayer` is the one place Futile makes Unity
  `Mesh`es -- a batch of quads (or triangles) per atlas and shader, with UVs and
  colours. Packed, it writes the same quads straight into the crust engine's
  draw batch (the GLES2 / GLES3 hosts unity_pack already uses): no
  `GameObject`, `MeshFilter` or `MeshRenderer` per layer, no mesh upload
  through Unity. This is the integration point that matters most, and it is
  small.
* **The entry point**: the `Futile` MonoBehaviour and the game's own script
  go through crust's **unity_pack**, which packs the scene (one GameObject),
  supplies `Time`, `Input`, `Screen` and the window, and calls the game's
  `Start` / `Update`. Futile's per-frame signal drives the C# subset code from
  there.
* **Assets**: atlases are TexturePacker JSON plus a texture, loaded through
  `Resources.Load`. Their names and contents are known when the game is
  packed, so unity_pack resolves them then: the atlas JSON is parsed at pack
  time into tables, the texture is packed with the engine's other textures,
  and an element looked up by name (`"Banana.png"`) can be resolved to an index
  where the name is a literal.
* **Physics** (`Physics`: `FPWorld`, `FPPolygonalCollider`, `FPDecomposer`):
  onto [Box2D-Packed](https://github.com/crustos/box2d) through unity_pack's
  Rigidbody2D / collider support. `FPDecomposer` already splits a polygon into
  convex parts -- Box2D takes convex polygons of up to 8 vertices as one shape,
  so the decomposition can target that limit directly.
* **Editor code** (`Rix/Editor`, `ThirdParty/Packrat/Editor`) is not part of
  the port. Unity-only code elsewhere goes behind `#if !CRUST` (crust defines
  `CRUST`); the source stays plain C# that Unity compiles as before.

### Status

3 of the 66 non-Editor scripts translate through crust's C# subset on their
own (`FPhysics`, `RXSignal`, `RXScroller`). Every other one stops at a named
line with a reason. The first blockers, by how many scripts they hold up:

| scripts | blocker | the way through |
|---|---|---|
| 12 | `string` (`FFacetType`, `FMatrix`, `FShader`, `FutileException`, ..) | debug / `ToString` text behind `#if !CRUST`; names resolved at pack time; `FLabel` text through unity_pack's strings |
| 10 | a type from another file (a base class or interface) | translating the library as one unit (crust) |
| 9 | `ref` arguments across files (`FFacetRenderLayer`, `FSprite`, `FLabel`, ..) | the same: crust lowers `ref` / `out` to a method of the same unit |
| 5 | `params` (`FFlipbookSprite`, `FMeshData`, `RXDebug`, ..) | an array built at the call (crust), or an explicit array |
| 4 | `event` (`FScreen`, `Futile`, `FButton`, ..) | a list of delegates; Futile's own `RXSignal` already translates |
| 2 | `base.Method()` (`FContainer`, `FGameObjectNode`) | crust: a call to the base implementation |
| 2 | `List.Remove` of a class (`FRenderer`, `FNode`) | reference equality, which arena classes give |
| 1 each | `??`, `char` (`FFont`), a cycle of classes (`FAtlas`, `FTouchManager`), `out` | arena classes for the cycles; small crust lowerings for the rest |

### Plan, in order

1. **One unit.** Translate `Futile/` as one compilation unit -- the step that
   clears the cross-file blockers (types, `ref`) for both this and crust's
   other ports. (crust)
2. **Arena classes.** `[MaxInstances(N)]` on the node classes (`FNode`,
   `FContainer`, `FSprite`, `FLabel`, ..), the atlas classes and the touch
   classes, sized for the game: a small game has a few hundred sprites, and
   the arena makes that a fixed, known amount of memory.
3. **Text out of the core.** `ToString` overrides and debug messages behind
   `#if !CRUST`; atlas and element names resolved when the game is packed.
4. **The renderer.** `FFacetRenderLayer` writes quads into the crust engine's
   batch when packed (behind `#if CRUST`), keeping its Unity `Mesh` path for
   Unity. One draw call per atlas and shader, as now.
5. **Signals and events.** `event` as Futile's `RXSignal`-style delegate lists;
   `params` as explicit arrays where it is cheap to change.
6. **The demo.** `BananaGame` packed with unity_pack as the end-to-end test:
   sprites, labels, touch / mouse input, the update signal.
7. **Physics** onto Box2D-Packed, with `FPDecomposer` targeting 8-vertex convex
   parts.

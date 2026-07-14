# DingoUnityExtensions

Russian version: `README_ru.md`.

`DingoUnityExtensions` is an asset-style Unity toolkit used as a shared technical foundation for Dingo projects. It combines local runtime/editor infrastructure with curated third-party packages and integration glue, so a team can solve recurring production problems once and reuse the solution across multiple games.

Primary branch: `dev`.

## Why this repository exists

Most Unity projects eventually accumulate the same cross-cutting code:

- lifecycle orchestration around `Update`, `LateUpdate`, coroutines, and delayed calls
- lightweight view binding for `uGUI` and TextMeshPro
- reusable reveal, tween, and micro-animation flows
- async loading for textures, files, JSON, and Addressables
- pooling for UI and gameplay views
- inspector and editor quality-of-life tools
- glue code around third-party libraries

This repository centralizes those concerns into one reusable layer that is intended to live directly under `Assets/` and be versioned like source code, not hidden behind opaque binaries.

## Key advantages

- One shared toolbox instead of copy-pasted utility scripts across many repositories.
- Production-oriented runtime helpers, not just editor sugar.
- Faster UI and gameplay iteration through view providers, pooled views, tween helpers, and debug tooling.
- Lower integration cost because local code and embedded third-party dependencies are maintained together.
- Source-level transparency: every system can be read, debugged, and modified inside the Unity project.
- Good fit for submodule-based Unity development where infrastructure is reused across several products.

## Architectural overview

`DingoUnityExtensions` is not a single framework. It is a layered collection of technical modules that can be adopted together or selectively.

### 1. Runtime foundation

Core primitives used by higher-level systems.

- `CoroutineParent.cs`
  Central coroutine host, ordered `Update` / `LateUpdate` / `FixedUpdate` orchestration, delayed invokes, coroutine cancellation by key, and bridges for `Task` / `UniTask`.
- `MonoBehaviours/`
  Base behaviours, singleton patterns, pixel-perfect transform helpers, UI-specific behaviours, and general reusable `MonoBehaviour` utilities.
- `Extensions/`
  Focused extension methods for transforms, vectors, colors, meshes, LINQ, strings, dictionaries, cameras, and tween helpers.
- `Utils/`
  Cross-cutting runtime/editor utilities for paths, reflection, coroutines, enums, sharing text, fuzzy matching, editor-runtime bridging, and safe file loading.
- `ExternalInit.cs`
  Compatibility shim for runtimes that need `System.Runtime.CompilerServices.IsExternalInit`.

### 2. View binding and UI composition

A lightweight container/provider approach for driving Unity components without introducing a heavyweight UI framework.

- `UnityViewProviders/`
  The core UI layer of the repository: `ValueContainer`, `EventContainer`, `UnityViewProvider<TView, TValue>`, grouped containers, list helpers, navigation helpers, async value containers, and typed providers for text, buttons, toggles, sliders, vectors, tuples, and more.
- `UnicodeFontIcons/`
  TextMeshPro icon binding through key-to-unicode mapping.
- `DevView/`
  Runtime debug drawing helpers backed by pools.
- `MonoBehaviours/UI/`
  Interaction helpers, layout utilities, graph/points UI elements, scroll helpers, and reusable UI behaviours.

### 3. Animation and presentation

Focused tools for common UI and view animation tasks.

- `Tweens/`
  Reveal and enable/disable flows, animation stacks, canvas group transitions, DOTween-based presentation helpers, and reusable animation endpoints.
- `MicroAnimations/`
  Small serializable animation units for transform, color, sound, images, and procedural image properties.
- `MaterialPropertiesAccess/`
  Time-driven material property application through `MaterialPropertyBlock` or shared materials.
- `uPaletteExtensions/`
  Integration layer between local animation logic and `uPalette` synchronizers.
- `LightUtils/`
  Global lighting preset application for `RenderSettings` and optional global volumes.
- `Art/`
  Rendering-facing helpers such as passing `RectTransform` size and pivot data into shader properties.

### 4. Loading, data, and IO

Utilities for asynchronous asset access and persistence workflows.

- `Addressables/`
  Thin wrappers around Addressables handles and LOD-aware asset loading.
- `ImageLoadGlobalSystem/`
  Shared image cache with loading-state propagation, lifetime management, reference tracking, and texture reuse across views.
- `Serialization/`
  Async JSON serialization, safe save flows, caching with file-stamp invalidation, and path-based data libraries.
- `NetWorking/`
  Minimal HTTP helpers and URI query helpers for lightweight integrations.
- `ClassTypeReference/`
  Serializable type references for inspector-driven workflows.

### 5. Performance and reuse

Helpers intended to reduce churn in frequently created objects and data buffers.

- `Pools/`
  Generic `MonoBehaviour` pools and pooled primitive containers.
- `Performance/`
  Background task queues, native array pooling, and texture pooling utilities.
- `MathAndGeometry/`
  Reusable geometric helpers such as frustum calculations and vector math.

### 6. Editor and authoring support

Quality-of-life tooling for authors and technical designers.

- `Editor/OnLoad.cs`
  Scans loaded assemblies and injects scripting define symbols for supported integrations.
- `PrefabsCreateMenu/`
  Config-driven prefab creation menus.
- Embedded editor-oriented libraries such as `NaughtyAttributes`, `Rotary Heart`, `MackySoft.SerializeReferenceExtensions`, and `uPalette` improve inspector workflows and authoring speed.

## Integration model

This repository is intentionally modular. Some integrations are activated when related assemblies are present in the project. `Editor/OnLoad.cs` inspects loaded assemblies and updates scripting define symbols automatically.

Auto-detected integration symbols include:

- `ADDRESSABLES_EXISTS`
- `BIND_EXISTS`
- `NEWTONSOFT_EXISTS`
- `UNITASK_EXISTS`
- `VINSPECTOR_EXISTS`
- `PROCEDURAL_IMAGE_EXISTS`
- `PROCEDURAL_UI_EXISTS`
- `MMFEEL_EXISTS`
- `NAUGHTYATTRIBUTES_EXISTS`
- `NAUGHTYATTRIBUTES_CK_EXISTS`

This matters because the same repository can be reused in projects with different dependency stacks without manually maintaining separate copies.

## Source dependencies

This repository includes nested git submodules. In the current checkout their commits are detached, but each one belongs to the branch line shown below.

| Path | Repository | Branch line | Role |
| --- | --- | --- | --- |
| `uPalette/` | `https://github.com/DingoBite/uPalette.git` | `as-submodule` | Palette authoring and runtime synchronizers. |
| `MackySoft/` | `https://github.com/DingoBite/Unity-SerializeReferenceExtensions.git` | `dingo` | Serialize-reference authoring tools used by inspector-driven workflows. |
| `NaughtyAttributes/` | `https://github.com/DingoBite/NaughtyAttributes.git` | `dingo` | Inspector attributes and editor ergonomics. |
| `Newtonsoft.Json-for-Unity.Converters/` | `https://github.com/DingoBite/Newtonsoft.Json-for-Unity.Converters.git` | `dingo` | Unity-specific JSON converters and editor configuration. |

## Runtime and package integrations

In the current codebase, some integrations are effectively core companions rather than optional add-ons:

- `DOTween`
  Required by the tween and micro-animation layers and several UI helpers.
- `UniTask`
  Used in pooling, async view/container handling, and multiplatform loading helpers.
- `Bind`
  Used by image loading, group binding helpers, data-library flows, and several UI/navigation integrations.
- `TextMeshPro`
  Required by text and icon-related providers.
- `Addressables`
  Enables the `Addressables/` wrappers and asset-handle flows.
- `Newtonsoft.Json`
  Enables the async serialization and data-library layers.

Additional features become available conditionally through the auto-detected symbols listed above.

## Top-level module reference

The repository root mixes local infrastructure and curated third-party modules. The list below is the quickest way to understand what lives where.

| Path | Role |
| --- | --- |
| `Addressables/` | Thin wrappers for Addressables asset loading. |
| `Art/` | Rendering helpers for UI and shader parameter bridging. |
| `ClassTypeReference/` | Serializable type references for inspector-driven selection. |
| `CoroutineParent.cs` | Global coroutine and ordered update orchestration. |
| `DevView/` | Runtime debug drawing and pooled debug visuals. |
| `Editor/` | Editor bootstrap and define-symbol processing. |
| `Extensions/` | Reusable extension methods across Unity and runtime types. |
| `ExternalInit.cs` | Compatibility shim for `System.Runtime.CompilerServices.IsExternalInit`. |
| `ImageLoadGlobalSystem/` | Shared async image loading and cache/lifetime management. |
| `LightUtils/` | Lighting preset application helpers. |
| `MackySoft/` | Embedded serialize-reference authoring tools. |
| `MaterialPropertiesAccess/` | Material and `MaterialPropertyBlock` animation accessors. |
| `MathAndGeometry/` | Geometry helpers, frustum math, and vector utilities. |
| `MicroAnimations/` | Small serializable animation building blocks. |
| `MonoBehaviours/` | Reusable behaviours, pixel-perfect transforms, UI helpers, and singleton base classes. |
| `NaughtyAttributes/` | Embedded inspector attribute library. |
| `NetWorking/` | Lightweight HTTP and URI helpers. |
| `Newtonsoft.Json-for-Unity.Converters/` | Unity-specific JSON converters and editor config. |
| `Performance/` | Task workers and pooled runtime buffers. |
| `Pools/` | Reusable object pools and pooled primitive containers. |
| `PrefabsCreateMenu/` | Configurable prefab creation menu tooling. |
| `Rotary Heart/` | Embedded autocomplete and editor helpers. |
| `Serialization/` | Async JSON, caching, and data-library infrastructure. |
| `Tweens/` | Reveal flows, animation containers, and DOTween helpers. |
| `UnicodeFontIcons/` | TextMeshPro icon mapping from semantic keys. |
| `UnityViewProviders/` | Core container/provider UI layer. |
| `uPalette/` | Embedded palette system. |
| `uPaletteExtensions/` | Local synchronizers and animation-aware palette integration. |
| `Utils/` | Miscellaneous runtime and editor helper utilities. |

## Representative technical patterns

### Ordered update orchestration

```csharp
CoroutineParent.AddUpdater(this, Tick, CoroutineOrderLayers.DEFAULT_PRIORITY);
CoroutineParent.AddLateUpdater(this, RefreshLate, CoroutineOrderLayers.MIN_PRIORITY_SPECIAL);
CoroutineParent.InvokeAfterSeconds(0.2f, Commit);
```

Why it is useful:

- removes the need for ad hoc global runner objects
- gives deterministic ordering hooks for cross-system coordination
- supports one-shot callbacks, keyed cancellation, and `Task` / `UniTask` bridging

### Pool-backed view reuse

```csharp
var pool = new Pool<ItemView>(_prefab, parent);
var view = pool.PullElement();
pool.PushElement(view);
```

Why it is useful:

- reduces instantiate/destroy churn for UI and gameplay views
- supports sync and async factories
- preserves parentage, ordering, layer inheritance, and optional custom deactivation logic

### Safe async serialization

```csharp
var serializer = new SemaphoreSerializer();
await serializer.SaveSerializeAsync(path, payload, bakFile: true);
var restored = await serializer.LoadDeserializeAsync<MyState>(path, cache: true);
```

Why it is useful:

- per-path save locks help prevent concurrent write corruption
- optional backup flow produces `.tmp` and `.bak` safety nets
- file cache invalidates when file stamps change instead of serving stale data indefinitely

### Shared image lifetime management

```csharp
var handle = new ImageLoadHandle(path);
handle.LoadFor(this);
handle.TextureFlow.SafeSubscribe(OnTextureLoaded);
```

Why it is useful:

- multiple receivers can share the same texture load
- textures are released when the last receiver is detached
- loading, success, and not-found states are propagated consistently to views

## Why this structure works in production

This repository is valuable not because it is large, but because it groups the right kinds of infrastructure together:

- UI-heavy projects can combine `UnityViewProviders`, `Tweens`, `UnicodeFontIcons`, and `Pools` to build reusable view layers quickly.
- Data-driven projects can combine `Serialization`, `ImageLoadGlobalSystem`, `Addressables`, and `Bind` for content loading and presentation flows.
- Editor-heavy pipelines benefit from serialize-reference tooling, inspector attributes, and prefab creation menus in one maintained source tree.

The main benefit is organizational: domain code stays focused on gameplay or product logic while cross-cutting technical concerns live in one maintained, versioned layer.

## When to use DingoUnityExtensions

Use it when you want:

- a shared technical base across several Unity projects
- fast UI iteration without committing to a large MVVM framework
- reusable runtime helpers for async flows, pooling, image loading, and presentation logic
- inspector-driven authoring with serialize-reference tooling and richer editor widgets
- a single place to standardize integrations around DOTween, UniTask, Bind, Addressables, and Newtonsoft JSON

It is especially effective for teams that prefer source-available infrastructure under `Assets/` instead of many disconnected utility packages.

## Installation

### Option 1. Git submodule (recommended)

Add the repository inside your Unity project, usually under `Assets/`:

```bash
git submodule add -b dev https://github.com/DingoBite/DingoUnityExtensions.git Assets/DingoUnityExtensions
git submodule update --init --recursive
```

`--recursive` is important because this repository includes nested submodules such as `uPalette`, `MackySoft`, `NaughtyAttributes`, and `Newtonsoft.Json-for-Unity.Converters`.

### Option 2. Copy into the project

Clone or download the repository and place it in a folder that Unity imports as assets, for example `Assets/DingoUnityExtensions`.

This repository is primarily designed as an asset folder, not as a standalone UPM package.

## Update strategy

If the repository was installed as a submodule:

```bash
git submodule update --remote --recursive Assets/DingoUnityExtensions
```

Recommended workflow:

- update on a feature branch
- let Unity reimport and regenerate scripting symbols
- validate the optional integrations that are active in the current project
- review nested submodule changes together with the root update

## Repository layout at a glance

```text
DingoUnityExtensions/
  Addressables/
  Art/
  ClassTypeReference/
  DevView/
  Editor/
  Extensions/
  ImageLoadGlobalSystem/
  LightUtils/
  MaterialPropertiesAccess/
  MathAndGeometry/
  MicroAnimations/
  MonoBehaviours/
  NetWorking/
  Performance/
  Pools/
  PrefabsCreateMenu/
  Serialization/
  Tweens/
  UnicodeFontIcons/
  UnityViewProviders/
  Utils/
  uPalette/
  uPaletteExtensions/
  MackySoft/
  NaughtyAttributes/
  Newtonsoft.Json-for-Unity.Converters/
  Rotary Heart/
  CoroutineParent.cs
```

## Technical trade-offs

This repository deliberately favors:

- source-level transparency over hiding everything behind compiled packages
- composition over a single heavyweight framework
- pragmatic coupling to widely used Unity libraries when the productivity win is clear
- incremental adoption, so a project can use only the layers it needs

The trade-off is breadth: the repository contains local code, wrappers, and curated third-party sources in one place. That breadth is intentional because a large part of the value comes from keeping those integrations compatible and centrally maintained.

## Third-party and licensing

Third-party code included here, or pulled in as nested submodules, remains under its own license. Check the corresponding folders and upstream repositories for licensing details.

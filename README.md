# DingoUnityExtensions (dev)

A collection of Unity utilities and extensions I use in development. The repository also contains some edited third‑party packages (for example, NaughtyAttributes).

Primary branch: `dev`.

## Installation

### Option 1. Git submodule (recommended)

Add the repo inside your Unity project (commonly under `Assets/`):

```bash
git submodule add -b dev https://github.com/DingoBite/DingoUnityExtensions.git Assets/DingoUnityExtensions
git submodule update --init --recursive
```

`--recursive` is recommended because this repository includes a submodule (`uPalette`).

### Option 2. Copy into the project

Clone or download the repository and place it into a folder that Unity imports as assets (for example, under `Assets/`).

## What’s inside

Top-level folders include (not exhaustive):

- Addressables
- Art
- ClassTypeReference
- DevView
- Editor
- Extensions
- ImageLoadGlobalSystem
- LightUtils
- MackySoft
- MaterialPropertiesAccess
- MathAndGeometry
- MicroAnimations
- MonoBehaviours
- NaughtyAttributes
- NetWorking
- Newtonsoft.Json-for-Unity.Converters
- Performance
- Pools
- PrefabsCreateMenu
- Rotary Heart
- Serialization
- Tweens
- UnicodeFontIcons
- UnityViewProviders
- Utils
- uPaletteExtensions

There is also a root-level `CoroutineParent.cs` helper used for coroutine utilities and centralized update dispatching.

## Third‑party

Any third‑party code included in this repo (or pulled in as submodules) remains under its own license. Check the corresponding folders/repositories for details.

# DingoUnityExtensions

Английская версия: `README.md`.

`DingoUnityExtensions` это asset-ориентированный набор инструментов для Unity, который используется как общая техническая основа для проектов Dingo. Он объединяет локальную runtime/editor-инфраструктуру, встраиваемые сторонние библиотеки и интеграционный glue-код, чтобы типовые production-задачи решались один раз и потом переиспользовались между несколькими играми.

Основная ветка: `dev`.

## Зачем существует этот репозиторий

Почти в каждом Unity-проекте со временем появляются одни и те же сквозные задачи:

- orchestration вокруг `Update`, `LateUpdate`, coroutine и отложенных вызовов
- легковесный view binding для `uGUI` и TextMeshPro
- переиспользуемые reveal-, tween- и micro-animation-пайплайны
- асинхронная загрузка текстур, файлов, JSON и Addressables
- pooling для UI и gameplay-view
- inspector- и editor-утилиты для ускорения авторинга
- glue-код вокруг сторонних библиотек

Этот репозиторий централизует такие задачи в одном переиспользуемом слое, который живет прямо под `Assets/` и версионируется как исходный код, а не как непрозрачный бинарный пакет.

## Ключевые преимущества

- Один общий toolbox вместо копипаста утилит между несколькими репозиториями.
- Упор на production runtime-инфраструктуру, а не только на editor sugar.
- Более быстрая итерация над UI и gameplay за счет view providers, pooling, tween-хелперов и debug-инструментов.
- Ниже стоимость интеграции, потому что локальный код и встроенные сторонние зависимости поддерживаются вместе.
- Полная прозрачность исходников: любую систему можно читать, дебажить и модифицировать прямо в Unity-проекте.
- Хорошо подходит для submodule-based разработки, где инфраструктура переиспользуется между несколькими продуктами.

## Архитектурный обзор

`DingoUnityExtensions` это не один framework, а набор технических слоев, которые можно подключать вместе или избирательно.

### 1. Runtime foundation

Базовые примитивы, на которые опираются более высокоуровневые системы.

- `CoroutineParent.cs`
  Центральный coroutine host, ordered orchestration для `Update` / `LateUpdate` / `FixedUpdate`, delayed invokes, отмена coroutine по ключу и bridge для `Task` / `UniTask`.
- `MonoBehaviours/`
  Базовые behaviours, singleton-паттерны, pixel-perfect transform helpers, UI-специфичные behaviours и общие reusable `MonoBehaviour`-утилиты.
- `Extensions/`
  Набор точечных extension methods для transform, vector, color, mesh, LINQ, string, dictionary, camera и tween-хелперов.
- `Utils/`
  Cross-cutting runtime/editor-утилиты для path, reflection, coroutine, enum, share text, fuzzy matching, editor-runtime bridging и безопасной загрузки файлов.
- `ExternalInit.cs`
  Compatibility shim для runtime-сред, которым нужен `System.Runtime.CompilerServices.IsExternalInit`.

### 2. View binding и UI composition

Легковесный container/provider-подход для управления Unity-компонентами без внедрения тяжелого UI-framework.

- `UnityViewProviders/`
  Ключевой UI-слой репозитория: `ValueContainer`, `EventContainer`, `UnityViewProvider<TView, TValue>`, групповые контейнеры, list helpers, navigation helpers, async value containers и типизированные providers для text, button, toggle, slider, vector, tuple и других типов.
- `UnicodeFontIcons/`
  Binding иконок TextMeshPro через key-to-unicode mapping.
- `DevView/`
  Runtime debug drawing helpers, работающие поверх pooling.
- `MonoBehaviours/UI/`
  Interaction helpers, layout utilities, graph/points UI-элементы, scroll helpers и reusable UI behaviours.

### 3. Animation и presentation

Сфокусированные инструменты для типовых UI- и view-animation-задач.

- `Tweens/`
  Reveal- и enable/disable-потоки, animation stacks, canvas group transitions, DOTween-based presentation helpers и reusable animation endpoints.
- `MicroAnimations/`
  Небольшие сериализуемые animation units для transform, color, sound, image и procedural image properties.
- `MaterialPropertiesAccess/`
  Time-driven изменение material properties через `MaterialPropertyBlock` или shared materials.
- `uPaletteExtensions/`
  Интеграционный слой между локальной animation-логикой и `uPalette` synchronizers.
- `LightUtils/`
  Применение lighting preset к `RenderSettings` и, при необходимости, к global volume.
- `Art/`
  Rendering-oriented helpers, например прокидывание размера и pivot `RectTransform` в shader properties.

### 4. Loading, data и IO

Утилиты для асинхронного доступа к asset-данным и persistence-пайплайнов.

- `Addressables/`
  Тонкие wrappers вокруг Addressables handles и LOD-aware asset loading.
- `ImageLoadGlobalSystem/`
  Общий image cache с распространением loading-state, управлением lifetime, учетом receivers и переиспользованием текстур между view.
- `Serialization/`
  Асинхронная JSON-сериализация, безопасные save-потоки, caching с invalidation по file-stamp и path-based data libraries.
- `NetWorking/`
  Минимальные HTTP-хелперы и URI query extensions.
- `ClassTypeReference/`
  Сериализуемые type references для inspector-driven workflows.

### 5. Performance и reuse

Инструменты, снижающие churn в часто создаваемых объектах и буферах данных.

- `Pools/`
  Generic `MonoBehaviour` pools и пуллинг примитивных контейнеров.
- `Performance/`
  Background task queues, native array pooling и texture pooling utilities.
- `MathAndGeometry/`
  Переиспользуемые geometric helpers, включая расчеты frustum и vector math.

### 6. Editor и authoring support

Инструменты качества жизни для авторов контента и technical designers.

- `Editor/OnLoad.cs`
  Сканирует загруженные assemblies и автоматически добавляет scripting define symbols для поддерживаемых интеграций.
- `PrefabsCreateMenu/`
  Config-driven tooling для меню создания prefab.
- Встроенные editor-oriented библиотеки вроде `NaughtyAttributes`, `Rotary Heart`, `MackySoft.SerializeReferenceExtensions` и `uPalette` ускоряют inspector workflows и authoring.

## Модель интеграции

Репозиторий изначально модульный. Часть интеграций активируется только тогда, когда соответствующие assemblies присутствуют в проекте. `Editor/OnLoad.cs` проверяет загруженные assemblies и автоматически обновляет scripting define symbols.

Автоматически определяемые integration symbols:

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

Это важно, потому что один и тот же репозиторий можно переиспользовать в проектах с разным dependency stack без ручной поддержки нескольких отдельных копий.

## Исходные зависимости

Репозиторий включает вложенные git submodule. В текущем checkout они находятся в detached state, но каждый pinned commit принадлежит ветке, указанной ниже.

| Путь | Репозиторий | Ветка | Роль |
| --- | --- | --- | --- |
| `uPalette/` | `https://github.com/DingoBite/uPalette.git` | `as-submodule` | Palette authoring и runtime synchronizers. |
| `MackySoft/` | `https://github.com/DingoBite/Unity-SerializeReferenceExtensions.git` | `dingo` | Serialize-reference authoring tools для inspector-driven workflows. |
| `NaughtyAttributes/` | `https://github.com/DingoBite/NaughtyAttributes.git` | `dingo` | Inspector attributes и editor ergonomics. |
| `Newtonsoft.Json-for-Unity.Converters/` | `https://github.com/DingoBite/Newtonsoft.Json-for-Unity.Converters.git` | `dingo` | Unity-specific JSON converters и editor configuration. |

## Runtime и package integrations

В текущем коде некоторые интеграции фактически являются базовыми спутниками репозитория, а не просто опциональными дополнениями:

- `DOTween`
  Нужен для tween- и micro-animation-слоев, а также для части UI-хелперов.
- `UniTask`
  Используется в pooling, async view/container handling и multiplatform loading helpers.
- `Bind`
  Используется в image loading, group binding helpers, data-library flows и нескольких UI/navigation integration points.
- `TextMeshPro`
  Нужен для text- и icon-related providers.
- `Addressables`
  Активирует `Addressables/` wrappers и asset-handle flows.
- `Newtonsoft.Json`
  Активирует async serialization и data-library layers.

Дополнительные возможности включаются условно через auto-detected symbols, перечисленные выше.

## Справочник по верхнеуровневым модулям

В корне репозитория смешаны локальная инфраструктура и curated third-party modules. Таблица ниже это самый быстрый способ понять, что где лежит.

| Path | Роль |
| --- | --- |
| `Addressables/` | Тонкие wrappers для Addressables asset loading. |
| `Art/` | Rendering helpers для UI и shader parameter bridging. |
| `ClassTypeReference/` | Сериализуемые type references для inspector-driven selection. |
| `CoroutineParent.cs` | Глобальный orchestration-слой для coroutine и ordered update. |
| `DevView/` | Runtime debug drawing и pooled debug visuals. |
| `Editor/` | Editor bootstrap и define-symbol processing. |
| `Extensions/` | Переиспользуемые extension methods для Unity- и runtime-типов. |
| `ExternalInit.cs` | Compatibility shim для `System.Runtime.CompilerServices.IsExternalInit`. |
| `ImageLoadGlobalSystem/` | Общая async image loading и cache/lifetime management. |
| `LightUtils/` | Lighting preset application helpers. |
| `MackySoft/` | Встроенные serialize-reference authoring tools. |
| `MaterialPropertiesAccess/` | Material и `MaterialPropertyBlock` animation accessors. |
| `MathAndGeometry/` | Geometry helpers, frustum math и vector utilities. |
| `MicroAnimations/` | Небольшие сериализуемые animation building blocks. |
| `MonoBehaviours/` | Reusable behaviours, pixel-perfect transforms, UI helpers и singleton base classes. |
| `NaughtyAttributes/` | Встроенная inspector attribute library. |
| `NetWorking/` | Легковесные HTTP- и URI-хелперы. |
| `Newtonsoft.Json-for-Unity.Converters/` | Unity-specific JSON converters и editor config. |
| `Performance/` | Task workers и pooled runtime buffers. |
| `Pools/` | Reusable object pools и pooled primitive containers. |
| `PrefabsCreateMenu/` | Configurable tooling для создания prefab через меню. |
| `Rotary Heart/` | Встроенные autocomplete и editor helpers. |
| `Serialization/` | Async JSON, caching и data-library infrastructure. |
| `Tweens/` | Reveal flows, animation containers и DOTween helpers. |
| `UnicodeFontIcons/` | TextMeshPro icon mapping по semantic keys. |
| `UnityViewProviders/` | Базовый container/provider UI-layer. |
| `uPalette/` | Встроенная palette system. |
| `uPaletteExtensions/` | Локальные synchronizers и animation-aware palette integration. |
| `Utils/` | Разные runtime и editor helper utilities. |

## Характерные технические паттерны

### Ordered update orchestration

```csharp
CoroutineParent.AddUpdater(this, Tick, CoroutineOrderLayers.DEFAULT_PRIORITY);
CoroutineParent.AddLateUpdater(this, RefreshLate, CoroutineOrderLayers.MIN_PRIORITY_SPECIAL);
CoroutineParent.InvokeAfterSeconds(0.2f, Commit);
```

Чем это полезно:

- убирает необходимость в ad hoc global runner objects
- дает детерминированные ordering hooks для координации между системами
- поддерживает one-shot callbacks, keyed cancellation и bridge с `Task` / `UniTask`

### Pool-backed view reuse

```csharp
var pool = new Pool<ItemView>(_prefab, parent);
var view = pool.PullElement();
pool.PushElement(view);
```

Чем это полезно:

- снижает instantiate/destroy churn для UI и gameplay-view
- поддерживает sync и async factories
- сохраняет parentage, ordering, layer inheritance и optional custom deactivation logic

### Safe async serialization

```csharp
var serializer = new SemaphoreSerializer();
await serializer.SaveSerializeAsync(path, payload, bakFile: true);
var restored = await serializer.LoadDeserializeAsync<MyState>(path, cache: true);
```

Чем это полезно:

- per-path save locks уменьшают риск concurrent write corruption
- optional backup flow создает `.tmp` и `.bak` safety nets
- file cache инвалидируется при изменении file stamp, а не раздает устаревшие данные бесконечно

### Shared image lifetime management

```csharp
var handle = new ImageLoadHandle(path);
handle.LoadFor(this);
handle.TextureFlow.SafeSubscribe(OnTextureLoaded);
```

Чем это полезно:

- несколько receivers могут делить одну и ту же загрузку текстуры
- текстуры освобождаются, когда отсоединяется последний receiver
- состояния loading, success и not-found распространяются по view единообразно

## Почему такая структура полезна в production

Сила репозитория не в том, что он большой, а в том, что в нем собраны правильные инфраструктурные слои:

- UI-heavy проекты могут быстро собирать reusable view layers, комбинируя `UnityViewProviders`, `Tweens`, `UnicodeFontIcons` и `Pools`.
- Data-driven проекты могут сочетать `Serialization`, `ImageLoadGlobalSystem`, `Addressables` и `Bind` для content loading и presentation flows.
- Editor-heavy pipelines выигрывают от serialize-reference tooling, inspector attributes и prefab creation menus в одном поддерживаемом source tree.

Главная польза организационная: domain code остается сфокусированным на gameplay или product logic, а сквозные технические задачи живут в одном поддерживаемом и версионируемом слое.

## Когда использовать DingoUnityExtensions

Используйте репозиторий, если вам нужны:

- общая техническая база для нескольких Unity-проектов
- быстрая итерация над UI без перехода на тяжелый MVVM-framework
- reusable runtime helpers для async flows, pooling, image loading и presentation logic
- inspector-driven authoring с serialize-reference tooling и более богатыми editor widgets
- единое место для стандартизации интеграций вокруг DOTween, UniTask, Bind, Addressables и Newtonsoft JSON

Особенно хорошо он подходит командам, которые предпочитают source-available infrastructure под `Assets/`, а не набор разрозненных utility packages.

## Установка

### Вариант 1. Git submodule (рекомендуется)

Добавьте репозиторий внутрь Unity-проекта, обычно под `Assets/`:

```bash
git submodule add -b dev https://github.com/DingoBite/DingoUnityExtensions.git Assets/DingoUnityExtensions
git submodule update --init --recursive
```

`--recursive` важен, потому что репозиторий включает вложенные submodule, например `uPalette`, `MackySoft`, `NaughtyAttributes` и `Newtonsoft.Json-for-Unity.Converters`.

### Вариант 2. Копирование в проект

Склонируйте или скачайте репозиторий и поместите его в папку, которую Unity импортирует как assets, например `Assets/DingoUnityExtensions`.

Репозиторий в первую очередь спроектирован как asset-folder, а не как standalone UPM package.

## Стратегия обновления

Если репозиторий подключен как submodule:

```bash
git submodule update --remote --recursive Assets/DingoUnityExtensions
```

Рекомендуемый workflow:

- обновлять на feature branch
- дать Unity переимпортировать assets и пересобрать scripting symbols
- валидировать те optional integrations, которые активны в текущем проекте
- просматривать изменения вложенных submodule вместе с обновлением корня

## Структура репозитория с высоты

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

## Технические trade-offs

Репозиторий сознательно делает ставку на:

- прозрачность исходников вместо сокрытия всего за compiled packages
- composition вместо одного тяжелого framework
- прагматичную связку с популярными Unity-библиотеками, когда выигрыш в productivity оправдан
- incremental adoption, чтобы проект мог брать только нужные ему слои

Цена такого подхода это широта: в одном месте лежат локальный код, wrappers и curated third-party sources. Но именно эта широта и является частью ценности, потому что совместимость этих интеграций поддерживается централизованно.

## Сторонний код и лицензирование

Сторонний код, включенный в репозиторий или подключаемый как nested submodule, остается под собственными лицензиями. Проверяйте соответствующие папки и upstream-репозитории для деталей лицензирования.

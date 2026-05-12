# Synty Assets — Guía de Uso

Pack de lowpoly estilo *Polygon* para un juego ambientado en el Antiguo Egipto.
Ubicación: `Assets/ThirdParty/Synty/`

---

## Estructura general

```
ThirdParty/Synty/
├── PolygonAncientEgypt/   ← contenido temático principal (~2 000 assets)
├── PolygonGeneric/        ← complementos genéricos (~1 000 assets)
└── SyntyPackageHelper/    ← utilidades de editor
```

---

## PolygonAncientEgypt

El pack gordo. Todo lo que necesitas para construir templos, casas, desiertos y escenas de puzle.

### Prefabs/

Usa **siempre los prefabs**, no los FBX directos. Cada FBX en `Models/` tiene su prefab equivalente en `Prefabs/` ya configurado con materiales y colliders.

| Carpeta | Contenido | Uso típico |
|---|---|---|
| `Prefabs/Buildings/` | 236 prefabs — templos, casas, muros, puertas, columnas, pirámides | Construir escenarios arrastrando a la escena |
| `Prefabs/Environment/` | 157 prefabs — suelo, rocas, palmeras, dunas, agua, oasis | Decoración de exteriores y naturaleza |
| `Prefabs/Props/` | 379 prefabs — jarrones, cofres, antorchas, pergaminos, estatuas, mesas, cestas… | Decorar interiores, colocar objetos interactuables |
| `Prefabs/Characters/` | 52 prefabs — faraones, guardias, sacerdotes, aldeanos | NPCs o personajes de fondo |
| `Prefabs/Characters/Attach/` | Accesorios sueltos (sombreros, armas en mano) | Equipar sobre personajes |
| `Prefabs/Weapons/` | 28 prefabs — espadas, lanzas, arcos, escudos | Props de armas |
| `Prefabs/Vehicles/` | 9 prefabs — carros de guerra, barcas | Decoración o mecánicas |
| `Prefabs/FX/` | 17 prefabs — fuego, humo, polvo, destellos | Efectos de partículas ya listos |

### Materials/

No toques los materiales directamente. Son compartidos entre prefabs.

| Carpeta | Qué controla |
|---|---|
| `Materials/Temple/Walls/Hieroglyphs/` | Muros con jeroglíficos pintados |
| `Materials/Temple/Walls/Murals/` | Frisos y murales decorativos |
| `Materials/Temple/Walls/Stone/` | Bloques de piedra lisos |
| `Materials/Temple/Walls/Bricks/` | Ladrillos de adobe |
| `Materials/Temple/Floors/Tiles/` | Suelos con baldosas |
| `Materials/House/` | Paredes y suelos de casas civiles |
| `Materials/Decals/` | Decals para superponer detalles (manchas, marcas) |
| `Materials/Alts/` | Variantes de color para reutilizar meshes con otro look |
| `Materials/FX/` | Materiales de partículas y efectos |

### Textures/

Solo lectura — referenciadas por los materiales.

- `Textures/Walls/MudBrick/` — ladrillo de barro (exterior)
- `Textures/Walls/Stone/` — piedra tallada (interior de templos)
- `Textures/Hieroglyphs/` — atlas de jeroglíficos (6 variantes)
- `Textures/Murals/` — 12 murales pintados
- `Textures/Decal/` — 15 decals (sangre, suciedad, marcas)
- `Textures/Alts/` — 30 variantes de paleta alternativa
- `Textures/FX/` — sprites para partículas

### Models/

FBX fuente. No usar directamente en escenas — existe por si necesitas importar a Blender u otro DCC.

- `Models/Collision/` y `Models/Collision/Convex/` — meshes simplificados para colliders (se usan internamente en los prefabs).

### Scripts/

- `SyntyWaterBobEgypt.cs` — hace que objetos sobre el agua suban y bajen simulando oleaje. Añádelo a cualquier prop flotante.

### Scenes/

- `Scenes/Overview.unity` — escena de catálogo con todos los assets colocados. Ábrela para inspeccionar visualmente qué hay disponible.
- `Scenes/Demo/` — demostración jugable del pack (útil como referencia de composición).

---

## PolygonGeneric

Pack complementario con assets más neutros, útil para transiciones o zonas no específicamente egipcias.

| Carpeta | Contenido | Uso |
|---|---|---|
| `Prefabs/Environment/` | 220 prefabs — vegetación, terreno, agua, cielos | Exteriores genéricos |
| `Prefabs/Props/` | 105 prefabs — cajas, barriles, escaleras, sillas | Relleno de interiores genéricos |
| `Prefabs/Building/` | 25 prefabs — muros y estructuras base | Prototipar niveles rápido |
| `Prefabs/Base/` | 70 prefabs — primitivas y piezas modulares | Kit de construcción modular |
| `Prefabs/FX/` | 15 prefabs — humo, polvo, destellos genéricos | Efectos reutilizables |
| `Prefabs/Weapons/` | 3 prefabs | Armas genéricas de relleno |

### Shaders/

51 archivos Shader Graph. Los más relevantes:

- `Shaders/SubGraphs/` — nodos reutilizables (ruido, gradientes, UV scroll). Puedes abrirlos en el Shader Graph Editor para entender cómo están hechos.
- `Shaders/InterfaceOverrides/` — overrides para el material inspector en el Editor.

---

## SyntyPackageHelper

Herramienta interna de Synty. No tocar.

- Detecta versión del pack y sugiere actualizaciones.
- Configura la pipeline de render (URP) automáticamente al importar.

---

## Cómo trabajar con estos assets

### Construir una escena nueva

1. Abre `PolygonAncientEgypt/Scenes/Overview.unity` para inspirarte.
2. Crea tu escena y arrastra prefabs desde `Prefabs/Buildings/` para la estructura.
3. Rellena con `Prefabs/Environment/` (suelo, rocas, plantas).
4. Decora con `Prefabs/Props/` (objetos interactuables, decoración).
5. Añade `Prefabs/FX/` para antorchas y efectos ambientales.

### Variaciones de color sin duplicar materiales

Usa los materiales de `Materials/Alts/` — son variantes del mismo mesh con paleta diferente. Así reutilizas geometría con aspecto distinto sin coste adicional.

### Decals

Los prefabs de `Prefabs/Props/` más los materiales de `Materials/Decals/` permiten superponer marcas sobre superficies. En URP usa el componente **Decal Projector**.

### Personajes (si los necesitas)

Los prefabs en `Prefabs/Characters/` son meshes estáticos (no tienen Animator por defecto). Si quieres animarlos, necesitarás añadir un `Animator` y un `AnimatorController` propio, o adquirir el pack de animaciones de Synty (POLYGON Starter Pack / Mecanim Anims).

### Agua con bobbing

Coloca un prop flotante en escena y añádele el script `SyntyWaterBobEgypt`. Configura `amplitude` y `frequency` en el Inspector para ajustar la intensidad del balanceo.

---

## Estadísticas

| | Ancient Egypt | Generic | Total |
|---|---|---|---|
| Prefabs | 878 | 438 | **1 316** |
| Modelos FBX | 884 | 435 | **1 319** |
| Materiales | 115 | 44 | **159** |
| Texturas | 153 | 56 | **209** |
| Shaders | — | 51 | **51** |

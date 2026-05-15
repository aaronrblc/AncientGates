# KHEMET — Game Design Document v0.1

> *Documento vivo. Esta versión cubre: concepto, pilares de diseño y prototipo técnico.*

---

## 1. Concepto

**Género:** Puzzle en primera persona  
**Plataforma:** PC  
**Motor:** Unity (VS 2026)  
**Ambientación:** Interior de una pirámide del Antiguo Egipto  
**Assets:** Polygon Ancient Egypt — Synty Studios  

El jugador despierta dentro de una pirámide. Cada sala es un puzzle matemático: un número visible en el HUD puede ser modificado interactuando con objetos, inscripciones y mecanismos del escenario. El entorno reacciona al valor de ese número — puertas, muros, trampas, altares. Pensar antes de actuar o quedarse bloqueado para siempre.

---

## 2. Pilares de diseño

### Core Fantasy
*Soy un explorador que descifra la lógica oculta de la pirámide.*  
El jugador no lucha, no corre: **razona**. La pirámide tiene sus propias reglas matemáticas y él las está desvelando. Cada sala resuelta es una revelación, no una victoria de habilidad.

### Loop Base
```
OBSERVAR → EXPERIMENTAR → CALCULAR → EJECUTAR → (reiniciar si falla)
```
1. El jugador entra a una sala y **observa** el número inicial y los objetos disponibles.  
2. **Experimenta** interactuando con modificadores (palancas, inscripciones, objetos).  
3. **Calcula** mentalmente (o en papel) qué secuencia lleva al valor objetivo.  
4. **Ejecuta** la secuencia correcta → el entorno reacciona → avanza.  
5. Si se bloquea sin solución posible → **reinicia la sala** desde el principio.

### Game Feel
- El número en el HUD debe reaccionar visualmente a cada cambio: animación de dígitos, brillo dorado, vibración sutil.  
- Los mecanismos del entorno deben tener **feedback físico claro**: puertas que rechinan, muros que tiemblan, antorchas que cambian de color según el valor.  
- El reinicio es inmediato y sin penalización de tiempo — el peso psicológico lo pone el propio jugador al darse cuenta de su error.  
- Silencio y ambiente sonoro denso (viento, arena, ecos) para forzar concentración.

---

## 3. Mecánica central — El Número

### Definición
- Un único entero visible en el HUD en todo momento.  
- Tiene un **valor inicial** al entrar a la sala (puede variar por sala).  
- Tiene un **valor objetivo** que activa el mecanismo de avance (puede ser visible o deducible).

### Tipos de modificadores (escalable)
| Tipo | Ejemplo en escenario | Efecto |
|---|---|---|
| Suma | Inscripción en muro | `+N` |
| Resta | Palanca hacia abajo | `-N` |
| Multiplicación | Altar encendido | `×N` |
| División | Vasija rota | `÷N` (solo si es divisible) |
| Reset | Estatua del dios | Vuelve al valor inicial |

### Reacción del entorno
El entorno monitoriza el número en tiempo real o por eventos (interacción):
- `número == objetivo` → puerta se abre, bloque se mueve, etc.  
- `número > umbral` → trampa activada, muro bloqueado.  
- `número < umbral` → luz apagada, pasaje cerrado.

---

## 4. Estructura de sala (prototipo)

Una sala prototipo contiene:
- **Punto de entrada** con el número inicial visible en HUD.  
- **2–4 modificadores** interactuables en el escenario.  
- **1 mecanismo reactivo** (puerta/muro/objeto) con valor objetivo.  
- **Trigger de reinicio** (salida bloqueada que devuelve al jugador al inicio de la sala).

### Ejemplo de sala prototipo
```
Número inicial: 3
Modificadores disponibles:
  - Inscripción A: ×4
  - Palanca B:     -6
  - Altar C:       +2

Objetivo: 6

Solución: 3 ×4 = 12 → 12 -6 = 6 ✓
Solución alternativa: 3 +2 = 5 ... sin salida → reinicio
```

---

## 5. Stack técnico

| Elemento | Tecnología |
|---|---|
| Motor | Unity (LTS recomendado) |
| IDE | Visual Studio 2026 |
| Assets 3D | Polygon Ancient Egypt — Synty |
| Cámara | First Person Controller (Unity) |
| HUD | Canvas World Space o Screen Space — Overlay |
| Gestión de estado de sala | ScriptableObject por sala |
| Interacción | Raycast desde cámara |

---

## 6. Pendiente (próximas iteraciones)

- [ ] Diseño de niveles (salas 1–N)  
- [ ] Sistema de narrativa / lore  
- [ ] Progresión de dificultad matemática  
- [ ] Música adaptativa  
- [ ] Menú principal y pantalla de game over  

---

*v0.1 — Documento base. Siguiente paso: prototipo jugable en Unity.*
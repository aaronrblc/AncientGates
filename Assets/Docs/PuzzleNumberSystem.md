# Sistema de Puzzles Numéricos

## Concepto

El jugador navega un nivel con modificadores repartidos por el espacio. Cada modificador altera un número global. Una puerta (u otro objeto reactivo) tiene una condición sobre ese número. El reto es elegir **qué modificadores tocar, cuáles evitar, y en qué orden**, para satisfacer la condición.

## Parámetros de un puzzle

| Parámetro | Descripción |
|---|---|
| **Valor inicial** | Número con el que empieza la partida (por defecto: 1) |
| **CON** | Condición de la puerta: `= N`, `≠ N`, `> N`, `< N`, `≥ N`, `≤ N`, `% N` |
| **Modificadores disponibles** | Lista de operaciones que el jugador puede tocar o ignorar |

### Operaciones disponibles (OperationType)

| Código | Efecto |
|---|---|
| `+N` | Suma N |
| `-N` | Resta N |
| `*N` | Multiplica por N |
| `/N` | Divide entre N (entero) |
| `=N` | Fija el valor a N |
| `↺`  | Reset al valor inicial |

## Orden y conmutatividad

- `+` y `-` entre sí son conmutativos (el orden no importa).
- En cuanto hay `*`, `/`, `=` o `↺` el orden **sí importa**.
- Puzzles de fase inicial: solo `+` y `-` (sin orden).
- Puzzles avanzados: mezcla con `*`, `/`, `=`, `↺` donde el orden es parte del reto.

## Cómo pedir un puzzle

Decirle a Claude:

> "Diseña un puzzle: valor inicial X, CON [condición], modificadores disponibles: [lista]. Quiero que haya exactamente [1 / 2 / N] combinación(es) válida(s)."

O más libre:

> "Dame un puzzle de dificultad media con 4 modificadores, solo sumas y restas."

Claude calculará todas las combinaciones posibles (subconjuntos × órdenes relevantes) y devolverá:
- La lista de modificadores
- Las combinaciones válidas
- Las combinaciones trampa (que parecen funcionar pero no)

## Ejemplo resuelto

**Valor inicial:** 1  
**CON:** > 3  
**Modificadores:** `-2`, `+3`

| Combinación | Resultado | ¿Válida? |
|---|---|---|
| (ninguno) | 1 | No |
| `-2` | -1 | No |
| `+3` | 4 | **Sí** |
| `-2` luego `+3` | 2 | No |
| `+3` luego `-2` | 2 | No |

Única solución: coger solo `+3`.

## Grupos de modificadores mutuamente exclusivos

Un `ModifierGroup` fuerza al jugador a elegir **uno** de los modificadores del grupo; el resto queda inaccesible.

### Configuración en Unity

1. Crear un GameObject vacío en la escena del nivel (p. ej. `ModifierGroup_A`).
2. Añadir el componente `ModifierGroup`.
3. Elegir `Mode`:
   - **Hide** — los descartados se desactivan (`SetActive(false)`).
   - **Disable** — los descartados quedan visibles pero no interactuables (highlight apagado).
4. Arrastrar a la lista `Members` los `InteractableTrigger` de los modificadores del grupo.

### Efecto sobre el diseño del puzzle

Al pedir un puzzle que use grupos, indicar cuáles modificadores compiten entre sí:

> "Dame un puzzle con valor inicial 1, CON = 6. Modificadores: grupo A (+2, +5), grupo B (*2, *3). Solo puede usarse un modificador de cada grupo."

El solver de `LevelDesigner` aún no filtra por grupos — calcular las combinaciones válidas teniendo en cuenta que solo se elige un miembro por grupo.

### Reset de nivel

Al recibir `LevelReset`, el grupo reactiva automáticamente a todos sus miembros (visibilidad y estado de uso).

## Notas de diseño

- Con solo `+` y `-`, el valor final es `inicial + suma(escogidos)` — el orden no importa.
- Para forzar orden como mecánica, introducir al menos una `*`, `/`, `=` o `↺`.
- Los modificadores con `PermanentDeactivator` (Reset GO) se usan una sola vez y alteran el número al salir de la zona — útiles para puzzles donde "pasar por aquí" tiene consecuencias permanentes.

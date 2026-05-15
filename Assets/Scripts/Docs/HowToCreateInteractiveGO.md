# Cómo crear un GameObject reactivo al número

Un objeto reactivo escucha el número del puzzle y ejecuta acciones cuando su condición se cumple o deja de cumplirse.

---

## Pasos

### 1. Añadir `NumberReactive`

Añade el componente `NumberReactive` al GameObject.

Configura la condición:
- **Condition** — el tipo de comparación (`Equals`, `GreaterThan`, `IsEven`, etc.)
- **Condition Value** — el número de referencia (no aplica en `IsEven` / `IsOdd`)

### 2. Añadir el script de acción

Añade al mismo GameObject el script que define lo que ocurre.  
Por ejemplo: `PositionMover` para mover el objeto de posición.

Configura sus parámetros según el comportamiento deseado.

### 3. Conectar los eventos

En `NumberReactive`, despliega los eventos y asigna:

- **On Condition Met** — acción cuando la condición se cumple  
  *Ejemplo: `PositionMover.Activate`*

- **On Condition Unmet** — acción cuando la condición deja de cumplirse  
  *Ejemplo: `PositionMover.Deactivate`*  
  *(Puede dejarse vacío si el objeto solo reacciona en una dirección)*

---

## Ejemplo

> Puerta que se abre cuando el número es par

| Campo | Valor |
|---|---|
| Condition | `IsEven` |
| On Condition Met | `PositionMover.Activate` |
| On Condition Unmet | `PositionMover.Deactivate` |
| PositionMover Offset | `(2, 0, 0)` |

# Synty AnimationBaseLocomotion — Setup en NPC

Pack: `Assets/ThirdParty/Synty/AnimationBaseLocomotion/`

## 1. Asignar el Animator Controller

1. Selecciona el prefab del NPC en la escena.
2. En el componente **Animator**, arrastra el controller que corresponda:
   - Personaje masculino Polygon → `Animations/Polygon/AC_Polygon_Masculine.controller`
   - Personaje femenino Polygon → `Animations/Polygon/AC_Polygon_Feminine.controller`
   - Variantes Sidekick → `Animations/Sidekick/AC_Sidekick_[Masc|Femn].controller`
3. Asegúrate de que **Avatar** apunta al avatar del mesh del NPC (el que vino rigged con el personaje Synty).

---

## 2. Script mínimo para un NPC

El `SamplePlayerAnimationController.cs` del pack está acoplado a input y cámara del jugador — no uses ese. En su lugar usa este script ligero que cubre idle/walk/run para un NPC con NavMeshAgent o velocidad manual.

```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class NpcAnimationDriver : MonoBehaviour
{
    // Gaits: 0=Idle, 1=Walk, 2=Run, 3=Sprint
    [SerializeField] private float walkThreshold = 0.5f;
    [SerializeField] private float runThreshold  = 3.0f;

    private Animator     _animator;
    private NavMeshAgent _agent;      // opcional; si no hay NavMesh, setea speed manualmente

    private static readonly int MoveSpeed  = Animator.StringToHash("MoveSpeed");
    private static readonly int CurrentGait = Animator.StringToHash("CurrentGait");
    private static readonly int IsGrounded  = Animator.StringToHash("IsGrounded");
    private static readonly int IsStopped   = Animator.StringToHash("IsStopped");
    private static readonly int IsWalking   = Animator.StringToHash("IsWalking");

    // Llama esto desde tu IA cada frame en lugar de usar Update si prefieres control externo
    public float Speed { get; set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent    = GetComponent<NavMeshAgent>();   // null si no tiene
    }

    private void Update()
    {
        float speed = _agent != null ? _agent.velocity.magnitude : Speed;
        UpdateAnimator(speed);
    }

    private void UpdateAnimator(float speed)
    {
        int gait;
        if      (speed < 0.05f)          gait = 0;   // Idle
        else if (speed < walkThreshold)  gait = 1;   // Walk
        else if (speed < runThreshold)   gait = 2;   // Run
        else                             gait = 3;   // Sprint

        _animator.SetFloat(MoveSpeed,   speed);
        _animator.SetInteger(CurrentGait, gait);
        _animator.SetBool(IsGrounded,  true);
        _animator.SetBool(IsStopped,   speed < 0.05f);
        _animator.SetBool(IsWalking,   gait == 1);
    }
}
```

Ajusta `walkThreshold` y `runThreshold` según las velocidades de tu NavMeshAgent (speed del agente).

---

## 3. Parámetros del Animator — referencia rápida

Solo los relevantes para un NPC básico:

| Parámetro | Tipo | Descripción |
|---|---|---|
| `MoveSpeed` | float | Velocidad 2D actual del personaje |
| `CurrentGait` | int | 0 Idle · 1 Walk · 2 Run · 3 Sprint |
| `IsGrounded` | bool | `true` siempre para NPCs en suelo |
| `IsStopped` | bool | `true` cuando velocidad ≈ 0 |
| `IsWalking` | bool | `true` cuando gait == Walk |
| `IsCrouching` | bool | Si necesitas agachado |
| `IsStrafing` | float | 0 = orientado a movimiento (normal para NPCs) |

Parámetros que **no necesitas tocar** en un NPC: `HeadLookX/Y`, `BodyLookX/Y`, `LeanValue`, `CameraRotationOffset`, `MovementInputTapped/Pressed`, `ShuffleDirection*` (son features del jugador).

> **Nota:** `MovementInputHeld` sí hace falta — el estado `Idle_Standing` solo transiciona a Locomotion cuando este bool es `true`. Ponlo a `true` mientras el NPC se mueve.

---

## 4. Strafing en NPCs

Por defecto el controller tiene `IsStrafing = 0`, lo que significa que el personaje **mira hacia donde se mueve** — comportamiento correcto para un NPC.

Si necesitas que el NPC mire hacia un objetivo mientras se mueve (p.ej. un guardia que te encañona), pon `IsStrafing = 1` y actualiza `StrafeDirectionX` / `StrafeDirectionZ` con el dot product entre el forward del NPC y su dirección de movimiento.

---

## 5. Verificar en el Editor

1. Entra en Play Mode con el NPC en escena.
2. Selecciona el NPC → ventana **Animator** → activa **Live Link** (icono cadena).
3. Verás los estados activos en el grafo y los valores de los parámetros en tiempo real.
4. Si el personaje está en T-Pose: el Avatar no coincide — revisa que el campo **Avatar** en el Animator sea el del mesh correcto.

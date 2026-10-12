using UnityEngine;

/// <summary>
/// NPC con animaciones Idle, Walk y Talk (parámetros del Animator: float "Speed", bool "Talking").
/// - Patrulla entre waypoints en loop.
/// - Se detiene y mira al jugador mientras platican.
/// - GoToAndStay(): deja de patrullar, camina a un punto y se queda ahí (lo usa NPCGuide).
/// </summary>
[RequireComponent(typeof(Animator))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Puntos de patrullaje (vacío = se queda quieto en Idle)")]
    public Transform[] waypoints;

    [Header("Movimiento")]
    public float moveSpeed = 1.2f;
    public float rotationSpeed = 5f;
    public float waypointThreshold = 0.1f;
    public float waitTimeAtPoint = 1.5f;

    [Header("Referencias")]
    public DialogueManager dialogueManager;

    private Animator animator;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isTalking;
    private Transform lookTarget;

    // Modo guía
    private bool patrolStopped;
    private Transform guideTarget;
    private System.Action onGuideArrived;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int TalkingParam = Animator.StringToHash("Talking");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Platicando con el jugador: quieto, lo mira y reproduce Talk.
        if (isTalking)
        {
            FaceTarget(lookTarget);
            SetAnimSpeed(0f);
            return;
        }

        bool dialogueOpen = dialogueManager != null && dialogueManager.IsDialogueActive;
        if (dialogueOpen)
        {
            SetAnimSpeed(0f);
            return;
        }

        // Modo guía: camina al punto destino y se queda ahí.
        if (guideTarget != null)
        {
            if (MoveToward(guideTarget.position))
            {
                guideTarget = null;
                SetAnimSpeed(0f);
                onGuideArrived?.Invoke();
                onGuideArrived = null;
            }
            return;
        }

        if (patrolStopped || waypoints == null || waypoints.Length == 0)
        {
            SetAnimSpeed(0f);
            return;
        }

        // Patrullaje normal
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            SetAnimSpeed(0f);
            return;
        }

        if (MoveToward(waypoints[currentWaypoint].position))
        {
            waitTimer = waitTimeAtPoint;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            SetAnimSpeed(0f);
        }
    }

    public void StartTalking(Transform target)
    {
        isTalking = true;
        lookTarget = target;
        animator.SetBool(TalkingParam, true);
    }

    public void StopTalking()
    {
        isTalking = false;
        lookTarget = null;
        animator.SetBool(TalkingParam, false);
    }

    /// <summary>Deja de patrullar, camina al destino y se queda ahí. Avisa al llegar.</summary>
    public void GoToAndStay(Transform destination, System.Action onArrived)
    {
        patrolStopped = true;
        guideTarget = destination;
        onGuideArrived = onArrived;
    }

    // Camina hacia una posición. Devuelve true cuando ya llegó.
    private bool MoveToward(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= waypointThreshold)
            return true;

        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

        SetAnimSpeed(moveSpeed);
        return false;
    }

    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    }

    private void SetAnimSpeed(float speed)
    {
        animator.SetFloat(SpeedParam, speed);
    }
}

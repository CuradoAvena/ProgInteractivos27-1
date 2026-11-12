using UnityEngine;
using UnityEngine.InputSystem;   // el NUEVO sistema de input

// =====================================================================
//  ThridPersonController.cs   (el movimiento del jugador)
//
//  DÓNDE SE PONE: en el JUGADOR. Necesita un CharacterController
//  (Add Component > Character Controller). Con [RequireComponent],
//  Unity lo agrega solo si falta.
//
//  USA EL NUEVO INPUT SYSTEM:
//    - La acción "moveAction" ya trae controles por defecto:
//      WASD, flechas y el stick izquierdo de un control (gamepad).
//    - Puedes cambiarlos en el Inspector (campo Move Action).
//
//  EL JUGADOR PUEDE SER UN CUBO O UNA CÁPSULA:
//    - El CharacterController es una cápsula invisible que maneja los choques.
//      Ajusta su Height (altura) y Radius para que cubra tu modelo.
//    - QUITA el BoxCollider/SphereCollider que Unity pone por defecto al crear
//      un cubo o esfera (si no, chocaría contra sí mismo).
//    - Un cubo es buena idea: se nota hacia dónde mira.
//
//  Se mueve en direcciones del mundo, por eso funciona con una cámara FIJA.
// =====================================================================
[RequireComponent(typeof(CharacterController))]
public class ThridPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;           // velocidad al caminar
    public float turnSmoothTime = 0.1f;    // qué tan rápido gira (menor = más brusco)
    public float gravedad = -20f;          // qué tan rápido cae

    [Header("Controles (nuevo Input System)")]
    // Una "acción" es un botón o eje con nombre. Esta devuelve un Vector2 (x = izq/der, y = arriba/abajo).
    public InputAction moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

    private CharacterController controller;
    private Animator animator;             // opcional: si no tienes uno, no pasa nada
    private float turnSmoothVelocity;      // dato interno para suavizar el giro
    private float velocidadVertical;       // cuánto estamos cayendo

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();   // puede ser null (no hay Animator)

        // Si no configuraste teclas en el Inspector, ponemos unas por defecto.
        if (moveAction.bindings.Count == 0)
        {
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveAction.AddBinding("<Gamepad>/leftStick");
        }
    }

    // Las acciones deben encenderse y apagarse junto con el script.
    void OnEnable() { moveAction.Enable(); }
    void OnDisable() { moveAction.Disable(); }

    void Update()
    {
        // 1. Leemos el input: x = izquierda/derecha, y = adelante/atrás.
        Vector2 input = moveAction.ReadValue<Vector2>();

        // normalized = misma velocidad en diagonal que en línea recta
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        Vector3 movimiento = Vector3.zero;

        // 2. Si presionamos algo, giramos hacia esa dirección y caminamos
        if (direction.sqrMagnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            movimiento = direction * moveSpeed;
        }

        // 3. Gravedad: si estamos en el piso, nos pegamos suave; si no, caemos más rápido cada vez
        if (controller.isGrounded && velocidadVertical < 0f) velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;
        movimiento.y = velocidadVertical;

        // 4. Movemos UNA sola vez por frame. Time.deltaTime hace que la velocidad
        //    sea igual en una compu rápida y en una lenta.
        controller.Move(movimiento * Time.deltaTime);

        // 5. Si hay Animator, le decimos qué tan rápido caminamos
        if (animator != null) animator.SetFloat("Speed", direction.magnitude);
    }
}
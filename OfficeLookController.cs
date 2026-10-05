using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Permite rotar la cámara de la oficina manteniendo presionado el CLIC DERECHO
/// y arrastrando el mouse — sin mover al jugador, sin CharacterController.
/// Es "mirar alrededor" desde un punto fijo, no exploración libre.
/// El clic izquierdo queda libre para el crosshair/botones de diálogo.
/// Colocar en la Main Camera de la escena de la oficina.
/// </summary>
public class OfficeLookController : MonoBehaviour
{
    [Header("Referencias")]
    public DialogueManager dialogueManager; // Para no rotar mientras hay diálogo abierto

    [Header("Sensibilidad")]
    public float sensitivity = 0.15f;

    [Header("Límite vertical (grados desde la posición inicial)")]
    [Tooltip("El giro horizontal (yaw) no tiene límite: puedes dar la vuelta completa para ver detrás de ti.")]
    public float minPitch = -20f; // Arriba
    public float maxPitch = 20f;  // Abajo

    private float yaw;   // Sin límite — se acumula libremente
    private float pitch; // Limitado entre minPitch y maxPitch
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        bool dialogueOpen = dialogueManager != null && dialogueManager.IsDialogueActive;
        bool rotating = !dialogueOpen && Mouse.current.rightButton.isPressed;

        if (rotating)
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

            yaw += delta.x; // Sin Clamp: puede dar la vuelta completa (360°)
            pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);

            transform.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}

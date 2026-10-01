using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseInteractor : MonoBehaviour
{
    [Header("Crosshair (UI)")]
    public RectTransform crosshairTransform;
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    [Header("Raycast hacia el mundo 3D")]
    public Camera mainCamera;
    public float interactRange = 15f;
    public LayerMask interactableLayer;

    [Header("Referencias")]
    public DialogueManager dialogueManager;

    private Interactable currentTarget;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Office.Examine.performed += OnExaminePerformed;
    }

    void OnEnable()
    {
        controls.Office.Enable();
        Cursor.lockState = CursorLockMode.Confined; // El mouse no sale de la ventana
    }

    void OnDisable()
    {
        controls.Office.Disable();
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        controls.Office.Examine.performed -= OnExaminePerformed;
    }

    void Update()
    {
        bool dialogueOpen = dialogueManager.IsDialogueActive;

        // Con diálogo abierto: se oculta el crosshair y se muestra el cursor normal
        // para poder hacer clic en los botones del diálogo.
        crosshairImage.gameObject.SetActive(!dialogueOpen);
        Cursor.visible = dialogueOpen;

        if (dialogueOpen)
        {
            currentTarget = null;
            return;
        }

        Vector2 mousePos = controls.Office.Point.ReadValue<Vector2>();
        crosshairTransform.position = mousePos;

        DetectInteractableUnderCursor(mousePos);
    }

    private void DetectInteractableUnderCursor(Vector2 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null && interactable.CanInteract)
            {
                currentTarget = interactable;
                crosshairImage.color = hoverColor;
                return;
            }
        }

        currentTarget = null;
        crosshairImage.color = normalColor;
    }

    private void OnExaminePerformed(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;
        if (dialogueManager.IsDialogueActive) return;

        currentTarget.Interact(dialogueManager);
        crosshairImage.color = normalColor;
    }
}

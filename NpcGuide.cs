using UnityEngine;

/// <summary>
/// Cuando una conversación activa la bandera "triggerFlag" (Set Flag On Enter en un nodo)
/// y el diálogo ya se cerró, el NPC deja su ruta, camina al punto destino
/// y al llegar hace aparecer el objeto (la llave).
/// Colocar en el mismo GameObject del NPC.
/// </summary>
public class NPCGuide : MonoBehaviour
{
    [Header("Disparador")]
    public string triggerFlag = "mario_sigueme";
    public DialogueManager dialogueManager;

    [Header("Destino y objeto")]
    public Transform destination;   // Punto B
    public GameObject keyObject;    // La llave (desactivada hasta que él llegue)

    [Header("Referencias del NPC")]
    public NPCPatrol patrol;
    public NPCInteractable interactable;

    [Header("Diálogos opcionales (qué dice si le hablas de nuevo)")]
    public DialogueNode dialogueWhileWalking;
    public DialogueNode dialogueAfterArrival;

    private bool started;

    void Start()
    {
        if (keyObject != null) keyObject.SetActive(false);
    }

    void Update()
    {
        if (started) return;
        if (!dialogueManager.HasFlag(triggerFlag)) return;
        if (dialogueManager.IsDialogueActive) return; // espera a que termine su línea

        started = true;

        if (dialogueWhileWalking != null)
            interactable.dialogueToStart = dialogueWhileWalking;

        patrol.GoToAndStay(destination, OnArrived);
    }

    private void OnArrived()
    {
        if (keyObject != null) keyObject.SetActive(true);

        if (dialogueAfterArrival != null)
            interactable.dialogueToStart = dialogueAfterArrival;
    }
}
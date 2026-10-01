using UnityEngine;

/// <summary>
/// Colocar en cualquier GameObject con Collider que el jugador deba poder
/// interactuar (archivero, puerta, objeto en el escritorio, etc.).
/// El Collider puede ser normal (no trigger) — el raycast lo detecta igual.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Qué pasa al interactuar")]
    public DialogueNode dialogueToStart;

    [Header("UI")]
    [Tooltip("Texto que se muestra en el prompt, ej: 'Abrir archivero'")]
    public string interactPrompt = "Interactuar";

    [Header("Opcional: solo se puede usar una vez")]
    public bool oneTimeUse = false;
    private bool alreadyUsed = false;

    public bool CanInteract => !(oneTimeUse && alreadyUsed);
    public bool WasExamined => alreadyUsed;

    public virtual void Interact(DialogueManager dialogueManager)
    {
        if (!CanInteract) return;

        alreadyUsed = true;
        dialogueManager.ShowNode(dialogueToStart);
    }
}

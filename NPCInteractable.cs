using UnityEngine;

/// <summary>
/// Interactable para NPCs: al examinarlo inicia su diálogo, el NPC voltea hacia
/// el jugador y entra en la animación Talk; cuando el diálogo se cierra, vuelve a Idle/Walk.
/// Usar en lugar del componente Interactable normal (hereda de él).
/// </summary>
public class NPCInteractable : Interactable
{
    [Header("NPC")]
    public NPCPatrol npc;

    private DialogueManager activeManager;

    public override void Interact(DialogueManager dialogueManager)
    {
        if (!CanInteract) return;

        base.Interact(dialogueManager);

        activeManager = dialogueManager;
        npc.StartTalking(Camera.main.transform);
    }

    void Update()
    {
        // Cuando se cierra el diálogo, el NPC deja de platicar.
        if (activeManager != null && !activeManager.IsDialogueActive)
        {
            npc.StopTalking();
            activeManager = null;
        }
    }
}

using UnityEngine;

/// <summary>
/// Colocar en la puerta de la oficina en vez de un Interactable normal.
/// Al examinarla: si falta revisar algún objeto obligatorio (ej. el escritorio),
/// muestra un aviso y no deja salir. Si ya todo fue revisado, pide confirmación Sí/No.
/// </summary>
public class ExitDoor : Interactable
{
    [Header("Objetos que deben examinarse antes de salir")]
    public Interactable[] requiredInteractables;

    [Header("Diálogo si falta algo por examinar")]
    public DialogueNode notYetNode;

    [Header("Diálogo de confirmación (debe tener choices: Sí / No)")]
    [Tooltip("El choice 'Sí' debe apuntar a un nodo marcado Is Ending. El choice 'No' puede dejarse sin Target Node (cierra el diálogo solo).")]
    public DialogueNode confirmExitNode;

    public override void Interact(DialogueManager dialogueManager)
    {
        if (AllRequiredExamined())
            dialogueManager.ShowNode(confirmExitNode);
        else
            dialogueManager.ShowNode(notYetNode);
    }

    private bool AllRequiredExamined()
    {
        foreach (var item in requiredInteractables)
        {
            if (item != null && !item.WasExamined)
                return false;
        }
        return true;
    }
}
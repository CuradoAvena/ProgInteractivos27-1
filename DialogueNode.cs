using UnityEngine;

/// <summary>
/// Representa un único nodo de diálogo: quién habla, qué dice, y a dónde lleva.
/// Si "choices" está vacío, el diálogo avanza automáticamente a "nextNode" (lineal).
/// Si "choices" tiene elementos, el jugador debe elegir una opción.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Contenido")]
    [TextArea(2, 5)]
    public string speakerName;

    [TextArea(3, 8)]
    public string dialogueText;

    [Header("Retrato del personaje (novela visual 2D)")]
    public Sprite speakerPortrait; // null = no muestra retrato (ej. narrador/notas)

    [Header("Flujo lineal (si no hay choices)")]
    public DialogueNode nextNode; // null = fin de esta rama

    [Header("Decisiones (dejar vacío si es lineal)")]
    public DialogueChoice[] choices;

    [Header("Flags de historia (opcional)")]
    [Tooltip("Nombre de una variable de historia que se activa al pasar por este nodo. Ej: 'vio_carpeta'")]
    public string setFlagOnEnter;

    [Header("Marcar como final")]
    public bool isEnding = false;
    public string endingName; // Ej: "Final: Escapa" / "Final: Confronta"

    [Header("Viñeta de zoom (ej: leer una nota, examinar un objeto pequeño)")]
    [Tooltip("Si se asigna, se muestra esta imagen en grande sobre la escena en vez del retrato normal")]
    public Sprite zoomImage;
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string choiceText;
    public DialogueNode targetNode;

    [Tooltip("Opcional: esta opción solo aparece si esta flag ya fue activada antes")]
    public string requiredFlag;
}

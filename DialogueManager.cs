using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class EndingReachedEvent : UnityEvent<string> { }

public class DialogueManager : MonoBehaviour
{
    [Header("Nodo inicial (opcional)")]
    public DialogueNode startingNode;

    [Header("Referencias UI (obligatorias)")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueBodyText;
    public GameObject choiceButtonPrefab; // Botón con un TMP_Text hijo
    public Transform choiceButtonContainer;

    [Header("Retrato y zoom (opcionales, para más adelante)")]
    public Image speakerPortraitImage;
    public GameObject portraitFrame; // El marco decorativo alrededor del retrato
    public GameObject zoomPanel;
    public Image zoomImageDisplay;

    [Header("Evento al alcanzar un final")]
    [Tooltip("Se dispara con el endingName del nodo. Úsalo para cambiar de escena, mostrar créditos, etc.")]
    public EndingReachedEvent onEndingReached;

    private readonly HashSet<string> storyFlags = new HashSet<string>();

    public bool IsDialogueActive => dialoguePanel.activeSelf;

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (zoomPanel != null) zoomPanel.SetActive(false);

        if (startingNode != null)
            ShowNode(startingNode);
    }

    public void ShowNode(DialogueNode node)
    {
        dialoguePanel.SetActive(true);

        if (!string.IsNullOrEmpty(node.setFlagOnEnter))
            storyFlags.Add(node.setFlagOnEnter);

        speakerNameText.text = node.speakerName;
        dialogueBodyText.text = node.dialogueText;

        UpdatePortraitAndZoom(node);
        ClearChoiceButtons();

        if (node.isEnding)
        {
            OnEndingReached(node.endingName);
            return;
        }

        if (node.choices != null && node.choices.Length > 0)
            BuildChoiceButtons(node);
        else
            CreateContinueButton(node.nextNode);
    }

    private void BuildChoiceButtons(DialogueNode node)
    {
        foreach (var choice in node.choices)
        {
            if (!string.IsNullOrEmpty(choice.requiredFlag) && !storyFlags.Contains(choice.requiredFlag))
                continue;

            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            btnObj.GetComponentInChildren<TMP_Text>().text = choice.choiceText;

            DialogueNode target = choice.targetNode;

            if (target == null)
                btnObj.GetComponent<Button>().onClick.AddListener(CloseDialogue); // Ej: botón "No"
            else
                btnObj.GetComponent<Button>().onClick.AddListener(() => ShowNode(target));
        }
    }

    private void CreateContinueButton(DialogueNode next)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        btnObj.GetComponentInChildren<TMP_Text>().text = "Continuar";

        if (next == null)
            btnObj.GetComponent<Button>().onClick.AddListener(CloseDialogue);
        else
            btnObj.GetComponent<Button>().onClick.AddListener(() => ShowNode(next));
    }

    public void CloseDialogue()
    {
        ClearChoiceButtons();
        dialoguePanel.SetActive(false);
        if (zoomPanel != null) zoomPanel.SetActive(false);
    }

    private void UpdatePortraitAndZoom(DialogueNode node)
    {
        bool hasZoom = node.zoomImage != null && zoomPanel != null && zoomImageDisplay != null;

        if (zoomPanel != null) zoomPanel.SetActive(hasZoom);
        if (hasZoom) zoomImageDisplay.sprite = node.zoomImage;

        bool hasPortrait = !hasZoom && node.speakerPortrait != null;

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait) speakerPortraitImage.sprite = node.speakerPortrait;
        }

        if (portraitFrame != null)
            portraitFrame.SetActive(hasPortrait);
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform child in choiceButtonContainer)
            Destroy(child.gameObject);
    }

    private void OnEndingReached(string endingName)
    {
        Debug.Log($"[DialogueManager] Final alcanzado: {endingName}");
        speakerNameText.text = "";
        dialogueBodyText.text = $"— {endingName} —";
        onEndingReached?.Invoke(endingName);
    }

    public bool HasFlag(string flag) => storyFlags.Contains(flag);
}

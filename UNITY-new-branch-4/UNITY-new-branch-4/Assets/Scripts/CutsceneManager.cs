using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public UnityEngine.UI.Image cutsceneImage;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Content")]
    public Sprite forestIntroSprite;
    public Sprite cityIntroSprite;
    public Sprite labIntroSprite;
    public Sprite endingSprite;

    private List<string> dialogueLines = new List<string>();
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private string fullText;

    void Start()
    {
        SetupCutscene();
        if (dialogueLines.Count > 0)
        {
            StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
        }
    }

    void SetupCutscene()
    {
        if (GameFlowManager.Instance == null) return;

        dialogueLines.Clear();
        currentLineIndex = 0;

        switch (GameFlowManager.Instance.currentState)
        {
            case GameFlowManager.GameState.Cutscene:
                dialogueLines.Add("The world was once peaceful, but then the corruption arrived.");
                dialogueLines.Add("I must venture into the forest to find the source.");
                if (forestIntroSprite != null) cutsceneImage.sprite = forestIntroSprite;
                break;
            case GameFlowManager.GameState.Dialog:
                dialogueLines.Add("The experiments... they were trying to harness the corruption.");
                dialogueLines.Add("It's over now. The laboratory is quiet.");
                dialogueLines.Add("Time to go home.");
                if (labIntroSprite != null) cutsceneImage.sprite = labIntroSprite;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (isTyping)
        {
            // Skip typing and show full line
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            // Move to next line or end cutscene
            currentLineIndex++;
            if (currentLineIndex < dialogueLines.Count)
            {
                StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
            }
            else
            {
                EndCutscene();
            }
        }
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void EndCutscene()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.CompleteCurrentState();
        }
    }
}

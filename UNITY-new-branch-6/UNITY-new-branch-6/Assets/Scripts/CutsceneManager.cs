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
    public CutsceneDialogueUI dialogueUI;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Content")]
    public Sprite cutScene1;
    public Sprite cutScene2;
    public Sprite cutScene3;
    public Sprite cutScene4;
    public Sprite cutScene5;
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
        if (dialogueUI == null)
            dialogueUI = GetComponentInChildren<CutsceneDialogueUI>(true);
        if (dialogueUI != null)
            dialogueUI.ApplyLayout();

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
                if (currentLineIndex == 0)
                {
                    cutsceneImage.sprite = cutScene1;
                }
                dialogueLines.Add("NARRATOR: After months of dead ends and late nights in the lab, the project was finally coming together.");
                dialogueLines.Add("NARRATOR: The lead scientists had just finalized their initial tests on a recently discovered organism.");
                dialogueLines.Add("SCIENTIST: Finally, they're done for the night. I just need to finish logging these baseline stability readings.");
                dialogueLines.Add("SCIENTIST: After all this overtime, I can finally head home early tomorrow");
                dialogueLines.Add("NARRATOR: Right behind him, the main viral specimen sat sealed inside its automated containment tank.");

                dialogueLines.Add("NARRATOR: It was a highly hazardous asset, kept under strict security protocols.");
                dialogueLines.Add("SCIENTIST: I should double-check the tank's containment pressure before I log out for the night.");
                dialogueLines.Add("SCIENTIST: That specimen has been fluctuating all week, and I don't want any surprises overnight.");

                dialogueLines.Add("NARRATOR: Without warning, a massive power spike overloaded the facility's grid, triggering a critical error.");
                dialogueLines.Add("NARRATOR: Emergency alarms cut through the building as the automated security gates locked down.");

                dialogueLines.Add("SCIENTIST: Wait... the containment seals are dropping. The door is locked, let me out!");
                dialogueLines.Add("NARRATOR: The over pressurized glass capsule shattered, instantly flooding the room with toxic airborne particles.");
                dialogueLines.Add("NARRATOR: The scientist collapsed into the chemical spill.");
                dialogueLines.Add("SCIENTIST: Agh! The tank blew...");
                dialogueLines.Add("SCIENTIST: No... it’s airborne... I can't breathe...");

                dialogueLines.Add("SCIENTIST: The virus didn't kill me—it remade me.");
                dialogueLines.Add("SCIENTIST: Now, I have to fight my way out before the infection takes completely.");


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
            ChangeBackground();
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

    void ChangeBackground()
    {
        // Change image depending on dialogue index

        if (currentLineIndex == 0)
        {
            cutsceneImage.sprite = cutScene1;
        }

        if (currentLineIndex == 4)
        {
            cutsceneImage.sprite = cutScene2;
        }
        if (currentLineIndex == 7)
        {
            cutsceneImage.sprite = cutScene3;
        }
        if (currentLineIndex == 9)
        {
            cutsceneImage.sprite = cutScene4;
        }
        if (currentLineIndex == 14)
        {
            cutsceneImage.sprite = cutScene5;
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

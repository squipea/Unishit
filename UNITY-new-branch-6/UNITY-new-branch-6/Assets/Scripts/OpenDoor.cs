using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class OpenDoor : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public GameObject interactText;
    public GameObject doorInputField;
    public GameObject lockDoorText;
    public int instructionForDoor = 1;
    public GameObject interactDoorIns;
    public bool isDoorUnlocked = false;
    



    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
    }
    public void Interact()
    {
        if (instructionForDoor == 1)
        {
            interactDoorIns.SetActive(true);
            instructionForDoor = 0;

            // First interaction starts the "press buttons" objective for this level.
            if (LevelManager.Instance != null)
                LevelManager.Instance.BeginButtonObjective(4);
        }

        Debug.Log($"[{nameof(OpenDoor)}] Interact called. isDoorUnlocked={isDoorUnlocked}", this);
        if (isDoorUnlocked)
        {
            SceneManager.LoadScene("Laboratory");
        }
        else
        {
            ShowPanel();
        }
            

    }

    public void OnNotTouchingPlayer()
    {
        sr.color = Color.black;
        interactText.SetActive(false);
    }

    public void OnTouchingPlayer()
    {
        sr.color = Color.yellow;
            interactText.SetActive(true);
    }

    public void DoorUnlocked()
    {
        isDoorUnlocked = true;
        Debug.Log($"[{nameof(OpenDoor)}] Door unlocked!", this);
    }

    public void ShowPanel()
    {
        lockDoorText.SetActive(true);
        StartCoroutine(HideAfterTime());
    }

    IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(3f);

        lockDoorText.SetActive(false);
    }

}

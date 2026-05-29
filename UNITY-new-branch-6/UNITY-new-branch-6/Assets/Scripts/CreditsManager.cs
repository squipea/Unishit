using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsManager : MonoBehaviour
{
    public TextMeshProUGUI creditsText;
    public float scrollSpeed = 50f;
    public RectTransform textRect;
    
    [Header("Content")]
    public string gameTitle = "Game Title Placeholder";
    public string developers = "Developer 1, Developer 2";
    public string artists = "Artist 1, Artist 2";
    public string animators = "Animator 1";
    public string soundEngineer = "Sound Engineer 1";
    public string programmers = "Programmer 1, Programmer 2";

    void Start()
    {
        SetupCredits();
        StartCoroutine(ScrollCredits());
    }

    void SetupCredits()
    {
        creditsText.text = $"<size=120%><color=#FFD700><b>{gameTitle}</b></color></size>\n\n\n" +
                          $"<b>DEVELOPERS</b>\n{developers}\n\n" +
                          $"<b>ARTISTS</b>\n{artists}\n\n" +
                          $"<b>ANIMATORS</b>\n{animators}\n\n" +
                          $"<b>SOUND ENGINEER</b>\n{soundEngineer}\n\n" +
                          $"<b>PROGRAMMERS</b>\n{programmers}\n\n\n\n" +
                          $"<i>Thank you for playing!</i>";
    }

    IEnumerator ScrollCredits()
    {
        if (textRect == null) yield break;

        // "Dropping down" - Start above the screen and scroll down
        float screenHeight = Screen.height;
        Vector2 pos = textRect.anchoredPosition;
        
        // Calculate the height of the text content
        float contentHeight = textRect.sizeDelta.y;
        
        // Start completely above the screen
        pos.y = screenHeight + 100f;
        textRect.anchoredPosition = pos;

        // Scroll down until completely below the screen
        float targetY = -contentHeight - 100f;
        
        while (textRect.anchoredPosition.y > targetY)
        {
            pos.y -= scrollSpeed * Time.deltaTime;
            textRect.anchoredPosition = pos;
            yield return null;
        }

        // End credits
        EndCredits();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndCredits();
        }
    }

    void EndCredits()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.CompleteCurrentState();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

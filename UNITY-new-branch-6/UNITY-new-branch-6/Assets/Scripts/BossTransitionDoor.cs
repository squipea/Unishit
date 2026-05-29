using UnityEngine;

public class BossTransitionDoor : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private bool isPlayerTouching = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Visual indicator: show red if soldier is alive (locked), green if soldier is dead (unlocked)
        bool soldierDefeated = !HasActiveSoldiersRemaining();
        if (sr != null)
        {
            sr.color = soldierDefeated ? Color.green : Color.red;
        }

        // Fallback explicit triggers if Unity Physics collisions fail:
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Fallback 'T' key pressed. Attempting interaction...");
            Interact();
        }
        else if (isPlayerTouching && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player touching door and pressed 'E'. Attempting interaction...");
            Interact();
        }
    }

    public void Interact()
    {
        if (HasActiveSoldiersRemaining())
        {
            Debug.Log("Door is locked! You must defeat the soldier first.");
            return;
        }

        Debug.Log("BossTransitionDoor: Player interacted. Teleporting to boss level...");
        // Prefer the transition component on this same GameObject (door)
        InitialSoldierStageTransition transition = GetComponent<InitialSoldierStageTransition>();
        if (transition == null)
            transition = FindFirstObjectByType<InitialSoldierStageTransition>();

        if (transition != null)
        {
            transition.TriggerTransition();
        }
        else
        {
            Debug.LogError("InitialSoldierStageTransition component not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Interact();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Interact();
        }
    }

    public void OnTouchingPlayer()
    {
        isPlayerTouching = true;
        // Optionally show prompt or change visual cue
        if (sr != null)
        {
            // Highlight when player is near
            sr.color = Color.yellow;
        }
    }

    public void OnNotTouchingPlayer()
    {
        isPlayerTouching = false;
        if (sr != null)
        {
            sr.color = Color.white; // Reset to default when not touching
        }
    }

    private void OnGUI()
    {
        if (isPlayerTouching)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            // Draw a subtle background box for readability
            Rect rect = new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 40);
            GUI.Box(rect, "");

            if (HasActiveSoldiersRemaining())
            {
                style.normal.textColor = Color.red;
                GUI.Label(rect, "Defeat Soldier First!", style);
            }
            else
            {
                style.normal.textColor = Color.green;
                GUI.Label(rect, "Press 'T' to teleport", style);
            }
        }
    }

    private bool HasActiveSoldiersRemaining()
    {
        SoldierAI[] soldiers = FindObjectsByType<SoldierAI>(FindObjectsSortMode.None);
        foreach (SoldierAI soldier in soldiers)
        {
            if (soldier != null && soldier.enabled && soldier.gameObject.activeInHierarchy)
            {
                EnemyHealth health = soldier.GetComponent<EnemyHealth>();
                if (health != null && health.enabled)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

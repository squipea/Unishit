using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    private bool hasTriggered = false;
    private bool isLocked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (BossCameraController.Instance != null)
            {
                BossCameraController.Instance.EnterBossMode();
                isLocked = true;
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Once locked, the player cannot exit boss mode
            if (isLocked)
                return;

            if (BossCameraController.Instance != null)
            {
                BossCameraController.Instance.ExitBossMode();
            }
        }
    }
}

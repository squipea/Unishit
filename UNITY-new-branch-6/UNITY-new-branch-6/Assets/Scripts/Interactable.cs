using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact();

    void OnTouchingPlayer();

    void OnNotTouchingPlayer();
}

/// <summary>
/// Placed on the Player. Detects nearby IInteractable objects using a proximity
/// overlap each frame (avoids trigger-layer-order issues) and calls Interact()
/// when the player presses E.
/// </summary>
public class Interactable : MonoBehaviour
{
    [Tooltip("How close the player must be to an IInteractable to detect it.")]
    public float interactRadius = 1.5f;

    private IInteractable currentInteractable;
    private bool eWasDown = false;

    void Update()
    {
        // Find nearest IInteractable within range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        IInteractable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearest = interactable;
                nearestDist = dist;
            }
        }

        // Handle enter/exit callbacks
        if (nearest != currentInteractable)
        {
            if (currentInteractable != null)
                currentInteractable.OnNotTouchingPlayer();

            currentInteractable = nearest;

            if (currentInteractable != null)
                currentInteractable.OnTouchingPlayer();
        }

        // Trigger Interact on E key press (rising edge only)
        bool eDown = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        if (eDown && !eWasDown && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
        eWasDown = eDown;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}

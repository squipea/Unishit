using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact();

    void OnTouchingPlayer();

    void OnNotTouchingPlayer();
}

public class Interactable : MonoBehaviour
{
    private IInteractable currentInteractable;

    void Update()
    {
        if(Keyboard.current.eKey.IsActuated() && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                currentInteractable.OnTouchingPlayer();
            }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
       
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null && interactable == currentInteractable)
            {
                currentInteractable.OnNotTouchingPlayer();
                currentInteractable = null;
            }
        
    }
}

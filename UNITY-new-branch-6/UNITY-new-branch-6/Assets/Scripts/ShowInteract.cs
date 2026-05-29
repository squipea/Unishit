using UnityEngine;

public class ShowInteract : MonoBehaviour, IInteractable
{
    public GameObject interactText;
    public void Interact()
    {
        
    }

    public void OnNotTouchingPlayer()
    {
        interactText.SetActive(false);
    }

    public void OnTouchingPlayer()
    {
        interactText.SetActive(true);
    }
}

using UnityEngine;

public class DeadTreePlay : MonoBehaviour, IInteractable
{
    public Animator deadTree;
    public void Interact()
    {
        
    }

    public void OnNotTouchingPlayer()
    {
        deadTree.SetBool("Entered", false);
    }

    public void OnTouchingPlayer()
    {
        Debug.Log("Player touched the dead tree. Animator parameter 'Entered' set to true.");
        deadTree.SetBool("Entered", true);
        
    }

}

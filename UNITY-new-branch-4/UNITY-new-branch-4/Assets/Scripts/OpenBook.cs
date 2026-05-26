using UnityEngine;

public class OpenBook : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Color originalColor;
    public GameObject bookUI;
    public GameObject interactText;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }
    public void Interact()
    {
        sr.color = Color.green;
    }

    public void OnNotTouchingPlayer()
    {
        interactText.SetActive(false);
    }

    public void OnTouchingPlayer()
    {
        interactText.SetActive(true);
         
    }

    public void DestroyBook()
    {
        Destroy(gameObject);
    }

    public void OnOpenBookUI()
    {
        bookUI.SetActive(true);
    }

}

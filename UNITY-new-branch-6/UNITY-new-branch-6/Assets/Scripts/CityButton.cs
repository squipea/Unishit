using UnityEngine;

public class CityButton : MonoBehaviour, IInteractable
{
    [Header("Optional visuals/UI")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject interactText;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.green;

    [Header("Wiring")]
    [SerializeField] private CityButtonHandler handler;

    private Color originalColor;
    private bool isPressed;

    private void Awake()
    {
        
            handler = FindFirstObjectByType<CityButtonHandler>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Interact()
    {
        Debug.Log("Button Pressed");
        if (isPressed)
        {
            return;
        }

        isPressed = true;
        Debug.Log($"[{nameof(CityButton)}] Interacted: {name}. Handler set? {handler != null}", this);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = pressedColor;
        }

        if (interactText != null)
        {
            interactText.SetActive(false);
        }

        if (handler != null)
        {
            handler.NotifyPressed(this);
        }
        else
        {
            Debug.LogWarning($"[{nameof(CityButton)}] No handler assigned on {name}. Door will not unlock.", this);
        }
    }

    public void OnTouchingPlayer()
    {
        if (isPressed)
        {
            if (interactText != null)
            {
                interactText.SetActive(false);
            }
            return;
        }

        

        if (interactText != null)
        {
            interactText.SetActive(true);
        }
    }

    public void OnNotTouchingPlayer()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPressed ? pressedColor : originalColor;
        }

        if (interactText != null)
        {
            interactText.SetActive(false);
        }
    }
}


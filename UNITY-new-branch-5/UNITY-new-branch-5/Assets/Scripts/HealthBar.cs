using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        if (fillImage != null && colorGradient != null)
        {
            fillImage.color = colorGradient.Evaluate(1f);
        }
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        if (fillImage != null && colorGradient != null)
        {
            fillImage.color = colorGradient.Evaluate(slider.normalizedValue);
        }
    }

    private void Update()
    {
        // Keep the health bar upright if the parent flips
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.localScale;
            Vector3 myScale = transform.localScale;
            
            // If parent is flipped (negative x), we flip our scale to compensate
            if (parentScale.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(myScale.x), myScale.y, myScale.z);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(myScale.x), myScale.y, myScale.z);
            }
        }
    }
}

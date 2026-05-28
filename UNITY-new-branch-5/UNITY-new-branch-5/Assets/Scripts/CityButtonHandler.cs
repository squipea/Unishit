using UnityEngine;

public class CityButtonHandler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private OpenDoor door;
    [SerializeField] private CityButton[] buttons;

    private bool[] pressed;

    private void Awake()
    {
        pressed = buttons == null ? new bool[0] : new bool[buttons.Length];
    }

    public void NotifyPressed(CityButton button)
    {
        if (buttons == null || buttons.Length == 0 || button == null)
        {
            Debug.LogWarning($"[{nameof(CityButtonHandler)}] NotifyPressed ignored. buttons set? {(buttons != null ? buttons.Length : 0)}. button null? {button == null}", this);
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == button)
            {
                if (pressed != null && i < pressed.Length && pressed[i])
                    break;

                pressed[i] = true;
                Debug.Log($"[{nameof(CityButtonHandler)}] Button pressed: {button.name} ({i + 1}/{buttons.Length})", this);

                if (LevelManager.Instance != null)
                    LevelManager.Instance.RegisterButtonPressed();

                break;
            }
        }

        for (int i = 0; i < pressed.Length; i++)
        {
            if (!pressed[i])
            {
                return;
            }
        }

        if (door != null)
        {
            Debug.Log($"[{nameof(CityButtonHandler)}] All buttons pressed. Unlocking door: {door.name}", this);
            door.DoorUnlocked();
        }
        else
        {
            Debug.LogWarning($"[{nameof(CityButtonHandler)}] All buttons pressed but Door reference is not set.", this);
        }
    }
}

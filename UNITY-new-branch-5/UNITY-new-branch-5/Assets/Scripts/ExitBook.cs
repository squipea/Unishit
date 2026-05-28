using UnityEngine;
using UnityEngine.InputSystem;

public class ExitBook : MonoBehaviour
{
    public GameObject bookUI;

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && bookUI.activeSelf)
        {
            OnExitBook();
        }
    }
    public void OnExitBook()
    {
        bookUI.SetActive(false);
    }
}

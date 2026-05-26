using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CloseDoorIns : MonoBehaviour
{
    public GameObject DoorPanelUI;
    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame && DoorPanelUI.activeSelf)
        {
            OnCloseDoorPanel();
        }
    }

    public void OnCloseDoorPanel()
    {
        DoorPanelUI.SetActive(false);
    }
}

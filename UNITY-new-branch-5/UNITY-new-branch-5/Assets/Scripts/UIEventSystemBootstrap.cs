using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Ensures gameplay scenes have an EventSystem so pause/menu UI buttons work.
/// </summary>
[DefaultExecutionOrder(-250)]
public static class UIEventSystemBootstrap
{
    public static void EnsureExists()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}

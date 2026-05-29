using UnityEngine;

/// <summary>
/// Place on the invisible "StartObjective" trigger in the Forest scene.
/// Activates LevelManager objectives when the player enters the zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StartObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private bool oneShot = true;

    private bool triggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot)
            return;

        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerHealth>() == null)
            return;

        if (LevelManager.Instance == null)
            return;

        if (!LevelManager.Instance.TryActivateForestObjectives())
            return;

        triggered = true;
    }
}

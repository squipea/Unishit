using System.Collections;
using UnityEngine;

public class Teleporter : MonoBehaviour, IInteractable
{
    public Transform bossSpawnPoint;
    public Transform player;
    public void Interact()
    {
        
    }

    public void OnNotTouchingPlayer()
    {
        
    }

    public void OnTouchingPlayer()
    {
        StartCoroutine(TeleportPlayer());
    }

    private IEnumerator TeleportPlayer()
    {
        Debug.Log("Teleporting player to boss spawn point...");
        

        if (player != null && bossSpawnPoint != null)
        {
            yield return new WaitForSeconds(3f);
            player.transform.position = bossSpawnPoint.position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}

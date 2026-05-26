using UnityEngine;
using System.Collections;

public class TentacleManager : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject tentaclePrefab;
    public int targetCount = 5;
    public float respawnDelay = 3f;

    [Header("Spawn Area")]
    public float minX = 26f;
    public float maxX = 58f;
    public float spawnY = -3f;

    private bool isRespawning = false;

    private void Update()
    {
        // Don't do anything if crystals are gone
        if (Object.FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None).Length == 0)
        {
            return;
        }

        MaintainPopulation();
    }

    private void MaintainPopulation()
    {
        if (isRespawning) return;

        int currentCount = 0;
        TentacleBossAI[] tentacles = Object.FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None);
        foreach (var t in tentacles)
        {
            // Only count tentacles that are alive (not dead/dying/disabled)
            if (t.enabled)
            {
                currentCount++;
            }
        }

        if (currentCount < targetCount)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        // Final check before spawning: are crystals still alive and is there still room?
        if (Object.FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None).Length > 0)
        {
            int currentCount = 0;
            TentacleBossAI[] tentacles = Object.FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None);
            foreach (var t in tentacles)
            {
                if (t.enabled) currentCount++;
            }

            if (currentCount < targetCount)
            {
                SpawnTentacle();
            }
        }

        isRespawning = false;
    }

    private void SpawnTentacle()
    {
        if (tentaclePrefab == null)
        {
            Debug.LogWarning("TentacleManager: Tentacle Prefab is not assigned!");
            return;
        }

        float spawnX = Random.Range(minX, maxX);
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            float playerX = playerObj.transform.position.x;
            // Spawn within 8 units of the player, but stay within arena bounds
            spawnX = Mathf.Clamp(Random.Range(playerX - 8f, playerX + 8f), minX, maxX);
        }

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        GameObject newTentacle = Instantiate(tentaclePrefab, spawnPos, Quaternion.identity);
        newTentacle.name = "TentacleBoss_Regen";
        
        // Update Objective HUD if it exists
        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.UpdateUI();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnY, 0);
        Vector3 size = new Vector3(maxX - minX, 1f, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}

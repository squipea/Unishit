using UnityEngine;
using System.Collections;

public class TentacleManager : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject tentaclePrefab;
    public int targetCount = 5;

    [Header("Spawn Area")]
    public float minX = 26f;
    public float maxX = 58f;
    public float spawnY = -5.4f;

    private bool isSpawningWave;

    void Start()
    {
        if (CrystalBossController.Instance == null)
            EnsureInitialWave(targetCount);
    }

    public void EnsureInitialWave(int count)
    {
        EnsurePopulation(count);
    }

    public void SpawnWave(int count)
    {
        if (!isSpawningWave)
            StartCoroutine(SpawnWaveRoutine(count));
    }

    IEnumerator SpawnWaveRoutine(int count)
    {
        isSpawningWave = true;

        int alive = CountAliveTentacles();
        int toSpawn = Mathf.Max(0, count - alive);
        for (int i = 0; i < toSpawn; i++)
            SpawnTentacle();

        isSpawningWave = false;
        yield return null;
    }

    void EnsurePopulation(int desiredCount)
    {
        int alive = CountAliveTentacles();
        for (int i = alive; i < desiredCount; i++)
            SpawnTentacle();
    }

    int CountAliveTentacles()
    {
        int count = 0;
        TentacleBossAI[] tentacles = FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None);
        foreach (TentacleBossAI tentacle in tentacles)
        {
            if (tentacle != null && tentacle.enabled)
                count++;
        }
        return count;
    }

    void SpawnTentacle()
    {
        if (tentaclePrefab == null)
        {
            Debug.LogWarning("TentacleManager: Tentacle Prefab is not assigned!");
            return;
        }

        // Find all active tentacles to avoid spawning too close to them
        TentacleBossAI[] tentacles = FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None);
        System.Collections.Generic.List<float> occupiedX = new System.Collections.Generic.List<float>();
        foreach (TentacleBossAI t in tentacles)
        {
            if (t != null && t.enabled)
            {
                occupiedX.Add(t.transform.position.x);
            }
        }

        float spawnX = Random.Range(minX, maxX);
        float minDistance = 5.5f; // Ensure at least 5.5 units of space between tentacles
        int maxAttempts = 100;
        bool foundValidPosition = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float candidateX = Random.Range(minX, maxX);
            
            // Check distance from other tentacles
            bool tooClose = false;
            foreach (float x in occupiedX)
            {
                if (Mathf.Abs(candidateX - x) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnX = candidateX;
                foundValidPosition = true;
                break;
            }
        }

        if (!foundValidPosition)
        {
            // Fallback: choose a random spot in the spawn area that is at least 3.0 units away from others
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float candidateX = Random.Range(minX, maxX);
                bool tooClose = false;
                foreach (float x in occupiedX)
                {
                    if (Mathf.Abs(candidateX - x) < 3.0f)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (!tooClose)
                {
                    spawnX = candidateX;
                    foundValidPosition = true;
                    break;
                }
            }
        }

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        GameObject newTentacle = Instantiate(tentaclePrefab, spawnPos, Quaternion.identity);
        newTentacle.name = "TentacleBoss_Regen";

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnY, 0);
        Vector3 size = new Vector3(maxX - minX, 1f, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}

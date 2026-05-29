using System.Collections;
using UnityEngine;

public class LevelClearOnEnemiesDead : MonoBehaviour
{
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private float sceneChangeDelay = 1f;

    private bool levelCompleted;

    private void Start()
    {
        StartCoroutine(CheckEnemiesRoutine());
    }

    private IEnumerator CheckEnemiesRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (!levelCompleted)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

            if (enemies.Length == 0)
            {
                levelCompleted = true;

                Debug.Log("All enemies defeated. Loading next scene...");

                yield return new WaitForSeconds(sceneChangeDelay);

                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.CompleteCurrentState();
                }
                else
                {
                    Debug.LogError("GameFlowManager was not found in the scene.");
                }

                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }
}
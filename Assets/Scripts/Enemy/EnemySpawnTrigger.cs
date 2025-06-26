using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class EnemySpawnGroup
{
    public Transform spawnPoint;
    public List<EnemySpawnInfo> enemiesToSpawn;
}

public enum WaveTriggerType { ByTime, ByClear }

[System.Serializable]
public class EnemyWave
{
    public List<EnemySpawnGroup> spawnGroups;
    public WaveTriggerType triggerType = WaveTriggerType.ByTime;
    public float delay = 5f; // Только для ByTime
}

public class EnemySpawnTrigger : MonoBehaviour
{
    [Header("Волны врагов")]
    public List<EnemyWave> waves;
    [Header("Уникальный ID для сохранения")]
    public string uniqueID;
    private bool activated = false;
    private int currentWave = 0;
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        // Проверяем, был ли уже активирован этот триггер (загрузка из сейва)
        if (GameManager.CurrentSaveData != null)
        {
            var idx = GameManager.CurrentSaveData.interactiveObjectStates.FindIndex(s => s.uniqueID == uniqueID);
            if (idx >= 0 && GameManager.CurrentSaveData.interactiveObjectStates[idx].stateData == "activated")
            {
                activated = true;
                gameObject.SetActive(false); // Отключаем триггер
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;
        activated = true;
        StartCoroutine(SpawnWavesCoroutine());
        SaveActivatedState();
    }

    System.Collections.IEnumerator SpawnWavesCoroutine()
    {
        while (currentWave < waves.Count)
        {
            var wave = waves[currentWave];
            SpawnWave(wave);
            if (wave.triggerType == WaveTriggerType.ByTime)
            {
                yield return new WaitForSeconds(wave.delay);
                currentWave++;
            }
            else // ByClear
            {
                while (aliveEnemies.Exists(e => e != null))
                    yield return null;
                currentWave++;
            }
        }
        // Все волны завершены — можно отключить триггер
        gameObject.SetActive(false);
    }

    void SpawnWave(EnemyWave wave)
    {
        foreach (var group in wave.spawnGroups)
        {
            if (group.spawnPoint == null || group.enemiesToSpawn == null) continue;
            foreach (var info in group.enemiesToSpawn)
            {
                if (info.enemyPrefab == null || info.count <= 0) continue;
                for (int i = 0; i < info.count; i++)
                {
                    Vector3 pos = group.spawnPoint.position + Random.insideUnitSphere * 1.5f;
                    pos.y = group.spawnPoint.position.y;
                    GameObject enemy = Instantiate(info.enemyPrefab, pos, group.spawnPoint.rotation);
                    aliveEnemies.Add(enemy);
                    var ai = enemy.GetComponent<EnemyAI>();
                    if (ai != null && PlayerManager.instance != null && PlayerManager.instance.player != null)
                    {
                        ai.target = PlayerManager.instance.player.transform;
                        ai.isAggressive = true;
                    }
                    // Подписываемся на уничтожение врага для ByClear
                    var health = enemy.GetComponent<EnemyHealth>();
                    if (health != null && waves[currentWave].triggerType == WaveTriggerType.ByClear)
                    {
                        health.OnDeath += () => { aliveEnemies.Remove(enemy); };
                    }
                }
            }
        }
    }

    void SaveActivatedState()
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            GameManager.Instance.UpdateInteractiveObjectState(uniqueID, "activated");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Rigidbody2D enemyTarget;
    [SerializeField] private EnemyWaveSchedule waveSchedule;

    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PlayerStats playerStats;

    private readonly Dictionary<EnemyData, ObjectPool<EnemyCharacter>> pools = new();
    [SerializeField, Min(0)] private int defaultCapacity = 10;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

    private class ActiveWaveState
    {
        public EnemyWave Wave { get; }
        public int SpawnEntryIndex { get; set; }
        public int SpawnedCount { get; set; }
        public float SpawnTimer { get; set; }

        public bool IsCompleted => SpawnEntryIndex >= Wave.Entries.Length;

        public ActiveWaveState(EnemyWave wave)
        {
            Wave = wave;
            SpawnTimer = wave.SpawnInterval;
        }
    }

    private readonly List<ActiveWaveState> activeWaves = new();

    private int nextWaveIndex;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(EnemySpawner)}: {error}", this);
            enabled = false;
            return;
        }

        CreateEnemyPools();
    }

    private void CreateEnemyPools()
    {
        foreach (var wave in waveSchedule.Waves)
        {
            foreach (var entry in wave.Entries)
            {
                EnemyData data = entry.EnemyData;

                if (pools.ContainsKey(data)) continue;

                ObjectPool<EnemyCharacter> pool = null;

                pool = new ObjectPool<EnemyCharacter>(
                    () => CreateEnemy(pool, data),
                    null,
                    OnReturnedEnemy,
                    OnDestroyEnemy,
                    true, defaultCapacity, maxPoolSize);

                pools.Add(data, pool);
            }
        }
    }

    private EnemyCharacter Spawn(EnemyData data, Vector2 position)
    {
        EnemyCharacter enemy = pools[data].Get();
        enemy.Spawn(data, enemyTarget, position);

        return enemy;
    }

    private EnemyCharacter CreateEnemy(IObjectPool<EnemyCharacter> ownerPool, EnemyData data)
    {
        EnemyCharacter enemy = Instantiate(data.Prefab, transform);
        enemy.Init(ownerPool, OnEnemyDefeated);
        enemy.gameObject.SetActive(false);

        return enemy;
    }

    private void OnReturnedEnemy(EnemyCharacter enemy)
    {
        enemy.ResetForPool();
        enemy.transform.SetParent(transform);
    }

    private void OnDestroyEnemy(EnemyCharacter enemy)
    {
        Destroy(enemy.gameObject);
    }

    private void OnEnemyDefeated(int exp)
    {
        playerStats.AddExp(exp);
        playerStats.AddKillCount();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f) return;

        StartReadyWaves();
        UpdateActiveWaves(deltaTime);
    }

    private void StartReadyWaves()
    {
        EnemyWave[] waves = waveSchedule.Waves;

        while (nextWaveIndex < waves.Length &&
            gameTimer.ElapsedTime >= waves[nextWaveIndex].StartTime)
        {
            ActiveWaveState state = new ActiveWaveState(waves[nextWaveIndex]);
            activeWaves.Add(state);

            ++nextWaveIndex;
        }
    }

    private void UpdateActiveWaves(float deltaTime)
    {
        for (int i = activeWaves.Count - 1; i >= 0; --i)
        {
            ActiveWaveState state = activeWaves[i];

            UpdateWaveSpawning(state, deltaTime);

            if (state.IsCompleted)
            {
                activeWaves.RemoveAt(i);
            }
        }
    }

    private void UpdateWaveSpawning(ActiveWaveState state, float deltaTime)
    {
        if (state.IsCompleted) return;

        float spawnInterval = state.Wave.SpawnInterval;

        if (spawnInterval <= 0f)
        {
            while (!state.IsCompleted)
            {
                SpawnNextEnemy(state);
            }

            return;
        }

        state.SpawnTimer += deltaTime;

        while (state.SpawnTimer >= spawnInterval && !state.IsCompleted)
        {
            SpawnNextEnemy(state);
            state.SpawnTimer -= spawnInterval;
        }
    }

    private void SpawnNextEnemy(ActiveWaveState state)
    {
        EnemySpawnEntry entry = state.Wave.Entries[state.SpawnEntryIndex];

        Spawn(entry.EnemyData, GetRandomSpawnPosition());

        ++state.SpawnedCount;

        if (state.SpawnedCount >= entry.Count)
        {
            ++state.SpawnEntryIndex;
            state.SpawnedCount = 0;
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }

    private bool TryValidateSettings(out string error)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            error = $"{nameof(spawnPoints)} Invalid";
            return false;
        }

        for (int i = 0; i < spawnPoints.Length; ++i)
        {
            if (spawnPoints[i] == null)
            {
                error = $"{nameof(spawnPoints)} [{i}] Invalid";
                return false;
            }
        }

        if (enemyTarget == null)
        {
            error = $"{nameof(enemyTarget)} Invalid";
            return false;
        }

        if (waveSchedule == null)
        {
            error = $"{nameof(EnemyWaveSchedule)} is Invalid";
            return false;
        }

        if (gameTimer == null)
        {
            error = $"{nameof(gameTimer)} is Invalid";
            return false;
        }
        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} is Invalid";
            return false;
        }

        if (!waveSchedule.TryValidateSettings(out string waveScheduleError))
        {
            error = $"{nameof(waveSchedule)}'s {waveScheduleError}";
            return false;
        }

        if (defaultCapacity < 0 || maxPoolSize < 1)
        {
            error = $"{nameof(defaultCapacity)} or {nameof(maxPoolSize)} Invalid";
            return false;
        }

        error = null;
        return true;
    }
}

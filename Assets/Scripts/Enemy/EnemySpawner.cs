using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Rigidbody2D enemyTarget;
    [SerializeField] private EnemyWaveSchedule waveSchedule;

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

    private float elapsedTime;
    private int nextWaveIndex;

    private void Awake()
    {
        if (!TryValidateConfiguration(out string error))
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
        enemy.Init(ownerPool);
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

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f) return;

        elapsedTime += deltaTime;

        StartReadyWaves();
        UpdateActiveWaves(deltaTime);
    }

    private void StartReadyWaves()
    {
        EnemyWave[] waves = waveSchedule.Waves;

        while (nextWaveIndex < waves.Length &&
            elapsedTime >= waves[nextWaveIndex].StartTime)
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

    private bool TryValidateConfiguration(out string error)
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

        if (waveSchedule == null ||
            waveSchedule.Waves == null ||
            waveSchedule.Waves.Length == 0)
        {
            error = $"{nameof(EnemyWaveSchedule)} Invalid";
            return false;
        }

        for (int i = 0; i < waveSchedule.Waves.Length; ++i)
        {
            EnemyWave wave = waveSchedule.Waves[i];

            if (wave == null ||
                wave.Entries == null ||
                wave.Entries.Length == 0)
            {
                error = $"{nameof(EnemyWave)} [{i}] or {nameof(EnemyWave.Entries)} Invalid";
                return false;
            }

            if (wave.StartTime < 0f || wave.SpawnInterval < 0f)
            {
                error = $"Wave [{i}]'s {nameof(EnemyWave.StartTime)} or " +
                        $"{nameof(EnemyWave.SpawnInterval)} Invalid";
                return false;
            }

            if (i > 0 &&
                wave.StartTime < waveSchedule.Waves[i - 1].StartTime)
            {
                error = $"Wave [{i}] Start Time earlier than previous wave";
                return false;
            }

            for (int j = 0; j < wave.Entries.Length; ++j)
            {
                EnemySpawnEntry entry = wave.Entries[j];

                if (entry == null ||
                    entry.Count <= 0 ||
                    entry.EnemyData == null ||
                    entry.EnemyData.Prefab == null)
                {
                    error = $"{nameof(EnemyWave)} [{i}]'s {nameof(EnemySpawnEntry)} [{j}] Invalid";
                    return false;
                }

                EnemyData enemyData = entry.EnemyData;

                if (enemyData.MaxHp < 1)
                {
                    error = $"{nameof(EnemyWave)}[{i}]'s {nameof(EnemySpawnEntry)}[{j}]'s {nameof(EnemyData.MaxHp)} Invalid";
                    return false;
                }

                if (enemyData.MoveSpeed < 0f)
                {
                    error = $"{nameof(EnemyWave)} [{i}]'s {nameof(EnemySpawnEntry)} [{j}]'s {nameof(EnemyData.MoveSpeed)} Invalid";
                    return false;
                }
            }
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

using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    [SerializeField, Min(0f)] private float startTime;
    [SerializeField, Min(0f)] private float spawnInterval;
    [SerializeField] private EnemySpawnEntry[] entries;

    public float StartTime => startTime;
    public float SpawnInterval => spawnInterval;
    public EnemySpawnEntry[] Entries => entries;
}

[System.Serializable]
public class EnemySpawnEntry
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField, Min(1f)] private int count = 1;

    public EnemyData EnemyData => enemyData;
    public int Count => count;
}

[CreateAssetMenu(fileName = "EnemyWaveSchedule", menuName = "SurvivalLike/Enemy Wave Schedule")]
public class EnemyWaveSchedule : ScriptableObject
{
    [SerializeField] private EnemyWave[] waves;

    public EnemyWave[] Waves => waves;
}

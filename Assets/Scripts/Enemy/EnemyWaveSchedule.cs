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

    public bool TryValidateSettings(out string error)
    {
        if (startTime < 0f)
        {
            error = $"{nameof(startTime)} must be at least 0.0.";
            return false;
        }
        if (spawnInterval < 0f)
        {
            error = $"{nameof(spawnInterval)} must be at least 0.0.";
            return false;
        }

        if (entries == null)
        {
            error = $"{nameof(entries)} is Invalid.";
            return false;
        }
        for (int i = 0; i < entries.Length; ++i)
        {
            if (entries[i] == null)
            {
                error = $"{nameof(entries)}[{i}] is Invalid.";
                return false;
            }
            if (!entries[i].TryValidateSettings(out string entryError))
            {
                error = $"{nameof(entries)}[{i}]'s {entryError}";
                return false;
            }
        }

        error = null;
        return true;
    }
}

[System.Serializable]
public class EnemySpawnEntry
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField, Min(1)] private int count = 1;

    public EnemyData EnemyData => enemyData;
    public int Count => count;

    public bool TryValidateSettings(out string error)
    {
        if (enemyData == null)
        {
            error = $"{nameof(enemyData)} is Invalid.";
            return false;
        }
        if (!enemyData.TryValidateSettings(out string enemyError))
        {
            error = $"{nameof(enemyData)}'s {enemyError}";
            return false;
        }

        if (count < 1)
        {
            error = $"{nameof(count)} must be at least 1";
            return false;
        }

        error = null;
        return true;
    }
}

[CreateAssetMenu(fileName = "EnemyWaveSchedule", menuName = "SurvivalLike/Enemy Wave Schedule")]
public class EnemyWaveSchedule : ScriptableObject
{
    [SerializeField] private EnemyWave[] waves;

    public EnemyWave[] Waves => waves;

    public bool TryValidateSettings(out string error)
    {
        if (waves == null)
        {
            error = $"{nameof(waves)} is Invalid.";
            return false;
        }
        for (int i=0; i<waves.Length; ++i)
        {
            if (waves[i] == null)
            {
                error = $"{nameof(waves)}[{i}] is Invalid.";
                return false;
            }
            if (!waves[i].TryValidateSettings(out string waveError))
            {
                error = $"{nameof(waves)}[{i}]'s {waveError}";
                return false;
            }

            if (i > 0 && waves[i].StartTime < waves[i - 1].StartTime)
            {
                error = $"{nameof(waves)}[{i}]'s StartTime ({waves[i].StartTime}) " +
                        $"must be greater than or equal to " +
                        $"{nameof(waves)}[{i-1}]'s StartTime ({waves[i-1].StartTime}).";
                return false;
            }
        }

        error = null;
        return true;
    }
}

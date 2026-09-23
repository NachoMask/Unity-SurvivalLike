using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public const float MinimumMaxHp = 1f;
    public const float MinimumCurrentHp = 0f;
    public const float MinimumRecoveryAmount = 0f;
    public const int MinimumDefense = 0;
    public const int MinimumProjectileCountBonus = 0;
    private const float RecoveryInterval = 1f;

    public event Action<int> LevelChanged;
    public event Action<float, float> ExpChanged;
    public event Action<int> KillCountChanged;
    public event Action<float, float> HpChanged;

    private int level = 1;
    private float currentExp = 0f;
    private float maxExp = 5f;
    private int killCount = 0;

    private float maxHpBase = 100f;
    private float recoveryAmount = 0f;
    private int defense = 0;
    private float moveSpeedBase = 2f;
    private float invulnerabilityTimeBase = 0.25f;

    private float maxHpBonus = 0f;
    private int projectileCountBonus = 0;
    private float moveSpeedBonus = 0f;
    private float attackBonus = 0f;
    private float projectileSpeedBonus = 0f;
    private float activeDurationBonus = 0f;
    private float attackRangeBonus = 0f;
    private float cooldownReductionBonus = 0f;
    private float expBonus = 0f;
    private float invulnerabilityTimeBonus = 0f;

    public float MaxHpBase => maxHpBase;
    public float MaxHpBonus => maxHpBonus;
    public float MoveSpeedBonus => moveSpeedBonus;
    public float AttackBonus => attackBonus;
    public float ProjectileSpeedBonus => projectileSpeedBonus;
    public float ActiveDurationBonus => activeDurationBonus;
    public float AttackRangeBonus => attackRangeBonus;
    public float CooldownReductionBonus => cooldownReductionBonus;
    public float ExpBonus => expBonus;

    private bool isLevelUpPending;

    private float recoveryElapsedTime;

    public int Level => level;
    public float CurrentExp => currentExp;
    public float MaxExp => maxExp;
    public int KillCount => killCount;

    public float MaxHp => Mathf.Max(MinimumMaxHp, maxHpBase *(1 + maxHpBonus));
    public float RecoveryAmount => recoveryAmount;
    public int Defense => defense;
    public int ProjectileCountBonus => projectileCountBonus;
    public float MoveSpeed => moveSpeedBase * (1f + moveSpeedBonus);
    public float AttackMultiplier => (1f + attackBonus);
    public float ProjectileSpeedMultiplier => (1f + projectileSpeedBonus);
    public float ActiveDurationMultiplier => (1f + activeDurationBonus);
    public float AttackRangeMultiplier => (1f + attackRangeBonus);
    public float CooldownMultiplier => Mathf.Max(0.1f, 1f - cooldownReductionBonus);
    public float ExpMultiplier => (1f + expBonus);
    public float InvulnerabilityTime => invulnerabilityTimeBase + invulnerabilityTimeBonus;

    public float CurrentHp { get; private set; }

    public bool IsDead => CurrentHp <= 0f;
    public bool IsLevelUpPending => isLevelUpPending;

    private void Awake()
    {
        CurrentHp = MaxHp;
    }

    private void Update()
    {
        if (RecoveryAmount <= 0f ||
            CurrentHp <= MinimumCurrentHp ||
            CurrentHp >= MaxHp)
        {
            recoveryElapsedTime = 0f;
            return;
        }

        recoveryElapsedTime += Time.deltaTime;

        while (recoveryElapsedTime >= RecoveryInterval)
        {
            recoveryElapsedTime -= RecoveryInterval;
            Heal(RecoveryAmount);
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage));
        }

        float finalDamage = (float)Mathf.Max(1f, damage - Defense);

        SetCurrentHp(CurrentHp - finalDamage);
    }

    private void Heal(float healAmount)
    {
        if (healAmount <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(healAmount));
        }

        SetCurrentHp(CurrentHp + healAmount);
    }

    public void AddExp(int exp)
    {
        if (exp < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(exp));
        }
        if (exp == 0) return;

        float finalExp = exp * ExpMultiplier;

        currentExp += finalExp;

        TryBeginLevelUp();

        ExpChanged?.Invoke(currentExp, maxExp);
    }

    public void AddKillCount()
    {
        ++killCount;
        KillCountChanged?.Invoke(killCount);
    }

    private void TryBeginLevelUp()
    {
        if (isLevelUpPending) return;
        if (currentExp < maxExp) return;

        currentExp -= maxExp;

        ++level;
        maxExp += level * 2f;

        isLevelUpPending = true;
        LevelChanged?.Invoke(level);
    }

    public void CompleteLevelUp()
    {
        if (!isLevelUpPending)
        {
            throw new InvalidOperationException(
                "There is no pending LevelUp to complete");
        }

        isLevelUpPending = false;

        TryBeginLevelUp();

        ExpChanged?.Invoke(currentExp, maxExp);
    }

    public void ApplyPassiveUpgrade(PlayerPassiveDefinition.PassiveStat stat, float amount)
    {
        if (!Enum.IsDefined(typeof(PlayerPassiveDefinition.PassiveStat), stat))
            throw new ArgumentOutOfRangeException(nameof(stat));

        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (PlayerPassiveDefinition.IsIntegerStat(stat) &&
            !Mathf.Approximately(amount, Mathf.Round(amount)))
            throw new ArgumentException($"{stat} requires an integer amount.", nameof(amount));

        switch (stat)
        {
            case PlayerPassiveDefinition.PassiveStat.MaxHpBonus:
                AddMaxHpBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.RecoveryAmount:
                AddRecoveryAmount(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.Defense:
                AddDefense(Mathf.RoundToInt(amount));
                break;
            case PlayerPassiveDefinition.PassiveStat.ProjectileCountBonus:
                AddProjectileCountBonus(Mathf.RoundToInt(amount));
                break;
            case PlayerPassiveDefinition.PassiveStat.MoveSpeedBonus:
                AddMoveSpeedBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.AttackBonus:
                AddAttackBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.ProjectileSpeedBonus:
                AddProjectileSpeedBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.ActiveDurationBonus:
                AddActiveDurationBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.AttackRangeBonus:
                AddAttackRangeBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.CooldownReductionBonus:
                AddCooldownReductionBonus(amount);
                break;
            case PlayerPassiveDefinition.PassiveStat.ExpBonus:
                AddExpBonus(amount);
                break;
        }
    }

    private void AddMaxHpBonus(float value)
    {
        if (value == 0f) return;

        float prevMaxHp = MaxHp;

        maxHpBonus += value;
        CurrentHp *= MaxHp / prevMaxHp;

        HpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void SetCurrentHp(float newCurrentHp)
    {
        float prevCurrentHp = CurrentHp;

        if (newCurrentHp == prevCurrentHp) return;

        CurrentHp = Mathf.Clamp(newCurrentHp, MinimumCurrentHp, MaxHp);
        HpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void AddRecoveryAmount(float value)
    {
        float newValue = Mathf.Max(MinimumRecoveryAmount, recoveryAmount + value);

        if (newValue == recoveryAmount) return;

        recoveryAmount = newValue;
    }

    private void AddDefense(int value)
    {
        int newValue = Mathf.Max(MinimumDefense, defense + value);

        if (newValue == defense) return;

        defense = newValue;
    }

    private void AddProjectileCountBonus(int value)
    {
        int newValue = Mathf.Max(MinimumProjectileCountBonus, projectileCountBonus + value);

        if (newValue == projectileCountBonus) return;

        projectileCountBonus = newValue;
    }

    private void AddMoveSpeedBonus(float value)
    {
        if (value == 0) return;

        moveSpeedBonus += value;
    }

    private void AddAttackBonus(float value)
    {
        if (value == 0) return;

        attackBonus += value;
    }

    private void AddProjectileSpeedBonus(float value)
    {
        if (value == 0) return;

        projectileSpeedBonus += value;
    }

    private void AddActiveDurationBonus(float value)
    {
        if (value == 0) return;

        activeDurationBonus += value;
    }

    private void AddAttackRangeBonus(float value)
    {
        if (value == 0) return;

        attackRangeBonus += value;
    }

    private void AddCooldownReductionBonus(float value)
    {
        if (value == 0) return;

        cooldownReductionBonus += value;
    }

    private void AddExpBonus(float value)
    {
        if (value == 0) return;

        expBonus += value;
    }
}

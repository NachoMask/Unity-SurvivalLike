using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public const int MinimumMaxHp = 1;
    public const int MinimumCurrentHp = 0;
    public const int MinimumRecoveryAmount = 0;
    public const int MinimumDefense = 0;
    public const int MinimumProjectileCountBonus = 0;

    public event Action<int> LevelChanged;
    public event Action<float, float> ExpChanged;
    public event Action<int> KillCountChanged;
    public event Action<float, float> HpChanged;

    private int level = 1;
    private float currentExp = 0f;
    private float maxExp = 5f;
    private int killCount = 0;

    private int maxHpBase = 100;
    private int recoveryAmount = 0;
    private int defense = 0;
    private float moveSpeedBase = 2f;

    private int maxHpBonus = 0;
    private int projectileCountBonus = 0;
    private float moveSpeedBonus = 0f;
    private float attackBonus = 0f;
    private float projectileSpeedBonus = 0f;
    private float activeDurationBonus = 0f;
    private float attackRangeBonus = 0f;
    private float cooldownReductionBonus = 0f;
    private float expBonus = 0f;

    public int Level => level;
    public float CurrentExp => currentExp;
    public float MaxExp => maxExp;
    public int KillCount => killCount;

    public float MaxHp => Mathf.Max(MinimumMaxHp, maxHpBase + maxHpBonus);
    public int RecoveryAmount => recoveryAmount;
    public int Defense => defense;
    public int ProjectileCountBonus => projectileCountBonus;
    public float MoveSpeed => moveSpeedBase * (1f + moveSpeedBonus);
    public float AttackMultiplier => (1f + attackBonus);
    public float ProjectileSpeedMultiplier => (1f + projectileSpeedBonus);
    public float ActiveDurationMultiplier => (1f + activeDurationBonus);
    public float AttackRangeMultiplier => (1f + attackRangeBonus);
    public float CooldownMultiplier => Mathf.Max(0.1f, 1f - cooldownReductionBonus);
    public float ExpMultiplier => (1f + expBonus);

    public float CurrentHp { get; private set; }

    private void Awake()
    {
        CurrentHp = MaxHp;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(damage));
        }

        float finalDamage = Mathf.Max(1f, damage - Defense);

        SetCurrentHp(CurrentHp - finalDamage);
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

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;

            LevelUp();
        }

        ExpChanged?.Invoke(currentExp, maxExp);
    }

    public void AddKillCount()
    {
        ++killCount;
        KillCountChanged?.Invoke(killCount);
    }

    private void LevelUp()
    {
        ++level;
        maxExp *= 1.5f;

        LevelChanged?.Invoke(level);
    }

    private void AddMaxHpBonus(int value)
    {
        if (value == 0) return;

        float prevMaxHp = MaxHp;

        maxHpBonus += value;
        CurrentHp = Mathf.RoundToInt(CurrentHp * (MaxHp / prevMaxHp));

        HpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void SetCurrentHp(float newCurrentHp)
    {
        float prevCurrentHp = CurrentHp;

        if (newCurrentHp == prevCurrentHp) return;

        CurrentHp = Mathf.Clamp(newCurrentHp, MinimumCurrentHp, MaxHp);
        HpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void AddRecoveryAmount(int value)
    {
        int newValue = Mathf.Max(MinimumRecoveryAmount, recoveryAmount + value);

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

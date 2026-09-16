using UnityEngine;

[CreateAssetMenu(fileName = "PassiveDefinition", menuName = "SurvivalLike/Definition/PlayerPassive")]
public class PlayerPassiveDefinition : PlayerUpgradeDefinition
{
    public enum PassiveStat
    {
        MaxHpBonus,
        RecoveryAmount,
        Defense,
        ProjectileCountBonus,
        MoveSpeedBonus,
        AttackBonus,
        ProjectileSpeedBonus,
        ActiveDurationBonus,
        AttackRangeBonus,
        CooldownReductionBonus,
        ExpBonus
    }

    [SerializeField] private PassiveStat stat;
    [SerializeField][Min(0.01f)] private float amount;
    [SerializeField][Min(1)] private int maxLevel;

    public PassiveStat Stat => stat;
    public float Amount => amount;
    public int MaxLevel => maxLevel;

    public override bool TryValidateSettings(out string error)
    {
        if (!base.TryValidateSettings(out string baseError))
        {
            error = baseError;
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PassiveStat), stat))
        {
            error = $"{nameof(stat)} has an unsupported value: {stat}";
            return false;
        }

        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
        {
            error = $"{nameof(amount)} must be a finite value greater than 0.";
            return false;
        }
        if (IsIntegerStat(stat) && !Mathf.Approximately(amount, Mathf.Round(amount)))
        {
            error = $"{nameof(amount)} must be an integer for {stat}";
            return false;
        }

        if (maxLevel < 1)
        {
            error = $"{nameof(PlayerPassiveDefinition)}'s {nameof(maxLevel)} must be at least 1.";
            return false;
        }

        error = null;
        return true;
    }

    public static bool IsIntegerStat(PassiveStat stat)
    {
        return stat is
            PassiveStat.Defense or
            PassiveStat.ProjectileCountBonus;
    }
}

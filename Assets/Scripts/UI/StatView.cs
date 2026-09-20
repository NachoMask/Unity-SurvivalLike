using System;
using TMPro;
using UnityEngine;

public class StatView : MonoBehaviour
{
    public enum StatType
    {
        MaxHp,
        Recovery,
        Defense,
        MoveSpeed,
        Attack,
        ProjectileSpeed,
        ActiveDuration,
        AttackRange,
        CooldownReduction,
        ProjectileCount,
        Exp
    }

    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI valueText;

    public StatType Type => statType;

    public void Refresh(PlayerStats stats)
    {
        if (statType == StatType.MaxHp)
        {
            if (Mathf.Approximately(stats.MaxHpBonus, 0f))
            {
                valueText.color = Color.white;
                valueText.text = $"{stats.MaxHpBase}";
                return;
            }

            valueText.color = Color.yellow;
            valueText.text = $"{stats.MaxHp}";
        }
        else
        {
            if (!TryGetDisplayText(stats, out string text))
            {
                valueText.color = Color.white;
                valueText.text = "-";
                return;
            }

            valueText.color = Color.yellow;
            valueText.text = text;
        }
    }

    private bool TryGetDisplayText(PlayerStats stats, out string text)
    {
        switch (statType)
        {
            case StatType.Recovery:
                return TryFormatNumber(stats.RecoveryAmount, out text);
            case StatType.Defense:
                return TryFormatInteger(stats.Defense, out text);
            case StatType.MoveSpeed:
                return TryFormatPercent(stats.MoveSpeedBonus, out text);
            case StatType.Attack:
                return TryFormatPercent(stats.AttackBonus, out text);
            case StatType.ProjectileSpeed:
                return TryFormatPercent(stats.ProjectileSpeedBonus, out text);
            case StatType.ActiveDuration:
                return TryFormatPercent(stats.ActiveDurationBonus, out text);
            case StatType.AttackRange:
                return TryFormatPercent(stats.AttackRangeBonus, out text);
            case StatType.CooldownReduction:
                return TryFormatPercent(stats.CooldownReductionBonus, out text);
            case StatType.ProjectileCount:
                return TryFormatInteger(stats.ProjectileCountBonus, out text);
            case StatType.Exp:
                return TryFormatPercent(stats.ExpBonus, out text);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static bool TryFormatNumber(float value, out string text)
    {
        if (Mathf.Approximately(value, 0f))
        {
            text = string.Empty;
            return false;
        }

        text = $"{value:0.##}";
        return true;
    }

    private static bool TryFormatPercent(float value, out string text)
    {
        if (Mathf.Approximately(value, 0f))
        {
            text = string.Empty;
            return false;
        }

        text = value.ToString("+0%;-0%;0%");
        return true;
    }

    private static bool TryFormatInteger(int value, out string text)
    {
        if (value == 0)
        {
            text = string.Empty;
            return false;
        }

        text = value.ToString("+0;-0;0");
        return true;
    }

    public bool TryValidateSettings(out string error)
    {
        if (!Enum.IsDefined(typeof(StatType), statType))
        {
            error = $"{nameof(statType)} has an unsupported value: {statType}";
            return false;
        }
        if (valueText == null)
        {
            error = $"{nameof(valueText)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

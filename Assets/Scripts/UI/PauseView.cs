using System.Collections.Generic;
using UnityEngine;

public class PauseView : MonoBehaviour
{
    [SerializeField] private StatView[] statViews;
    [SerializeField] private OwnedUpgradeView[] attackViews;
    [SerializeField] private OwnedUpgradeView[] passiveViews;

    public void Show(PlayerStats stats,
        IReadOnlyDictionary<PlayerAttackDefinition, IPlayerAttackUpgradeable> attacks,
        IReadOnlyDictionary<PlayerPassiveDefinition, int> passives)
    {
        foreach (var view in statViews)
        {
            view.Refresh(stats);
        }

        BindAttacks(attacks);
        BindPassives(passives);
    }

    private void BindAttacks(IReadOnlyDictionary<PlayerAttackDefinition, IPlayerAttackUpgradeable> attacks)
    {
        foreach (var view in attackViews)
        {
            view.Clear();
        }

        int index = 0;

        foreach (var pair in attacks)
        {
            if (index >= attackViews.Length) return;

            PlayerAttackDefinition definition = pair.Key;
            IPlayerAttackUpgradeable attack = pair.Value;

            attackViews[index].Bind(definition.Icon, attack.Level, attack.MaxLevel);

            ++index;
        }
    }

    private void BindPassives(IReadOnlyDictionary<PlayerPassiveDefinition, int> passives)
    {
        foreach (var view in passiveViews)
        {
            view.Clear();
        }

        int index = 0;

        foreach (var pair in passives)
        {
            if (index >= passiveViews.Length) return;

            PlayerPassiveDefinition definition = pair.Key;
            int level = pair.Value;

            passiveViews[index].Bind(definition.Icon, level, definition.MaxLevel);

            ++index;
        }
    }

    public bool TryValidateSettings(out string error)
    {
        if (statViews == null)
        {
            error = $"{nameof(statViews)} is Invalid.";
            return false;
        }

        int requiredCount = System.Enum.GetValues(typeof(StatView.StatType)).Length;


        if (statViews.Length != requiredCount)
        {
            error = $"{nameof(statViews)}'s length must be {requiredCount}";
            return false;
        }

        HashSet<StatView.StatType> statTypeCheck = new();

        for (int i = 0; i < statViews.Length; ++i)
        {
            StatView view = statViews[i];

            if (view == null)
            {
                error = $"{nameof(statViews)}[{i}] is Invalid.";
                return false;
            }
            if (!statTypeCheck.Add(view.Type))
            {
                error = $"{nameof(view)} is duplicated type.";
                return false;
            }
            if (!view.TryValidateSettings(out string viewError))
            {
                error = $"{nameof(statViews)}[{i}]'s {viewError}";
                return false;
            }
        }

        HashSet<OwnedUpgradeView> upgradeViewCheck = new();

        if (attackViews == null)
        {
            error = $"{nameof(attackViews)} is Invalid.";
            return false;
        }
        if (attackViews.Length != 6)
        {
            error = $"{nameof(attackViews)}'s length must be 6";
            return false;
        }
        for (int i = 0; i < attackViews.Length; ++i)
        {
            OwnedUpgradeView view = attackViews[i];

            if (view == null)
            {
                error = $"{nameof(attackViews)}[{i}] is Invalid.";
                return false;
            }
            if (!upgradeViewCheck.Add(view))
            {
                error = $"{nameof(attackViews)} is duplicated UpgradeView.";
                return false;
            }
            if (!view.TryValidateSettings(out string viewError))
            {
                error = $"{nameof(attackViews)}[{i}]'s {viewError}";
                return false;
            }
        }

        if (passiveViews == null)
        {
            error = $"{nameof(passiveViews)} is Invalid.";
            return false;
        }
        if (passiveViews.Length != 6)
        {
            error = $"{nameof(passiveViews)}'s length must be 6";
            return false;
        }
        for (int i = 0; i < passiveViews.Length; ++i)
        {
            OwnedUpgradeView view = passiveViews[i];

            if (view == null)
            {
                error = $"{nameof(passiveViews)}[{i}] is Invalid.";
                return false;
            }
            if (!upgradeViewCheck.Add(view))
            {
                error = $"{nameof(passiveViews)} is duplicated UpgradeView.";
                return false;
            }
            if (!view.TryValidateSettings(out string viewError))
            {
                error = $"{nameof(passiveViews)}[{i}]'s {viewError}";
                return false;
            }
        }

        error = null;
        return true;
    }
}

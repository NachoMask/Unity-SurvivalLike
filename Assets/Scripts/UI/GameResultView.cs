using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private ResultUpgradeView[] attackViews;
    [SerializeField] private ResultUpgradeView[] passiveViews;

    public void Show(
        int elapsedSeconds, int level, int killCount,
        IReadOnlyDictionary<PlayerAttackDefinition, IPlayerAttackUpgradeable> attacks,
        IReadOnlyDictionary<PlayerPassiveDefinition, int> passives)
    {
        int min = elapsedSeconds / 60;
        int sec = elapsedSeconds % 60;

        timeText.text = $"{min:D2}:{sec:D2}";

        levelText.text = $"{level}";
        killCountText.text = $"{killCount}";

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
            if (index >= attackViews.Length) break;

            PlayerAttackDefinition definition = pair.Key;
            IPlayerAttackUpgradeable attack = pair.Value;

            attackViews[index].Bind(definition.Icon, attack.Level);

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
            if (index >= passiveViews.Length) break;

            PlayerPassiveDefinition definition = pair.Key;
            int level = pair.Value;

            passiveViews[index].Bind(definition.Icon, level);

            ++index;
        }
    }

    public bool TryValidateSettings(out string error)
    {
        if (timeText == null)
        {
            error = $"{nameof(timeText)} is Invalid.";
            return false;
        }
        if (levelText == null)
        {
            error = $"{nameof(levelText)} is Invalid.";
            return false;
        }
        if (killCountText == null)
        {
            error = $"{nameof(killCountText)} is Invalid.";
            return false;
        }

        HashSet<ResultUpgradeView> viewCheck = new();

        if (attackViews == null)
        {
            error = $"{nameof(attackViews)} is Invalid.";
            return false;
        }
        if (attackViews.Length != 6)
        {
            error = $"{nameof(attackViews)}' Length must be 6";
            return false;
        }
        for (int i = 0; i < attackViews.Length; ++i)
        {
            ResultUpgradeView view = attackViews[i];

            if (view == null)
            {
                error = $"{nameof(attackViews)}[{i}] is Invalid.";
                return false;
            }
            if (!viewCheck.Add(view))
            {
                error = $"{nameof(attackViews)}[{i}] is duplicated UpgradeView.";
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
            error = $"{nameof(passiveViews)}' Length must be 6";
            return false;
        }
        for (int i = 0; i < passiveViews.Length; ++i)
        {
            ResultUpgradeView view = passiveViews[i];

            if (view == null)
            {
                error = $"{nameof(passiveViews)}[{i}] is Invalid.";
                return false;
            }
            if (!viewCheck.Add(view))
            {
                error = $"{nameof(passiveViews)}[{i}] is duplicated UpgradeView.";
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

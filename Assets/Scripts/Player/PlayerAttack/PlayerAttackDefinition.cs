using UnityEngine;

[CreateAssetMenu(fileName = "AttackDefinition", menuName = "SurvivalLike/Definition/PlayerAttack")]
public class PlayerAttackDefinition : PlayerUpgradeDefinition
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab => prefab;

    public override bool TryValidateSettings(out string error)
    {
        if (!base.TryValidateSettings(out string baseError))
        {
            error = baseError;
            return false;
        }

        if (prefab == null)
        {
            error = $"{nameof(prefab)} is Invalid";
            return false;
        }
        if (!prefab.TryGetComponent(out IPlayerAttackUpgradeable _))
        {
            error = $"{nameof(prefab)} is't {nameof(IPlayerAttackUpgradeable)}";
            return false;
        }

        error = null;
        return true;
    }
}

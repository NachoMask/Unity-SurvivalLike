using UnityEngine;

public interface IPlayerAttackUpgradeable
{
    int Level { get; }
    int MaxLevel { get; }
    bool CanUpgrade { get; }
    string NextUpgradeDescription { get; }

    bool TryUpgrade();
}

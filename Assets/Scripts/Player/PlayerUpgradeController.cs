using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUpgradeController : MonoBehaviour
{
    private const int MaximumOwnedAttackCount = 6;
    private const int MaximumOwnedPassiveCount = 6;
    private const int MaximumUpgradeChoiceCount = 3;

    [SerializeField] private GameFlowController gameFlowController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Transform playerAttackRoot;

    [SerializeField] private OwnedUpgradeHUDView[] attackViews;
    [SerializeField] private OwnedUpgradeHUDView[] passiveViews;
    private int lastAttackIndex = 0;
    private int lastPassiveIndex = 0;

    [SerializeField] private GameObject levelupScreen;
    [SerializeField] private UpgradeMenuView[] upgradeMenus;

    [SerializeField] private PlayerUpgradeDefinition[] availableUpgradeDefinitions;
    [SerializeField] private PlayerAttackDefinition[] startingAttackDefinitions;

    private bool isSelectingUpgrade;

    private readonly List<PlayerUpgradeDefinition> upgradeCandidates = new();
    private readonly List<PlayerUpgradeDefinition> currentUpgradeList = new();

    private readonly Dictionary<PlayerAttackDefinition, IPlayerAttackUpgradeable> ownedAttacks = new();
    private readonly Dictionary<PlayerPassiveDefinition, int> ownedPassives = new();

    public IReadOnlyDictionary<PlayerAttackDefinition, IPlayerAttackUpgradeable> OwnedAttacks
        => ownedAttacks;
    public IReadOnlyDictionary<PlayerPassiveDefinition, int> OwnedPassives
        => ownedPassives;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerUpgradeController)} {name}: {error}", this);
            enabled = false;
            return;
        }

        foreach (var attackIcon in attackViews)
        {
            attackIcon.Clear();
        }
        foreach (var passiveIcon in passiveViews)
        {
            passiveIcon.Clear();
        }

        AcquireStartingAttacks();
    }

    private void AcquireStartingAttacks()
    {
        for (int i = 0; i < startingAttackDefinitions.Length; ++i)
        {
            AcquireAttack(startingAttackDefinitions[i]);
        }
    }

    private void OnEnable()
    {
        playerStats.LevelChanged += BeginUpgradeSelection;
        gameFlowController.GameOver += CancelUpgradeSelection;
        gameFlowController.GameClear += CancelUpgradeSelection;
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.LevelChanged -= BeginUpgradeSelection;
        }
        if (gameFlowController != null)
        {
            gameFlowController.GameOver -= CancelUpgradeSelection;
            gameFlowController.GameClear -= CancelUpgradeSelection;
        }
    }

    private void BeginUpgradeSelection(int level)
    {
        MakeUpgradeList();

        if (currentUpgradeList.Count == 0)
        {
            playerStats.CompleteLevelUp();
        }
        else
        {
            if (!gameFlowController.TryBeginUpgradeSelection()) return;

            isSelectingUpgrade = true;

            ShowUpgradeList();
        }
    }

    private void MakeUpgradeList()
    {
        upgradeCandidates.Clear();
        currentUpgradeList.Clear();

        CollectUpgradeCandidates();

        int choiceCount = Mathf.Min(MaximumUpgradeChoiceCount, upgradeCandidates.Count);

        for (int i = 0; i < choiceCount; ++i)
        {
            int randomIndex = UnityEngine.Random.Range(i, upgradeCandidates.Count);

            PlayerUpgradeDefinition temp = upgradeCandidates[i];
            upgradeCandidates[i] = upgradeCandidates[randomIndex];
            upgradeCandidates[randomIndex] = temp;

            currentUpgradeList.Add(upgradeCandidates[i]);

            UpgradeMenuView upgradeView = upgradeMenus[i];
            PlayerUpgradeDefinition definition = upgradeCandidates[i];

            switch (definition)
            {
                case PlayerAttackDefinition attackDefinition:
                    if (!ownedAttacks.TryGetValue(attackDefinition, out IPlayerAttackUpgradeable ownedAttack))
                    {
                        upgradeView.Bind(
                            definition.Icon,
                            definition.DisplayName,
                            "NEW",
                            definition.Description,
                            () => SelectUpgrade(definition));
                    }
                    else
                    {
                        upgradeView.Bind(
                            definition.Icon,
                            definition.DisplayName,
                            $"LV.{ownedAttack.Level + 1}",
                            ownedAttack.NextUpgradeDescription,
                            () => SelectUpgrade(definition));
                    }
                    break;

                case PlayerPassiveDefinition passiveDefinition:
                    int currentLevel =
                        ownedPassives.TryGetValue(passiveDefinition, out int level) ? level : 0;

                    upgradeView.Bind(
                        definition.Icon,
                        definition.DisplayName,
                        currentLevel == 0 ? "NEW" : $"LV.{currentLevel + 1}",
                        definition.Description,
                        () => SelectUpgrade(definition));
                    break;
            }
        }
    }

    private void CollectUpgradeCandidates()
    {
        foreach (var definition in availableUpgradeDefinitions)
        {
            switch (definition)
            {
                case PlayerAttackDefinition attackDefinition:
                    if (ownedAttacks.TryGetValue(attackDefinition, out IPlayerAttackUpgradeable ownedAttack))
                    {
                        if (ownedAttack.CanUpgrade)
                            upgradeCandidates.Add(definition);
                    }
                    else if (ownedAttacks.Count < MaximumOwnedAttackCount)
                    {
                        upgradeCandidates.Add(definition);
                    }
                    break;

                case PlayerPassiveDefinition passiveDefinition:
                    if (ownedPassives.TryGetValue(passiveDefinition, out int currentLevel))
                    {
                        if (currentLevel < passiveDefinition.MaxLevel)
                            upgradeCandidates.Add(definition);
                    }
                    else if (ownedPassives.Count < MaximumOwnedPassiveCount)
                    {
                        upgradeCandidates.Add(definition);
                    }
                    break;
            }
        }
    }

    private void ShowUpgradeList()
    {
        levelupScreen.SetActive(true);

        for (int i = 0; i < upgradeMenus.Length; ++i)
        {
            if (i >= currentUpgradeList.Count)
            {
                upgradeMenus[i].gameObject.SetActive(false);
                continue;
            }

            upgradeMenus[i].gameObject.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(upgradeMenus[0].Button.gameObject);
    }

    private void SelectUpgrade(PlayerUpgradeDefinition definition)
    {
        if (!isSelectingUpgrade) return;

        switch (definition)
        {
            case PlayerAttackDefinition attackDefinition:
                if (ownedAttacks.TryGetValue(attackDefinition, out IPlayerAttackUpgradeable upgradeable))
                {
                    if (!upgradeable.TryUpgrade())
                        throw new InvalidOperationException($"{attackDefinition.name} can't be upgraded.");
                }
                else
                {
                    AcquireAttack(attackDefinition);
                }
                break;

            case PlayerPassiveDefinition passiveDefinition:
                ApplyPassive(passiveDefinition);
                break;
        }

        EndUpgradeSelection();
        playerStats.CompleteLevelUp();
    }

    private void AcquireAttack(PlayerAttackDefinition definition)
    {
        GameObject newAttack = Instantiate(definition.Prefab, playerAttackRoot);

        IPlayerAttackUpgradeable attack
            = newAttack.GetComponent<IPlayerAttackUpgradeable>();

        OwnedUpgradeHUDView view = attackViews[lastAttackIndex];
        ++lastAttackIndex;
        view.Bind(definition.Icon);

        ownedAttacks[definition] = attack;
    }

    private void ApplyPassive(PlayerPassiveDefinition definition)
    {
        playerStats.ApplyPassiveUpgrade(definition.Stat, definition.Amount);

        int currentLevel = ownedPassives.TryGetValue(definition, out int level) ? level : 0;
        int nextLevel = currentLevel + 1;

        if (currentLevel == 0)
        {
            OwnedUpgradeHUDView view = passiveViews[lastPassiveIndex];
            ++lastPassiveIndex;
            view.Bind(definition.Icon);
        }

        ownedPassives[definition] = nextLevel;
    }

    private void EndUpgradeSelection()
    {
        if (!gameFlowController.TryEndUpgradeSelection()) return;

        EventSystem.current.SetSelectedGameObject(null);

        levelupScreen.SetActive(false);

        isSelectingUpgrade = false;
    }

    private void CancelUpgradeSelection()
    {
        isSelectingUpgrade = false;
        levelupScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private bool TryValidateSettings(out string error)
    {
        if (gameFlowController == null)
        {
            error = $"{nameof(gameFlowController)} is Invalid.";
            return false;
        }

        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} is Invalid";
            return false;
        }

        if (playerAttackRoot == null)
        {
            error = $"{nameof(playerAttackRoot)} is Invalid";
            return false;
        }

        HashSet<OwnedUpgradeHUDView> upgradeViewCheck = new();

        if (attackViews == null)
        {
            error = $"{nameof(attackViews)} is Invalid.";
            return false;
        }
        if (attackViews.Length != MaximumOwnedAttackCount)
        {
            error = $"{nameof(attackViews)}'s Length must be {MaximumOwnedAttackCount}";
            return false;
        }
        for (int i = 0; i < attackViews.Length; ++i)
        {
            OwnedUpgradeHUDView attackView = attackViews[i];

            if (attackView == null)
            {
                error = $"{nameof(attackViews)}[{i}] is Invalid.";
                return false;
            }
            if (!upgradeViewCheck.Add(attackView))
            {
                error = $"{nameof(attackViews)}[{i}] is duplicated UpgradeView.";
                return false;
            }

            if (!attackViews[i].TryValidateSettings(out string iconError))
            {
                error = $"{nameof(attackViews)}[{i}]'s {iconError}";
                return false;
            }
        }

        if (passiveViews == null)
        {
            error = $"{nameof(passiveViews)} is Invalid.";
            return false;
        }
        if (passiveViews.Length != MaximumOwnedPassiveCount)
        {
            error = $"{nameof(passiveViews)}'s Length must be {MaximumOwnedPassiveCount}";
            return false;
        }
        for (int i = 0; i < passiveViews.Length; ++i)
        {
            OwnedUpgradeHUDView passiveView = passiveViews[i];

            if (passiveView == null)
            {
                error = $"{nameof(passiveViews)}[{i}] is Invalid.";
                return false;
            }
            if (!upgradeViewCheck.Add(passiveView))
            {
                error = $"{nameof(passiveViews)}[{i}] is duplicated UpgradeView.";
                return false;
            }

            if (!passiveViews[i].TryValidateSettings(out string iconError))
            {
                error = $"{nameof(passiveViews)}[{i}]'s {iconError}";
                return false;
            }
        }

        if (levelupScreen == null)
        {
            error = $"{nameof(levelupScreen)} is Invalid";
            return false;
        }

        if (upgradeMenus == null)
        {
            error = $"{nameof(upgradeMenus)} is Invalid";
            return false;
        }
        if (upgradeMenus.Length < MaximumUpgradeChoiceCount)
        {
            error = $"{nameof(upgradeMenus)}'s Length must be at least {MaximumUpgradeChoiceCount}";
            return false;
        }
        for (int i = 0; i < upgradeMenus.Length; ++i)
        {
            if (upgradeMenus[i] == null)
            {
                error = $"{nameof(upgradeMenus)}[{i}] is Invalid";
                return false;
            }
            if (!upgradeMenus[i].TryValidateSettings(out string menuError))
            {
                error = $"{nameof(upgradeMenus)}[{i}]'s {menuError}";
                return false;
            }
        }

        if (EventSystem.current == null)
        {
            error = $"{nameof(EventSystem.current)} is Invalid";
            return false;
        }

        HashSet<PlayerUpgradeDefinition> availableUpgradesCheck = new();

        if (availableUpgradeDefinitions == null)
        {
            error = $"{nameof(availableUpgradeDefinitions)} is Invalid";
            return false;
        }
        for (int i = 0; i < availableUpgradeDefinitions.Length; ++i)
        {
            PlayerUpgradeDefinition definition = availableUpgradeDefinitions[i];

            if (definition == null)
            {
                error = $"{nameof(availableUpgradeDefinitions)}[{i}] is Invalid";
                return false;
            }
            if (availableUpgradesCheck.Contains(definition))
            {
                error = $"{nameof(availableUpgradeDefinitions)}[{i}] is duplicated definition.";
                return false;
            }

            availableUpgradesCheck.Add(definition);

            if (!definition.TryValidateSettings(out string definitionError))
            {
                error = $"{nameof(availableUpgradeDefinitions)}[{i}]'s {definitionError}";
                return false;
            }
        }

        HashSet<PlayerAttackDefinition> startingAttacksCheck = new();

        if (startingAttackDefinitions == null)
        {
            error = $"{nameof(startingAttackDefinitions)} is Invalid";
            return false;
        }
        if (startingAttackDefinitions.Length > MaximumOwnedAttackCount)
        {
            error = $"{nameof(startingAttackDefinitions)} can't contain more than {MaximumOwnedAttackCount} definitions.";
            return false;
        }

        for (int i = 0; i < startingAttackDefinitions.Length; ++i)
        {
            PlayerAttackDefinition definition = startingAttackDefinitions[i];

            if (definition == null)
            {
                error = $"{nameof(startingAttackDefinitions)}[{i}] is Invalid";
                return false;
            }
            if (startingAttacksCheck.Contains(definition))
            {
                error = $"{nameof(startingAttackDefinitions)}[{i}] is duplicated definition.";
                return false;
            }
            if (!availableUpgradesCheck.Contains(definition))
            {
                error = $"{nameof(startingAttackDefinitions)}[{i}] is not include in {nameof(availableUpgradeDefinitions)}.";
                return false;
            }

            startingAttacksCheck.Add(definition);

            if (!definition.TryValidateSettings(out string definitionError))
            {
                error = $"{nameof(startingAttackDefinitions)}[{i}]'s {definitionError}";
                return false;
            }
        }

        error = null;
        return true;
    }
}

using System;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        SelectingUpgrade,
        GameOver
    }

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject gameOverScreen;

    public event Action GameOver;

    public GameState State { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(GameFlowController)} {name}: {error}", this);
            enabled = false;
            return;
        }

        gameOverScreen.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        playerStats.HpChanged += OnHpChanged;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.HpChanged -= OnHpChanged;
    }

    public bool TryBeginUpgradeSelection()
    {
        if (State == GameState.GameOver) return false;

        if (State != GameState.Playing)
            throw new InvalidOperationException($"Can't begin upgrade selection in state {State}.");

        SetState(GameState.SelectingUpgrade);
        return true;
    }

    public bool TryEndUpgradeSelection()
    {
        if (State == GameState.GameOver) return false;

        if (State != GameState.SelectingUpgrade)
            throw new InvalidOperationException($"Can't end upgrade selection in state {State}.");

        SetState(GameState.Playing);
        return true;
    }

    private void OnHpChanged(float currentHp, float maxHp)
    {
        if (currentHp > 0f || State == GameState.GameOver) return;

        SetState(GameState.GameOver);

        GameOver?.Invoke();
        gameOverScreen.SetActive(true);
    }

    private void SetState(GameState state)
    {
        State = state;

        bool isPlaying = state == GameState.Playing;

        Time.timeScale = isPlaying ? 1f : 0f;
        playerController.enabled = isPlaying;
    }

    private bool TryValidateSettings(out string error)
    {
        if (playerController == null)
        {
            error = $"{nameof(playerController)} is Invalid.";
            return false;
        }
        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} is Invalid.";
            return false;
        }
        if (gameOverScreen == null)
        {
            error = $"{nameof(gameOverScreen)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

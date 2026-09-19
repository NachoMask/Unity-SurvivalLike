using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        SelectingUpgrade,
        GameOver
    }

    [Header("# Playing")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerUpgradeController playerUpgradeController;

    [Header("# GameOver")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button quitButton;

    [Header("# Results")]
    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameResultView gameResultView;
    [SerializeField] private Button doneButton;

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

        EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
    }

    private void SetState(GameState state)
    {
        State = state;

        bool isPlaying = state == GameState.Playing;

        Time.timeScale = isPlaying ? 1f : 0f;
        playerController.enabled = isPlaying;
    }

    public void ShowResults()
    {
        gameOverScreen.SetActive(false);

        gameResultView.Show(gameTimer.ElapsedSeconds, playerStats.Level, playerStats.KillCount,
            playerUpgradeController.OwnedAttacks, playerUpgradeController.OwnedPassives);

        resultScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(doneButton.gameObject);
    }

    public void EnterTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    private bool TryValidateSettings(out string error)
    {
        if (gameTimer == null)
        {
            error = $"{nameof(gameTimer)} is Invalid.";
            return false;
        }
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
        if (playerUpgradeController == null)
        {
            error = $"{nameof(playerUpgradeController)} is Invalid.";
            return false;
        }
        if (gameOverScreen == null)
        {
            error = $"{nameof(gameOverScreen)} is Invalid.";
            return false;
        }
        if (quitButton == null)
        {
            error = $"{nameof(quitButton)} is Invalid.";
            return false;
        }
        if (resultScreen == null)
        {
            error = $"{nameof(resultScreen)} is Invalid.";
            return false;
        }

        if (gameResultView == null)
        {
            error = $"{nameof(gameResultView)} is Invalid.";
            return false;
        }
        if (!gameResultView.TryValidateSettings(out string viewError))
        {
            error = $"{nameof(gameResultView)}'s {viewError}";
            return false;
        }

        if (doneButton == null)
        {
            error = $"{nameof(doneButton)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

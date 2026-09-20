using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameFlowController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        SelectingUpgrade,
        Paused,
        GameOver
    }

    [Header("# Playing")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerUpgradeController playerUpgradeController;

    [Header("# Pause")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private PauseView pauseView;
    [SerializeField] private Button continueButton;
    [SerializeField] private InputActionReference pauseActionRef;

    private InputAction pauseAction;

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
        pauseAction = pauseActionRef?.action;

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(GameFlowController)} {name}: {error}", this);
            enabled = false;
            return;
        }

        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        playerStats.HpChanged += OnHpChanged;
        pauseAction.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.HpChanged -= OnHpChanged;

        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        switch (State)
        {
            case GameState.Playing:
                TryBeginPause();
                break;
            case GameState.Paused:
                TryEndPause();
                break;

            case GameState.SelectingUpgrade:
            case GameState.GameOver:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryBeginPause()
    {
        if (State == GameState.GameOver) return false;

        if (State != GameState.Playing)
            throw new InvalidOperationException($"Can't begin pause in state {State}.");

        SetState(GameState.Paused);

        ShowPauseScreen();

        return true;
    }

    private bool TryEndPause()
    {
        if (State == GameState.GameOver) return false;

        if (State != GameState.Paused)
            throw new InvalidOperationException($"Can't end pause in state {State}.");

        HidePauseScreen();

        SetState(GameState.Playing);

        return true;
    }

    public void ContinueGame()
    {
        if (State != GameState.Paused) return;

        TryEndPause();
    }

    public void ExitPausedGame()
    {
        if (State != GameState.Paused) return;

        HidePauseScreen();
        SetState(GameState.GameOver);
        ShowResults();
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

    private void ShowPauseScreen()
    {
        pauseView.Show(playerStats, playerUpgradeController.OwnedAttacks, playerUpgradeController.OwnedPassives);

        pauseScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    private void HidePauseScreen()
    {
        EventSystem.current.SetSelectedGameObject(null);
        pauseScreen.SetActive(false);
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

        if (pauseScreen == null)
        {
            error = $"{nameof(pauseScreen)} is Invalid.";
            return false;
        }
        if (pauseView == null)
        {
            error = $"{nameof(pauseView)} is Invalid.";
            return false;
        }
        if (!pauseView.TryValidateSettings(out string pauseViewError))
        {
            error = $"{nameof(pauseView)}'s {pauseViewError}";
            return false;
        }
        if (continueButton == null)
        {
            error = $"{nameof(continueButton)} is Invalid.";
            return false;
        }
        if (pauseActionRef == null)
        {
            error = $"{nameof(pauseActionRef)} is Invalid.";
            return false;
        }
        if (pauseAction == null)
        {
            error = $"{nameof(pauseAction)} is Invalid.";
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
        if (!gameResultView.TryValidateSettings(out string resultViewError))
        {
            error = $"{nameof(gameResultView)}'s {resultViewError}";
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

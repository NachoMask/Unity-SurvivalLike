using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PlayerStats playerStats;

    [Header("# Progress")]
    [SerializeField] private Slider expMeter;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("# Game")]
    [SerializeField] private Slider hpMeter;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(HUD)} {name}: {error}");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        UpdateHpMeter(playerStats.CurrentHp, playerStats.MaxHp);
    }

    private void OnEnable()
    {
        gameTimer.ElapsedSecondsChanged += UpdateTimeText;
        
        playerStats.ExpChanged += UpdateExpMeter;
        playerStats.LevelChanged += UpdateLevelText;
        playerStats.KillCountChanged += UpdateKillCount;
        playerStats.HpChanged += UpdateHpMeter;

        UpdateTimeText(gameTimer.ElapsedSeconds);
        UpdateExpMeter(playerStats.CurrentExp, playerStats.MaxExp);
        UpdateLevelText(playerStats.Level);
        UpdateKillCount(playerStats.KillCount);
        UpdateHpMeter(playerStats.CurrentHp, playerStats.MaxHp);
    }

    private void OnDisable()
    {
        if (gameTimer != null)
            gameTimer.ElapsedSecondsChanged -= UpdateTimeText;

        if (playerStats != null)
        {
            playerStats.ExpChanged -= UpdateExpMeter;
            playerStats.LevelChanged -= UpdateLevelText;
            playerStats.KillCountChanged -= UpdateKillCount;
            playerStats.HpChanged -= UpdateHpMeter;
        }
    }

    private void UpdateTimeText(int time)
    {
        int min = time / 60;
        int sec = time % 60;

        timeText.text = $"{min:D2}:{sec:D2}";
    }

    private void UpdateExpMeter(float currentExp, float maxExp)
    {
        expMeter.value = (float)currentExp / maxExp;
    }

    private void UpdateLevelText(int level)
    {
        levelText.text = $"Lv.{level}";
    }

    private void UpdateKillCount(int killCount)
    {
        killCountText.text = $"{killCount}";
    }

    private void UpdateHpMeter(int currentHp, int maxHp)
    {
        hpMeter.value = (float)currentHp / maxHp;
    }

    private bool TryValidateSettings(out string error)
    {
        if (gameTimer == null)
        {
            error = $"{nameof(gameTimer)} is Invalid.";
            return false;
        }
        if (playerStats == null)
        {
            error = $"{ nameof(playerStats)} is Invalid.";
            return false;
        }

        if (expMeter == null)
        {
            error = $"{nameof(expMeter)} is Invalid.";
            return false;
        }
        if (levelText == null)
        {
            error = $"{nameof(levelText)} is Invalid.";
            return false;
        }
        if (hpMeter == null)
        {
            error = $"{nameof(hpMeter)} is Invalid.";
            return false;
        }
        if (killCountText == null)
        {
            error = $"{nameof(killCountText)} is Invalid.";
            return false;
        }
        if (timeText == null)
        {
            error = $"{nameof(timeText)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

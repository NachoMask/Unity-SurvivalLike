using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OwnedUpgradeView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI levelText;

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        levelText.text = string.Empty;
    }

    public void Bind(Sprite icon, int currentLevel, int maxLevel)
    {
        iconImage.sprite = icon;
        iconImage.enabled = true;
        SetLevel(currentLevel, maxLevel);
    }

    public void SetLevel(int currentLevel, int maxLevel)
    {
        levelText.text = $"{currentLevel}/{maxLevel}";
    }

    public bool TryValidateSettings(out string error)
    {
        if (iconImage == null)
        {
            error = $"{nameof(iconImage)} is Invalid.";
            return false;
        }
        if (levelText == null)
        {
            error = $"{nameof(levelText)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

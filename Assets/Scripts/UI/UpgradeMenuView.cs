using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeMenuView : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject cursor;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public Button Button => button;

    private void OnEnable()
    {
        cursor.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        cursor.gameObject.SetActive(false);
    }

    public void Bind(
        Sprite icon,
        string displayName,
        string level,
        string description,
        Action onSelected)
    {
        iconImage.sprite = icon;
        nameText.text = displayName;
        levelText.text = level;
        descriptionText.text = description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected());
    }

    public void OnSelect(BaseEventData eventData)
    {
        cursor.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        cursor.gameObject.SetActive(false);
    }

    internal bool TryValidateSettings(out string error)
    {
        if (button == null)
        {
            error = $"{nameof(button)} is invalid.";
            return false;
        }
        if (cursor == null)
        {
            error = $"{nameof(cursor)} is invalid.";
            return false;
        }
        if (iconImage == null)
        {
            error = $"{nameof(iconImage)} is invalid.";
            return false;
        }
        if (nameText == null)
        {
            error = $"{nameof(nameText)} is invalid.";
            return false;
        }
        if (levelText == null)
        {
            error = $"{nameof(levelText)} is invalid.";
            return false;
        }
        if (descriptionText == null)
        {
            error = $"{nameof(descriptionText)} is invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

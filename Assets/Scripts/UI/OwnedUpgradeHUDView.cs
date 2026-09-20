using UnityEngine;
using UnityEngine.UI;

public class OwnedUpgradeHUDView : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void Bind(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = true;
    }

    public bool TryValidateSettings(out string error)
    {
        if (iconImage == null)
        {
            error = $"{nameof(iconImage)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

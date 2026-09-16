using UnityEngine;

public abstract class PlayerUpgradeDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField][TextArea] private string description;
    [SerializeField] private Sprite icon;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public virtual bool TryValidateSettings(out string error)
    {
        if (displayName == null)
        {
            error = $"{nameof(displayName)} is invalid";
            return false;
        }
        if (description == null)
        {
            error = $"{nameof(description)} is invalid";
            return false;
        }
        if (icon == null)
        {
            error = $"{nameof(icon)} is invalid";
            return false;
        }

        error = null;
        return true;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonView : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject cursor;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(MenuButtonView)} {name} : {error}", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        cursor.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (cursor != null)
            cursor.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        cursor.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        cursor.gameObject.SetActive(false);
    }

    private bool TryValidateSettings(out string error)
    {
        if (cursor == null)
        {
            error = $"{nameof(cursor)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

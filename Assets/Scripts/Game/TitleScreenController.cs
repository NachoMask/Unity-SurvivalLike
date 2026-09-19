using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private InputSystemUIInputModule uiInputModule;

    [SerializeField] private TextMeshProUGUI pressToStartText;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button startButton;

    private bool isWaitingForStartInput = true;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(TitleScreenController)} {name} : {error}", this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        EnterTitle();
    }

    private void Update()
    {
        if (!isWaitingForStartInput) return;

        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            isWaitingForStartInput = false;
            StartCoroutine(ShowMenu());
        }
    }

    private void EnterTitle()
    {
        Time.timeScale = 1f;
        isWaitingForStartInput = true;

        uiInputModule.enabled = false;
        EventSystem.current.SetSelectedGameObject(null);

        pressToStartText.gameObject.SetActive(true);
        menuRoot.SetActive(false);
    }

    private IEnumerator ShowMenu()
    {
        pressToStartText.gameObject.SetActive(false);
        menuRoot.SetActive(true);

        while (Keyboard.current != null &&
            Keyboard.current.anyKey.isPressed)
        {
            yield return null;
        }

        yield return null;

        uiInputModule.enabled = true;
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    private bool TryValidateSettings(out string error)
    {
        if (uiInputModule == null)
        {
            error = $"{nameof(uiInputModule)} is Invalid.";
            return false;
        }

        if (pressToStartText == null)
        {
            error = $"{nameof(pressToStartText)} is Invalid.";
            return false;
        }

        if (menuRoot == null)
        {
            error = $"{nameof(menuRoot)} is Invalid.";
            return false;
        }

        if (startButton == null)
        {
            error = $"{nameof(startButton)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}

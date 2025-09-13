using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu("VPSV/GameEndUI (TMP)")]
public class GameEndUI_TMP : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public Button restartButton;
    public Button quitButton;

    [Header("Text Presets")]
    public string winTitle = "VICTORY!";
    [TextArea] public string subtitle = "Thanks for playing!";

    [Header("Options")]
    public string mainMenuSceneName = "";   // để trống = Quit khi build
    public float fadeDuration = 0.25f;
    public bool allowAnyKeyRestart = false;

    bool shown;

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        HideInstant();
        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (quitButton) quitButton.onClick.AddListener(QuitToMenu);
    }

    void Update()
    {
        if (shown && allowAnyKeyRestart && Input.anyKeyDown) Restart();
    }

    // === API ===
    public void ShowWin() => ShowWith(winTitle, subtitle);

    public void ShowWith(string title, string sub)
    {
        if (titleText) titleText.text = title;
        if (subtitleText) subtitleText.text = sub;
        StopAllCoroutines();
        StartCoroutine(Fade(1f));
    }

    public void HideInstant()
    {
        if (!canvasGroup) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        shown = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Application.Quit();
    }

    System.Collections.IEnumerator Fade(float target)
    {
        if (!canvasGroup) yield break;
        float start = canvasGroup.alpha, t = 0f;
        canvasGroup.blocksRaycasts = target > 0.5f;
        canvasGroup.interactable = target > 0.5f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // vẫn fade khi pause
            canvasGroup.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        canvasGroup.alpha = target;
        shown = target >= 0.99f;
    }
}

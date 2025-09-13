using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Đừng quên mở lại time
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // thoát Play Mode trong Editor
#elif UNITY_WEBGL
    Debug.Log("Quit không hỗ trợ trên WebGL");
#else
    Application.Quit(); // build PC/Mobile thì thoát app
#endif
    }
}

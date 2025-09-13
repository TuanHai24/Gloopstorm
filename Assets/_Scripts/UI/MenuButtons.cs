using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public string gameplaySceneName = "Game";  // điền tên scene chơi

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
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

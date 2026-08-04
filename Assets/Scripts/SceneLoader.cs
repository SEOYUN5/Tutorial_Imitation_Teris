using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameSScene");
    }

    public static void LoadLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }

    public static void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
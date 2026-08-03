using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    public void ShowResult(int score)
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        bool isNewRecord = score > highScore;

        if (isNewRecord)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        finalScoreText.text = "SCORE: " + score;
        highScoreText.text = "BEST: " + highScore;
    }

    private void OnRetry()
    {
        SceneLoader.LoadGame();
    }

    private void OnMainMenu()
    {
        SceneLoader.LoadLobby();
    }
}
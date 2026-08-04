using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private BlockPreviewRenderer nextPreview;
    [SerializeField] private BlockPreviewRenderer holdPreview;
    [SerializeField] private GameOverController gameOverController;

    private int displayedScore;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        displayedScore = score;
        scoreText.text = $"SCORE\n{displayedScore}";
    }

    public void UpdateNext(Tetromino.TetrominoType type)
    {
        nextPreview?.ShowBlock(type);
    }

    public void UpdateHold(Tetromino.TetrominoType? type)
    {
        holdPreview?.ShowBlock(type);
    }

    public void ShowGameOver(int score)
    {
        gameOverPanel.SetActive(true);
        if (gameOverController != null)
            gameOverController.ShowResult(score);
    }

    public void ShowPause(bool isPaused) => pausePanel.SetActive(isPaused);
}
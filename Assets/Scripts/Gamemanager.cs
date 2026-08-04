using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private LiquidGaugeController liquidGauge;
    [SerializeField] private BackgroundController backgroundController;
    [SerializeField] private AudioClip gameOverBGM;

    private IAudioService audioService;
    private int score;
    private int level = 1;
    private int totalClearedLines = 0;

    private enum GameState { Ready, Playing, Paused, LineClearing, GameOver }
    private GameState state = GameState.Ready;

    private float fallInterval = 1.0f;
    private float fallTimer = 0f;

    private void Awake()
    {
        audioService = musicManager;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        score = 0;
        level = 1;
        totalClearedLines = 0;
        fallInterval = 1.0f;
        uiManager.UpdateScore(score);

        if (liquidGauge != null)
            liquidGauge.PlayInitialBGM();

        state = GameState.Playing;
        SpawnNext();
    }

    private void Update()
    {
        if (state != GameState.Playing) return;
        HandleInput();
        HandleFall();
    }

    private void HandleInput()
    {
        InputManager.Direction dir = inputManager.GetMoveInput();
        if (dir != InputManager.Direction.None)
        {
            bool moved = board.MoveBlock(dir);
            if (moved)
            {
                audioService.PlaySFX("Move");
                RefreshRender();
            }
        }

        if (inputManager.GetRotateCW())
        {
            bool rotated = board.RotateBlock(true);
            if (rotated)
            {
                audioService.PlaySFX("Rotate");
                RefreshRender();
            }
        }

        if (inputManager.GetRotateCCW())
        {
            bool rotated = board.RotateBlock(false);
            if (rotated)
            {
                audioService.PlaySFX("Rotate");
                RefreshRender();
            }
        }

        if (inputManager.GetHardDropInput())
        {
            audioService.PlaySFX("Drop");

            int typeIndex = (int)board.currentTetromino.type;
            Vector2Int[] finalPositions = board.HardDropAndGetPositions();

            boardRenderer.LockSprites(finalPositions, typeIndex);

            List<int> clearedRows = board.CheckLines();
            if (clearedRows.Count > 0)
            {
                foreach (int row in clearedRows)
                    boardRenderer.DropLockedSprite(row);
                score += CalculateScore(clearedRows.Count);
                uiManager.UpdateScore(score);
                UpdateLevel(clearedRows.Count);

                if (clearedRows.Count >= 4)
                    audioService.PlaySFX("TetrisClear");
                else
                    audioService.PlaySFX("LineClear");

                if (liquidGauge != null)
                    liquidGauge.OnCombo(clearedRows.Count);
            }
            SpawnNext();
            RefreshRender();
        }

        // Hold
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            bool held = board.HoldBlock();
            if (held)
            {
                audioService.PlaySFX("Hold");
                uiManager.UpdateNext(board.nextType);
                uiManager.UpdateHold(board.holdType);
                RefreshRender();
            }
            else
            {
                audioService.PlaySFX("NoHold");
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (state == GameState.Playing) Pause();
            else if (state == GameState.Paused) Resume();
        }
        //debug
        //if (Keyboard.current.f1Key.wasPressedThisFrame)
        //{
        //    score += CalculateScore(4) + 100;
        //    uiManager.UpdateScore(score);
        //    if (liquidGauge != null)
        //        liquidGauge.OnCombo(5);
        //    Debug.Log("[디버그] 5줄 클리어 시뮬");
        //}
    }


    private void HandleFall()
    {
        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
        {
            fallTimer = 0f;
            bool locked = board.StepDown();

            if (locked)
            {
                audioService.PlaySFX("Drop");

                int typeIndex = (int)board.currentTetromino.type;
                boardRenderer.LockSprites(board.currentTetromino.positions, typeIndex);

                List<int> clearedRows = board.CheckLines();
                if (clearedRows.Count > 0)
                {
                    foreach (int row in clearedRows)
                        boardRenderer.DropLockedSprite(row);
                    score += CalculateScore(clearedRows.Count);
                    uiManager.UpdateScore(score);
                    UpdateLevel(clearedRows.Count);

                    if (clearedRows.Count >= 4)
                        audioService.PlaySFX("TetrisClear");
                    else
                        audioService.PlaySFX("LineClear");

                    if (liquidGauge != null)
                        liquidGauge.OnCombo(clearedRows.Count);
                }
                SpawnNext();
            }

            RefreshRender();
        }
    }

    private void UpdateLevel(int clearedLineCount)
    {
        totalClearedLines += clearedLineCount;
        int newLevel = 1 + totalClearedLines / 5;

        if (newLevel != level)
        {
            level = newLevel;
            fallInterval = Mathf.Max(0.05f, 1.0f / (1f + (level - 1) * 0.5f));
        }
    }

    private void RefreshRender()
    {
        Tetromino.TetrominoType? type = board.currentTetromino != null
            ? board.currentTetromino.type
            : (Tetromino.TetrominoType?)null;

        Vector2Int[] positions = board.currentTetromino != null
            ? board.currentTetromino.positions
            : null;

        Vector2Int[] ghostPositions = board.currentTetromino != null
            ? board.GetGhostPositions()
            : null;

        boardRenderer.Refresh(board.GetGrid(), positions, type, ghostPositions);
    }

    private void SpawnNext()
    {
        boardRenderer.PickNewSet();
        bool spawnSuccess = board.SpawnBlock();
        if (!spawnSuccess)
            GameOver();

        uiManager.UpdateNext(board.nextType);
        uiManager.UpdateHold(board.holdType);
    }

    private int CalculateScore(int lines)
    {
        return lines switch
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        };
    }

    public void Pause()
    {
        state = GameState.Paused;
        Time.timeScale = 0f;
        uiManager.ShowPause(true);
    }

    public void Resume()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
        uiManager.ShowPause(false);
    }

    public void GameOver()
    {
        state = GameState.GameOver;

        if (backgroundController != null)
            backgroundController.Reset();

        if (liquidGauge != null)
            liquidGauge.Reset();

        if (audioService != null)
            audioService.PlaySFX("GameOver");

        musicManager.FadeOutBGM(1.5f);

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return StartCoroutine(boardRenderer.PlayGameOverAnimation());

        musicManager.PlayGameOverBGM(gameOverBGM);

        if (uiManager != null)
            uiManager.ShowGameOver(score);
    }
}
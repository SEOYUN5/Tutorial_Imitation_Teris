using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private Sprite[] tetrominoSprites;
    [SerializeField] private int pieceCount = 12;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip lobbyBGM;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private RectTransform canvasRect; // Canvas의 Rect Transform
    [SerializeField] private Transform backgroundObject; // LobbyBackground (이 뒤에 블록 배치)

    private void Start()
    {
        SpawnFloatingPieces();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(savedVolume); //일관성

        if (bgmSource != null && lobbyBGM != null)
        {
            bgmSource.clip = lobbyBGM;
            bgmSource.loop = true;
            bgmSource.volume = savedVolume;
            bgmSource.Play();
        }
    }

    private void SpawnFloatingPieces()
    {
        if (canvasRect == null) return;

        float halfW = canvasRect.rect.width / 2f;
        float halfH = canvasRect.rect.height / 2f;

        // 타이틀이 있는 중앙 영역 (피해야 할 구역)
        float excludeHalfW = 550f;
        float excludeMinY = 50f;
        float excludeMaxY = 350f;

        int spawned = 0;
        int attempts = 0;

        while (spawned < pieceCount && attempts < pieceCount * 20)
        {
            attempts++;

            Vector2 pos = new Vector2(
                Random.Range(-halfW, halfW),
                Random.Range(-halfH, halfH)
            );

            bool insideExcludeZone =
                Mathf.Abs(pos.x) < excludeHalfW &&
                pos.y > excludeMinY && pos.y < excludeMaxY;

            if (insideExcludeZone) continue;

            GameObject piece = new GameObject("FloatingPiece" + spawned, typeof(RectTransform));
            piece.transform.SetParent(canvasRect, false);

            RectTransform rt = piece.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80) * Random.Range(0.8f, 1.5f);
            rt.anchoredPosition = pos;

            Image img = piece.AddComponent<Image>();
            img.sprite = tetrominoSprites[Random.Range(0, tetrominoSprites.Length)];
            Color c = Color.white;
            c.a = 0.5f;
            img.color = c;
            img.raycastTarget = false;

            FloatingPiece fp = piece.AddComponent<FloatingPiece>();
            fp.Init(canvasRect);

            if (backgroundObject != null)
            {
                int bgIndex = backgroundObject.GetSiblingIndex();
                piece.transform.SetSiblingIndex(bgIndex + 1);
            }

            spawned++;
        }
    }

    public void OnVolumeChanged(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;

        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    public void OnPlayButton()
    {
        SceneLoader.LoadGame();
    }

    public void OnSettingsButton()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnQuitButton()
    {
        SceneLoader.QuitGame();
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private LiquidGaugeController gaugeController;

    private void OnEnable()
    {
        if (volumeSlider == null) return;

        float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        volumeSlider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        if (musicManager != null)
            musicManager.SetBGMVolume(value);

        if (gaugeController != null)
            gaugeController.SetVolume(value);
    }

    public void OnRestartButton()
    {
        SceneLoader.LoadGame();
    }
}
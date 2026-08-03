using UnityEngine;

public class MusicManager : MonoBehaviour, IAudioService
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip rotateClip;
    [SerializeField] private AudioClip lineClearClip;
    [SerializeField] private AudioClip tetrisClearClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip holdClip;
    [SerializeField] private AudioClip noHoldClip;

    [SerializeField] private AudioClip gameOverBGMClip;


    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        bgmSource.volume = startVolume; 
    }

    public void PlayGameOverBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = false;  
        bgmSource.Play();
    }


    public void SetBGMVolume(float value)
    {
        bgmSource.volume = value;
    }
    public void PlaySFX(string name)
    {
        AudioClip clip = name switch
        {
            "Move" => moveClip,
            "Rotate" => rotateClip,
            "LineClear" => lineClearClip,
            "TetrisClear" => tetrisClearClip,
            "GameOver" => gameOverClip,
            "Drop" => dropClip,
            "Hold" => holdClip,
            "NoHold" => noHoldClip,
            _ => null
        };

        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
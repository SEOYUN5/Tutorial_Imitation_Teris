using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LiquidGaugeController : MonoBehaviour
{
    [SerializeField] private Image leftGauge;
    [SerializeField] private Image rightGauge;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip[] stageBGMs = new AudioClip[7];
    [SerializeField] private float checkInterval = 12f;
    [SerializeField] private int requiredLines = 3;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("최고 단계 연출")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private ParticleSystem leftMaxParticle;
    [SerializeField] private ParticleSystem rightMaxParticle;
    [SerializeField] private Color pulseColor = new Color(1f, 0.9f, 0f);

    [Header("레인보우 모드")]
    [SerializeField] private float rainbowActivateTime = 20f;
    [SerializeField] private float rainbowCycleTime = 4f;

    private int currentStep = -1;
    private float targetFill = 0f;
    private float currentFill = 0f;
    private float timer = 0f;
    private int linesInWindow = 0;
    private Coroutine fadeCoroutine;
    private Coroutine pulseCoroutine;

    private float maxStageTimer = 0f;
    private bool rainbowMode = false;
    private int rainbowIndex = 0;
    private float rainbowTimer = 0f;
    private float savedVolume = 1f;

    private Camera mainCamera;

    private Color[] rainbowColors = new Color[]
    {
        new Color(0.55f, 0.15f, 0.15f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.5f, 0.5f, 0.15f),
        new Color(0.15f, 0.4f, 0.15f),
        new Color(0.15f, 0.3f, 0.5f),
        new Color(0.15f, 0.15f, 0.4f),
        new Color(0.3f, 0.15f, 0.45f),
    };

    private void Awake()
    {
        if (leftGauge != null) leftGauge.fillAmount = 0f;
        if (rightGauge != null) rightGauge.fillAmount = 0f;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        if (bgmSource != null)
            bgmSource.volume = savedVolume;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            StepDown();
            ResetWindow();
        }

        float drainTarget = targetFill;
        if (currentStep >= 0 && linesInWindow < requiredLines)
        {
            float progress = timer / checkInterval;
            float nextStepFill = currentStep > 0 ? (float)currentStep / 7f : 0f;
            drainTarget = Mathf.Lerp(targetFill, nextStepFill, progress);
        }

        currentFill = Mathf.Lerp(currentFill, drainTarget, Time.deltaTime * 3f);
        if (leftGauge != null) leftGauge.fillAmount = currentFill;
        if (rightGauge != null) rightGauge.fillAmount = currentFill;

        if (currentStep == 6)
        {
            maxStageTimer += Time.deltaTime;

            if (!rainbowMode && maxStageTimer >= rainbowActivateTime)
            {
                rainbowMode = true;
                rainbowIndex = 0;
                rainbowTimer = 0f;
                Debug.Log("레인보우 모드 활성화!");
            }

            if (rainbowMode)
            {
                rainbowTimer += Time.deltaTime;
                if (rainbowTimer >= rainbowCycleTime)
                {
                    rainbowTimer = 0f;
                    rainbowIndex = (rainbowIndex + 1) % rainbowColors.Length;
                }

                if (mainCamera != null)
                {
                    Color targetColor = rainbowColors[rainbowIndex];
                    mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetColor, Time.deltaTime * 3f);
                }

                if (leftMaxParticle != null && !leftMaxParticle.isPlaying)
                    leftMaxParticle.Play();
                if (rightMaxParticle != null && !rightMaxParticle.isPlaying)
                    rightMaxParticle.Play();
            }
        }
        else
        {
            if (rainbowMode)
            {
                rainbowMode = false;
                maxStageTimer = 0f;
            }
        }
    }

    public void OnCombo(int lineCount = 1)
    {
        linesInWindow += lineCount;
        Debug.Log("줄 클리어 이번 구간 카운트: " + linesInWindow + "/" + requiredLines);

        if (currentStep == 6 && cameraShake != null)
            cameraShake.Shake(0.1f, 0.15f);

        if (linesInWindow >= requiredLines)
        {
            StepUp();
            ResetWindow();
        }
    }

    private void StepUp()
    {
        currentStep = Mathf.Min(currentStep + 1, 6);
        targetFill = (float)(currentStep + 1) / 7f;
        ChangeBGMWithFade(currentStep);
        Debug.Log("단계 상승 Step: " + currentStep);

        if (currentStep == 6)
        {
            maxStageTimer = 0f;
            TriggerMaxStageEffects();
        }
    }

    private void StepDown()
    {
        if (currentStep < 0) return;

        if (currentStep == 6)
        {
            currentStep = -1;
            rainbowMode = false;
            maxStageTimer = 0f;
        }
        else
        {
            currentStep--;
        }

        targetFill = currentStep >= 0 ? (float)(currentStep + 1) / 7f : 0f;

        int bgmIndex = Mathf.Max(0, currentStep);
        ChangeBGMWithFade(bgmIndex);

        StopMaxStageEffects();

        StartCoroutine(FlashRed());

        Debug.Log("단계 하락 Step: " + currentStep);
    }

    private IEnumerator FlashRed()
    {
        Color flashColor = new Color(1f, 0.2f, 0.2f);

        for (int i = 0; i < 4; i++)
        {
            if (leftGauge != null) leftGauge.color = flashColor;
            if (rightGauge != null) rightGauge.color = flashColor;
            yield return new WaitForSeconds(0.1f);

            if (leftGauge != null) leftGauge.color = Color.white;
            if (rightGauge != null) rightGauge.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void TriggerMaxStageEffects()
    {
        if (cameraShake != null)
            cameraShake.Shake(0.3f, 0.3f);

        if (leftMaxParticle != null)
            leftMaxParticle.Play();
        if (rightMaxParticle != null)
            rightMaxParticle.Play();

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseGauge());
    }

    private void StopMaxStageEffects()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
            ResetGaugeColor();
        }

        if (leftMaxParticle != null && leftMaxParticle.isPlaying)
            leftMaxParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (rightMaxParticle != null && rightMaxParticle.isPlaying)
            rightMaxParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (mainCamera != null)
            mainCamera.backgroundColor = Color.black;
    }

    private IEnumerator PulseGauge()
    {
        while (currentStep == 6)
        {
            float t = (Mathf.Sin(Time.time * 6f) + 1f) / 2f;
            Color c = Color.Lerp(Color.white, pulseColor, t);
            if (leftGauge != null) leftGauge.color = c;
            if (rightGauge != null) rightGauge.color = c;
            yield return null;
        }
        ResetGaugeColor();
    }

    private void ResetGaugeColor()
    {
        if (leftGauge != null) leftGauge.color = Color.white;
        if (rightGauge != null) rightGauge.color = Color.white;
    }

    private void ResetWindow()
    {
        timer = 0f;
        linesInWindow = 0;
    }

    private void ChangeBGMWithFade(int stepIndex)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAndPlayBGM(stepIndex));
    }

    private IEnumerator FadeAndPlayBGM(int stepIndex)
    {
        float currentTime = bgmSource.time;

        float originalVolume = savedVolume;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();

        if (stageBGMs[stepIndex] != null)
        {
            bgmSource.clip = stageBGMs[stepIndex];

            float safeTime = Mathf.Clamp(currentTime, 0f, Mathf.Max(0f, stageBGMs[stepIndex].length - 0.05f));
            bgmSource.time = safeTime;

            bgmSource.loop = true;
            bgmSource.Play();
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, originalVolume, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = originalVolume;
    }

    public void PlayInitialBGM()
    {
        if (bgmSource != null && stageBGMs[0] != null)
        {
            savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            bgmSource.Stop();
            bgmSource.clip = stageBGMs[0];
            bgmSource.time = 0f;
            bgmSource.loop = true;
            bgmSource.volume = savedVolume;
            bgmSource.Play();
            Debug.Log($"초기 BGM 재생, 볼륨={savedVolume}");
        }
    }

    public int GetCurrentStep()
    {
        return currentStep;
    }

    public void SetVolume(float value)
    {
        savedVolume = value;
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void Reset()
    {
        currentStep = -1;
        targetFill = 0f;
        currentFill = 0f;
        timer = 0f;
        linesInWindow = 0;
        maxStageTimer = 0f;
        rainbowMode = false;

        StopMaxStageEffects();

        if (leftGauge != null) leftGauge.fillAmount = 0f;
        if (rightGauge != null) rightGauge.fillAmount = 0f;
    }
}
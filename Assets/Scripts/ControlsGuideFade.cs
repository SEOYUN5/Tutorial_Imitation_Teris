using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlsGuideFade : MonoBehaviour
{
    [SerializeField] private float visibleDuration = 30f;
    [SerializeField] private float fadeDuration = 1.5f;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;
        Color startColor = image.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
            image.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}

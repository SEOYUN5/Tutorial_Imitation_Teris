using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private LiquidGaugeController liquidGauge;

    private Camera mainCamera;

    private Color[] rainbowColors = new Color[]
    {
    Color.black,                          
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
        mainCamera = Camera.main;
        mainCamera.backgroundColor = Color.black;
    }

    private void Update()
    {
        if (liquidGauge == null) return;

        int step = liquidGauge.GetCurrentStep();
        int colorIndex = Mathf.Clamp(step + 1, 0, rainbowColors.Length - 1);
        Color targetColor = rainbowColors[colorIndex];

        mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetColor, Time.deltaTime * 2f);
    }

    public void Reset()
    {
        mainCamera.backgroundColor = Color.black;
    }
}
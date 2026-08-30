using UnityEngine;

public class FloatingPiece : MonoBehaviour
{
    private RectTransform rt;
    private RectTransform canvasRect;

    private float speedX;
    private float speedY;
    private float rotationSpeed;
    private float bobAmount;
    private float bobSpeed;
    private Vector2 basePos;
    private float bobOffset;

    public void Init(RectTransform canvasRectTransform)
    {
        rt = GetComponent<RectTransform>();
        canvasRect = canvasRectTransform;

        basePos = rt.anchoredPosition;
        speedX = Random.Range(-15f, 15f);
        speedY = Random.Range(-8f, 8f);
        rotationSpeed = Random.Range(-20f, 20f);
        bobAmount = Random.Range(10f, 25f);
        bobSpeed = Random.Range(0.5f, 1.2f);
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        basePos += new Vector2(speedX, speedY) * Time.deltaTime;

        float bob = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
        rt.anchoredPosition = basePos + new Vector2(0, bob);

        rt.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        float halfW = canvasRect.rect.width / 2f;
        float halfH = canvasRect.rect.height / 2f;
        float margin = 100f;

        if (basePos.x < -halfW - margin) basePos.x = halfW + margin;
        if (basePos.x > halfW + margin) basePos.x = -halfW - margin;
        if (basePos.y < -halfH - margin) basePos.y = halfH + margin;
        if (basePos.y > halfH + margin) basePos.y = -halfH - margin;
    }
}
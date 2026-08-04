using UnityEngine;

public class BlockPreviewRenderer : MonoBehaviour
{
    [SerializeField] private Sprite[] blockSprites; // 7개
    [SerializeField] private float cellSpacing = 1f;

    private SpriteRenderer[] cells;

    private void Awake()
    {
        cells = new SpriteRenderer[16];
        for (int i = 0; i < 16; i++)
        {
            GameObject cellObj = new GameObject($"Cell{i}");
            cellObj.transform.SetParent(transform, false);

            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            cells[i] = sr;
        }
    }

    public void ShowBlock(Tetromino.TetrominoType? type)
    {
        foreach (var cell in cells)
            cell.enabled = false;

        if (type == null) return;

        Vector2Int[] shape = Tetromino.GetShapeData(type.Value)[0];
        Sprite sprite = blockSprites[(int)type.Value % blockSprites.Length];

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var pos in shape)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        float shapeWidth = (maxX - minX + 1) * cellSpacing;
        float shapeHeight = (maxY - minY + 1) * cellSpacing;

        int index = 0;
        foreach (var pos in shape)
        {
            float x = (pos.x - minX) * cellSpacing - shapeWidth / 2f + cellSpacing / 2f;
            float y = (pos.y - minY) * cellSpacing - shapeHeight / 2f + cellSpacing / 2f;

            cells[index].transform.localPosition = new Vector3(x, y, 0);
            cells[index].sprite = sprite;
            cells[index].enabled = true;
            index++;
        }
    }
}
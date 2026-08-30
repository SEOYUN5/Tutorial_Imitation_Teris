using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private Sprite[] blockSprites;

    private GameObject[,] cells;
    private Sprite[,] lockedSprites;
    private Sprite[] currentSet;

    private GameObject[,] ghostCells;
    private Vector2Int[] lastGhostPositions;

    private void Start()
    {
        cells = new GameObject[10, 20];
        lockedSprites = new Sprite[10, 20];
        ghostCells = new GameObject[10, 20];

        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.SetParent(transform);
                cell.transform.localPosition = new Vector3(x, y, 0);

                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;
                cells[x, y] = cell;

                GameObject ghost = new GameObject($"Ghost_{x}_{y}");
                ghost.transform.SetParent(transform);
                ghost.transform.localPosition = new Vector3(x, y, 0);

                SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
                ghostSr.sortingOrder = 0;
                ghostSr.enabled = false;

                Color c = Color.white;
                c.a = 0.15f;
                ghostSr.color = c;
                ghostCells[x, y] = ghost;
            }
        }

        PickNewSet();
    }

    public void PickNewSet()
    {
        currentSet = blockSprites;
    }

    public void LockSprites(Vector2Int[] positions, int typeIndex)
    {
        foreach (Vector2Int pos in positions)
        {
            if (pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 20)
            {
                lockedSprites[pos.x, pos.y] = currentSet[typeIndex];
                cells[pos.x, pos.y].GetComponent<SpriteRenderer>().sprite = lockedSprites[pos.x, pos.y];
            }
        }
    }

    public void DropLockedSprite(int clearedY)
    {
        for (int y = clearedY; y < 19; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                lockedSprites[x, y] = lockedSprites[x, y + 1];
                cells[x, y].GetComponent<SpriteRenderer>().sprite = lockedSprites[x, y];
            }
        }

        for (int x = 0; x < 10; x++)
        {
            lockedSprites[x, 19] = null;
            cells[x, 19].GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public void Refresh(bool[,] grid, Vector2Int[] tetrominoPositions, Tetromino.TetrominoType? tetrominoType, Vector2Int[] ghostPositions = null)
    {
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                sr.sprite = grid[x, y] ? lockedSprites[x, y] : null;
            }
        }

        if (lastGhostPositions != null)
        {
            foreach (var pos in lastGhostPositions)
                if (pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 20)
                    ghostCells[pos.x, pos.y].GetComponent<SpriteRenderer>().enabled = false;
        }

        if (ghostPositions != null && tetrominoType.HasValue)
        {
            int typeIndex = (int)tetrominoType.Value;
            foreach (var pos in ghostPositions)
            {
                if (pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 20)
                {
                    SpriteRenderer gsr = ghostCells[pos.x, pos.y].GetComponent<SpriteRenderer>();
                    gsr.sprite = currentSet[typeIndex];
                    gsr.enabled = true;
                }
            }
        }
        lastGhostPositions = ghostPositions;

        if (tetrominoPositions != null && tetrominoType.HasValue)
        {
            int typeIndex = (int)tetrominoType.Value;
            foreach (Vector2Int pos in tetrominoPositions)
            {
                if (pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 20)
                    cells[pos.x, pos.y].GetComponent<SpriteRenderer>().sprite = currentSet[typeIndex];
            }
        }
    }

    public IEnumerator PlayGameOverAnimation()
    {
        List<GameObject> particles = new List<GameObject>();

        // 맨 위 줄부터 아래로 순서대로 부수기
        for (int y = 19; y >= 0; y--)
        {
            bool hasBlock = false;
            for (int x = 0; x < 10; x++)
            {
                if (lockedSprites[x, y] != null)
                    hasBlock = true;
            }

            if (!hasBlock) continue;

            // 이 줄의 블록들을 잘게 쪼개서 파티클로 변환
            for (int x = 0; x < 10; x++)
            {
                if (lockedSprites[x, y] == null) continue;

                // 원본 셀 숨기기
                cells[x, y].GetComponent<SpriteRenderer>().sprite = null;

                // 파티클 5개 생성 (블록 한 개당)
                for (int p = 0; p < 5; p++)
                {
                    GameObject particle = new GameObject($"Particle_{x}_{y}_{p}");
                    particle.transform.SetParent(transform);
                    particle.transform.localPosition = new Vector3(x, y, 0);
                    particle.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

                    SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
                    sr.sprite = lockedSprites[x, y];
                    sr.sortingOrder = 2;

                    // 랜덤 속도 (아래 + 약간 옆으로)
                    Vector2 velocity = new Vector2(
                        Random.Range(-2f, 2f),
                        Random.Range(-3f, -1f)
                    );

                    StartCoroutine(AnimateParticle(particle, sr, velocity));
                    particles.Add(particle);
                }
            }

            // 줄마다 약간의 딜레이 (위에서부터 순서대로 부서지는 느낌)
            yield return new WaitForSeconds(0.05f);
        }

        // 파티클 다 사라질 때까지 대기
        yield return new WaitForSeconds(1.0f);

        // 파티클 정리
        foreach (var p in particles)
            if (p != null) Destroy(p);
    }

    private IEnumerator AnimateParticle(GameObject particle, SpriteRenderer sr, Vector2 velocity)
    {
        float elapsed = 0f;
        float duration = 1.0f;
        Vector3 startPos = particle.transform.localPosition;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 중력 적용
            velocity.y -= 9.8f * Time.deltaTime;
            particle.transform.localPosition += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;

            // 서서히 투명해지기
            Color c = startColor;
            c.a = 1f - t;
            sr.color = c;

            // 회전
            particle.transform.Rotate(0, 0, Random.Range(-360f, 360f) * Time.deltaTime);

            yield return null;
        }

        Destroy(particle);
    }
}
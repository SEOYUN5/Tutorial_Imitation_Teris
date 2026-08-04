using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;

    private bool[,] grid = new bool[Width, Height];

    public Tetromino currentTetromino { get; private set; }
    public Tetromino.TetrominoType nextType { get; private set; }
    public Tetromino.TetrominoType? holdType { get; private set; }
    private bool holdUsed = false;

    [SerializeField] private Tetromino tetrominoPrefab;

    private void Awake()
    {
        nextType = GetRandomType();
    }

    private Tetromino.TetrominoType GetRandomType()
    {
        return (Tetromino.TetrominoType)Random.Range(
            0, System.Enum.GetValues(typeof(Tetromino.TetrominoType)).Length);
    }

    public bool SpawnBlock()
    {
        if (currentTetromino != null)
            Destroy(currentTetromino.gameObject);

        Tetromino.TetrominoType type = nextType;
        nextType = GetRandomType();
        holdUsed = false;

        return SpawnBlockFromType(type);
    }

    private bool SpawnBlockFromType(Tetromino.TetrominoType type)
    {
        currentTetromino = Instantiate(tetrominoPrefab);
        currentTetromino.Init(Tetromino.GetShapeData(type), type);
        Vector2Int spawnOffset = new Vector2Int(Width / 2 - 1, Height - 2);
        currentTetromino.UpdatePosition(spawnOffset);

        if (!IsValidPosition(currentTetromino.positions))
        {
            Destroy(currentTetromino.gameObject);
            return false;
        }
        return true;
    }

    public bool HoldBlock()
    {
        if (holdUsed) return false;
        holdUsed = true;

        Tetromino.TetrominoType currentType = currentTetromino.type;
        Destroy(currentTetromino.gameObject);
        currentTetromino = null;

        if (holdType == null)
        {
            holdType = currentType;
            SpawnBlockFromType(nextType);
            nextType = GetRandomType();
        }
        else
        {
            Tetromino.TetrominoType swapType = holdType.Value;
            holdType = currentType;
            SpawnBlockFromType(swapType);
        }

        return true;
    }

    public bool MoveBlock(InputManager.Direction direction)
    {
        Vector2Int delta = direction switch
        {
            InputManager.Direction.Left => new Vector2Int(-1, 0),
            InputManager.Direction.Right => new Vector2Int(1, 0),
            InputManager.Direction.Down => new Vector2Int(0, -1),
            _ => Vector2Int.zero
        };

        if (!CheckCollision(delta)) return false;
        currentTetromino.UpdatePosition(delta);
        return true;
    }

    public bool RotateBlock(bool clockwise)
    {
        if (clockwise)
            currentTetromino.Rotate();
        else
            currentTetromino.RotateBack();

        Vector2Int[] kicks = {
            Vector2Int.zero,
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1),
            new Vector2Int(-2, 0),
            new Vector2Int(2, 0),
            new Vector2Int(0, -1)
        };

        foreach (var kick in kicks)
        {
            if (CheckCollision(kick))
            {
                if (kick != Vector2Int.zero)
                    currentTetromino.UpdatePosition(kick);
                return true;
            }
        }

        // 실패 시 반대 방향으로 되돌리기
        if (clockwise)
            currentTetromino.RotateBack();
        else
            currentTetromino.Rotate();

        return false;
    }

    public bool CheckCollision(Vector2Int delta)
    {
        foreach (var pos in currentTetromino.positions)
        {
            Vector2Int next = pos + delta;
            if (next.x < 0 || next.x >= Width || next.y < 0) return false;
            if (next.y < Height && grid[next.x, next.y]) return false;
        }
        return true;
    }

    private bool IsValidPosition(Vector2Int[] positions)
    {
        foreach (var pos in positions)
        {
            if (pos.x < 0 || pos.x >= Width || pos.y < 0 || pos.y >= Height) return false;
            if (grid[pos.x, pos.y]) return false;
        }
        return true;
    }

    public void LockBlock()
    {
        foreach (var pos in currentTetromino.positions)
            if (pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height)
                grid[pos.x, pos.y] = true;
    }

    public System.Collections.Generic.List<int> CheckLines()
    {
        var clearedRows = new System.Collections.Generic.List<int>();
        for (int y = Height - 1; y >= 0; y--)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                DropLines(y);
                clearedRows.Add(y);
                y++; //같은줄 다시 검사
            }
        }
        return clearedRows;
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < Width; x++)
            if (!grid[x, y]) return false;
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < Width; x++)
            grid[x, y] = false;
    }

    private void DropLines(int clearedY)
    {
        for (int y = clearedY; y < Height - 1; y++)
            for (int x = 0; x < Width; x++)
                grid[x, y] = grid[x, y + 1];
    }

    public bool StepDown()
    {
        if (!MoveBlock(InputManager.Direction.Down))
        {
            LockBlock();
            return true;
        }
        return false;
    }

    public bool[,] GetGrid() => grid;

    public Vector2Int[] HardDropAndGetPositions()
    {
        while (MoveBlock(InputManager.Direction.Down)) { }
        Vector2Int[] finalPositions = (Vector2Int[])currentTetromino.positions.Clone();
        LockBlock();
        return finalPositions;
    }

    public Vector2Int[] GetGhostPositions()
    {
        Vector2Int[] positions = (Vector2Int[])currentTetromino.positions.Clone();
        Vector2Int delta = Vector2Int.zero;

        while (true)
        {
            Vector2Int nextDelta = delta + new Vector2Int(0, -1);
            bool valid = true;

            foreach (var pos in currentTetromino.positions)
            {
                Vector2Int next = pos + nextDelta;
                if (next.x < 0 || next.x >= Width || next.y < 0) { valid = false; break; }
                if (next.y < Height && grid[next.x, next.y]) { valid = false; break; }
            }

            if (!valid) break;
            delta = nextDelta;
        }

        for (int i = 0; i < positions.Length; i++)
            positions[i] = currentTetromino.positions[i] + delta;

        return positions;
    }

}
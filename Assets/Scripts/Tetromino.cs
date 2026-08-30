using UnityEngine;

public class Tetromino : MonoBehaviour
{
    public Vector2Int[] positions { get; private set; }
    public int rotationState { get; private set; }
    public TetrominoType type { get; private set; }

    private Vector2Int pivot;
    private Vector2Int[][] shapeData;

    private static readonly Vector2Int[][] IShape =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2) },
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2) },
    };

    private static readonly Vector2Int[][] OShape =
    {
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
    };

    private static readonly Vector2Int[][] TShape =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,0) },
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(-1,0) },
    };

    private static readonly Vector2Int[][] JShape =
    {
        new[] { new Vector2Int(-1,1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0) },
        new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(0,-1) },
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(-1,-1), new Vector2Int(0,-1) },
    };

    private static readonly Vector2Int[][] LShape =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
        new[] { new Vector2Int(-1,-1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0) },
        new[] { new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1) },
    };

    private static readonly Vector2Int[][] SShape =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new[] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
    };

    private static readonly Vector2Int[][] ZShape =
    {
        new[] { new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0) },
        new[] { new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1) },
        new[] { new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0) },
        new[] { new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1) },
    };

    public void Init(Vector2Int[][] shape, TetrominoType t)
    {
        shapeData = shape;
        type = t;
        rotationState = 0;
        pivot = Vector2Int.zero;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        var offsets = shapeData[rotationState];
        positions = new Vector2Int[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
            positions[i] = pivot + offsets[i];
    }

    public void Rotate()
    {
        rotationState = (rotationState + 1) % 4;
        ApplyRotation();
    }

    public void RotateBack()
    {
        rotationState = (rotationState + 3) % 4;
        ApplyRotation();
    }

    public void UpdatePosition(Vector2Int delta)
    {
        pivot += delta;
        ApplyRotation();
    }

    public enum TetrominoType { I, O, T, J, L, S, Z }

    public static Vector2Int[][] GetShapeData(TetrominoType type)
    {
        return type switch
        {
            TetrominoType.I => IShape,
            TetrominoType.O => OShape,
            TetrominoType.T => TShape,
            TetrominoType.J => JShape,
            TetrominoType.L => LShape,
            TetrominoType.S => SShape,
            TetrominoType.Z => ZShape,
            _ => TShape
        };
    }
}
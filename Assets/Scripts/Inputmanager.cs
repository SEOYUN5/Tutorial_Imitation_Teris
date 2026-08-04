using UnityEngine;
using UnityEngine.InputSystem;

// 키보드 입력만 감지하고 다른 클래스는 직접 조작하지 않음
public class InputManager : MonoBehaviour
{
    public enum Direction { None, Left, Right, Down }

    [SerializeField] private float initialDelay = 0.15f; 
    [SerializeField] private float repeatRate = 0.05f;    

    private Direction heldDirection = Direction.None;
    private float holdTimer = 0f;
    private bool hasRepeatedOnce = false;

    public Direction GetMoveInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Direction.None;

        Direction currentDirection = Direction.None;

        if (keyboard.leftArrowKey.isPressed) currentDirection = Direction.Left;
        else if (keyboard.rightArrowKey.isPressed) currentDirection = Direction.Right;
        else if (keyboard.downArrowKey.isPressed) currentDirection = Direction.Down;

        if (keyboard.leftArrowKey.wasPressedThisFrame) currentDirection = Direction.Left;
        if (keyboard.rightArrowKey.wasPressedThisFrame) currentDirection = Direction.Right;
        if (keyboard.downArrowKey.wasPressedThisFrame) currentDirection = Direction.Down;

        if (currentDirection == Direction.None)
        {
            heldDirection = Direction.None;
            holdTimer = 0f;
            hasRepeatedOnce = false;
            return Direction.None;
        }

        if (currentDirection != heldDirection)
        {
            heldDirection = currentDirection;
            holdTimer = 0f;
            hasRepeatedOnce = false;
            return currentDirection;
        }

        holdTimer += Time.deltaTime;

        float threshold = hasRepeatedOnce ? repeatRate : initialDelay;

        if (holdTimer >= threshold)
        {
            holdTimer = 0f;
            hasRepeatedOnce = true;
            return heldDirection;
        }

        return Direction.None;
    }

    public bool GetRotateCW()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;
        return keyboard.eKey.wasPressedThisFrame;
    }

    public bool GetRotateCCW()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;
        return keyboard.qKey.wasPressedThisFrame;
    }

    public bool GetHardDropInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;
        return keyboard.spaceKey.wasPressedThisFrame;
    }
}
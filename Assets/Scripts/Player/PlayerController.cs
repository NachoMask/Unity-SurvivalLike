using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerCharacter))]
public class PlayerController : MonoBehaviour
{
    private PlayerCharacter playerCharacter;

    [Header("Movement Actions")]
    [SerializeField] private InputActionReference moveUpRef;
    [SerializeField] private InputActionReference moveDownRef;
    [SerializeField] private InputActionReference moveLeftRef;
    [SerializeField] private InputActionReference moveRightRef;

    private readonly List<Vector2> pressedDirections = new();

    private InputAction moveUpAction;
    private InputAction moveDownAction;
    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputActionMap playerActionMap;

    private void Awake()
    {
        playerCharacter = GetComponent<PlayerCharacter>();

        moveUpAction = moveUpRef?.action;
        moveDownAction = moveDownRef?.action;
        moveLeftAction = moveLeftRef?.action;
        moveRightAction = moveRightRef?.action;

        if (moveUpAction == null ||
            moveDownAction == null ||
            moveLeftAction == null ||
            moveRightAction == null)
        {
            Debug.LogError("Movement actions are not assigned", this);
            enabled = false;
            return;
        }

        playerActionMap = moveUpAction.actionMap;

        if (moveDownAction.actionMap != playerActionMap ||
            moveLeftAction.actionMap != playerActionMap ||
            moveRightAction.actionMap != playerActionMap)
        {
            Debug.LogError("Movement actions must belong to same action map", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerActionMap == null) return;

        Subscribe(moveUpAction);
        Subscribe(moveDownAction);
        Subscribe(moveLeftAction);
        Subscribe(moveRightAction);
        playerActionMap.Enable();
    }

    private void OnDisable()
    {
        if (playerActionMap == null) return;

        Unsubscribe(moveUpAction);
        Unsubscribe(moveDownAction);
        Unsubscribe(moveLeftAction);
        Unsubscribe(moveRightAction);
        playerActionMap.Disable();

        pressedDirections.Clear();
        playerCharacter.SetMoveDirection(Vector2.zero);
    }

    private void Subscribe(InputAction action)
    {
        action.performed += OnDirectionPressed;
        action.canceled += OnDirectionReleased;
    }

    private void Unsubscribe(InputAction action)
    {
        action.performed -= OnDirectionPressed;
        action.canceled -= OnDirectionReleased;
    }

    private void OnDirectionPressed(InputAction.CallbackContext context)
    {
        Vector2 dir = GetDirection(context.action);

        pressedDirections.Remove(dir);
        pressedDirections.Add(dir);
        UpdateMoveDirection();
    }

    private void OnDirectionReleased(InputAction.CallbackContext context)
    {
        Vector2 dir = GetDirection(context.action);

        pressedDirections.Remove(dir);
        UpdateMoveDirection();
    }

    private Vector2 GetDirection(InputAction action)
    {
        if (action == moveUpAction)
        {
            return Vector2.up;
        }
        if (action == moveDownAction)
        {
            return Vector2.down;
        }
        if (action == moveLeftAction)
        {
            return Vector2.left;
        }
        if (action == moveRightAction)
        {
            return Vector2.right;
        }

        return Vector2.zero;
    }

    private void UpdateMoveDirection()
    {
        Vector2 dir = Vector2.zero;
        bool foundHorizontal = false;
        bool foundVertical = false;

        for (int i = pressedDirections.Count - 1; i >= 0; --i)
        {
            Vector2 pressedDir = pressedDirections[i];

            if (!foundHorizontal && pressedDir.x != 0)
            {
                dir.x = pressedDir.x;
                foundHorizontal = true;
            }

            if (!foundVertical && pressedDir.y != 0)
            {
                dir.y = pressedDir.y;
                foundVertical = true;
            }

            if (foundHorizontal && foundVertical)
            {
                break;
            }
        }

        playerCharacter.SetMoveDirection(dir.normalized);
    }
}

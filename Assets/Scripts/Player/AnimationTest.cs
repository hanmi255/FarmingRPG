using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Events;
using Assets.Scripts.Enums;

public class PlayerAnimationTest : MonoBehaviour
{
    public float inputX;
    public float inputY;
    public bool isWalking;
    public bool isRunning;
    public bool isIdle;
    public bool isCarrying;
    public ToolEffect toolEffect;
    public bool isUsingToolRight;
    public bool isUsingToolLeft;
    public bool isUsingToolUp;
    public bool isUsingToolDown;
    public bool isLiftingToolRight;
    public bool isLiftingToolLeft;
    public bool isLiftingToolUp;
    public bool isLiftingToolDown;
    public bool isPickingRight;
    public bool isPickingLeft;
    public bool isPickingUp;
    public bool isPickingDown;
    public bool isSwingingToolRight;
    public bool isSwingingToolLeft;
    public bool isSwingingToolUp;
    public bool isSwingingToolDown;
    public bool idleUp;
    public bool idleDown;
    public bool idleLeft;
    public bool idleRight;

    private void Update()
    {
        MovementParameters movementParams = new()
        {
            inputX = inputX,
            inputY = inputY,
            isWalking = isWalking,
            isRunning = isRunning,
            isIdle = isIdle,
            isCarrying = isCarrying,
            toolEffect = toolEffect,
            isUsingToolRight = isUsingToolRight,
            isUsingToolLeft = isUsingToolLeft,
            isUsingToolUp = isUsingToolUp,
            isUsingToolDown = isUsingToolDown,
            isLiftingToolRight = isLiftingToolRight,
            isLiftingToolLeft = isLiftingToolLeft,
            isLiftingToolUp = isLiftingToolUp,
            isLiftingToolDown = isLiftingToolDown,
            isPickingRight = isPickingRight,
            isPickingLeft = isPickingLeft,
            isPickingUp = isPickingUp,
            isPickingDown = isPickingDown,
            isSwingingToolRight = isSwingingToolRight,
            isSwingingToolLeft = isSwingingToolLeft,
            isSwingingToolUp = isSwingingToolUp,
            isSwingingToolDown = isSwingingToolDown,
            isIdleUp = idleUp,
            isIdleDown = idleDown,
            isIdleLeft = idleLeft,
            isIdleRight = idleRight
        };

        EventHandler.CallMovementEvent(movementParams);
    }
}
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    [RequireComponent(typeof(Animator))]
    public class MovementAnimationParameterControl : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            EventHandler.MovementEvent += SetAnimationParameters;
        }

        private void OnDisable()
        {
            EventHandler.MovementEvent -= SetAnimationParameters;
        }

        private void SetAnimationParameters(MovementParameters movementParams)
        {
            // 基本移动输入
            animator.SetFloat(Settings.inputX, movementParams.inputX);
            animator.SetFloat(Settings.inputY, movementParams.inputY);
            animator.SetBool(Settings.isWalking, movementParams.isWalking);
            animator.SetBool(Settings.isRunning, movementParams.isRunning);
            animator.SetInteger(Settings.toolEffect, (int)movementParams.toolEffect);

            // 工具使用状态
            SetDirectionalTriggers(movementParams, Settings.ActionType.UsingTool,
                movementParams.isUsingToolUp, movementParams.isUsingToolDown,
                movementParams.isUsingToolLeft, movementParams.isUsingToolRight);

            // 工具抬起状态
            SetDirectionalTriggers(movementParams, Settings.ActionType.LiftingTool,
                movementParams.isLiftingToolUp, movementParams.isLiftingToolDown,
                movementParams.isLiftingToolLeft, movementParams.isLiftingToolRight);

            // 拾取状态
            SetDirectionalTriggers(movementParams, Settings.ActionType.Picking,
                movementParams.isPickingUp, movementParams.isPickingDown,
                movementParams.isPickingLeft, movementParams.isPickingRight);

            // 工具挥动状态
            SetDirectionalTriggers(movementParams, Settings.ActionType.SwingingTool,
                movementParams.isSwingingToolUp, movementParams.isSwingingToolDown,
                movementParams.isSwingingToolLeft, movementParams.isSwingingToolRight);

            // 空闲方向状态
            SetDirectionalTriggers(movementParams, Settings.ActionType.Idle,
                movementParams.isIdleUp, movementParams.isIdleDown,
                movementParams.isIdleLeft, movementParams.isIdleRight);
        }

        private void SetDirectionalTriggers(MovementParameters movementParams, Settings.ActionType actionType,
            bool isUp, bool isDown, bool isLeft, bool isRight)
        {
            if (isUp)
            {
                animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Up));
            }
            if (isDown)
            {
                animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Down));
            }
            if (isLeft)
            {
                animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Left));
            }
            if (isRight)
            {
                animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Right));
            }
        }

        private void AnimationEventPlayFootstepSound()
        {

        }
    }
}
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    [RequireComponent(typeof(Animator))]
    public class MovementAnimationParameterControl : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
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
            _animator.SetFloat(Settings.inputX, movementParams.inputX);
            _animator.SetFloat(Settings.inputY, movementParams.inputY);
            _animator.SetBool(Settings.isWalking, movementParams.isWalking);
            _animator.SetBool(Settings.isRunning, movementParams.isRunning);
            _animator.SetInteger(Settings.toolEffect, (int)movementParams.toolEffect);

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
                _animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Up));
            }
            if (isDown)
            {
                _animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Down));
            }
            if (isLeft)
            {
                _animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Left));
            }
            if (isRight)
            {
                _animator.SetTrigger(Settings.GetDirectionalAnimation(actionType, Settings.Direction.Right));
            }
        }

        private void AnimationEventPlayFootstepSound()
        {

        }
    }
}
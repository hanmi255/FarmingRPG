using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    [RequireComponent(typeof(Animator))]
    public class MovementAnimationParameterControl : MonoBehaviour
    {
        #region Fields
        private Animator _animator;  // 动画控制器
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 在对象初始化时获取Animator组件引用
        /// </summary>
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 当启用对象时订阅移动事件
        /// </summary>
        private void OnEnable()
        {
            EventHandler.MovementEvent += SetAnimationParameters;
        }

        /// <summary>
        /// 当禁用对象时取消订阅移动事件
        /// </summary>
        private void OnDisable()
        {
            EventHandler.MovementEvent -= SetAnimationParameters;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 设置动画参数以响应移动事件
        /// </summary>
        /// <param name="movementParams">移动参数</param>
        private void SetAnimationParameters(MovementParameters movementParams)
        {
            // 基本移动输入
            _animator.SetFloat(Settings.inputX, movementParams.inputX);
            _animator.SetFloat(Settings.inputY, movementParams.inputY);
            _animator.SetBool(Settings.isWalking, movementParams.isWalking);
            _animator.SetBool(Settings.isRunning, movementParams.isRunning);
            _animator.SetInteger(Settings.toolEffect, (int)movementParams.toolEffect);

            // 工具使用状态
            SetDirectionalTriggers(Settings.ActionType.UsingTool,
                movementParams.isUsingToolUp, movementParams.isUsingToolDown,
                movementParams.isUsingToolLeft, movementParams.isUsingToolRight);

            // 工具抬起状态
            SetDirectionalTriggers(Settings.ActionType.LiftingTool,
                movementParams.isLiftingToolUp, movementParams.isLiftingToolDown,
                movementParams.isLiftingToolLeft, movementParams.isLiftingToolRight);

            // 拾取状态
            SetDirectionalTriggers(Settings.ActionType.Picking,
                movementParams.isPickingUp, movementParams.isPickingDown,
                movementParams.isPickingLeft, movementParams.isPickingRight);

            // 工具挥动状态
            SetDirectionalTriggers(Settings.ActionType.SwingingTool,
                movementParams.isSwingingToolUp, movementParams.isSwingingToolDown,
                movementParams.isSwingingToolLeft, movementParams.isSwingingToolRight);

            // 空闲方向状态
            SetDirectionalTriggers(Settings.ActionType.Idle,
                movementParams.isIdleUp, movementParams.isIdleDown,
                movementParams.isIdleLeft, movementParams.isIdleRight);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 设置方向性触发器
        /// </summary>
        /// <param name="actionType">动作类型</param>
        /// <param name="isUp">是否向上</param>
        /// <param name="isDown">是否向下</param>
        /// <param name="isLeft">是否向左</param>
        /// <param name="isRight">是否向右</param>
        private void SetDirectionalTriggers(Settings.ActionType actionType,
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

        /// <summary>
        /// 动画事件：播放脚步声
        /// </summary>
        private void AnimationEventPlayFootstepSound()
        {

        }
        #endregion
    }
}
using System.Collections.Generic;
using Assets.Scripts.Animation;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AnimationOverrides))]
    public class PlayerUnit : SingletonMonoBehaviour<PlayerUnit>
    {
        // 动画覆盖
        private AnimationOverrides _animationOverrides;

        // 基本移动输入
        private float _inputX;
        private float _inputY;
        private bool _isWalking;
        private bool _isRunning;
        private bool _isIdle;
        private bool _isCarrying = false;
        private ToolEffect _toolEffect;

        // 工具使用状态
        private bool _isUsingToolUp;
        private bool _isUsingToolDown;
        private bool _isUsingToolLeft;
        private bool _isUsingToolRight;

        // 工具抬起状态
        private bool _isLiftingToolUp;
        private bool _isLiftingToolDown;
        private bool _isLiftingToolLeft;
        private bool _isLiftingToolRight;

        // 拾取状态
        private bool _isPickingUp;
        private bool _isPickingDown;
        private bool _isPickingLeft;
        private bool _isPickingRight;

        // 工具挥动状态
        private bool _isSwingingToolUp;
        private bool _isSwingingToolDown;
        private bool _isSwingingToolLeft;
        private bool _isSwingingToolRight;

        private Rigidbody2D _rigidbody2D;

        private Direction _direction;

        private List<CharacterAttribute> _characterAttributeCustomisationList;
        private float _movementSpeed;

        [SerializeField] private SpriteRenderer _equippedItemSpriteRenderer = null;

        private CharacterAttribute _armsCharacterAttribute;
        private CharacterAttribute _toolCharacterAttribute;

        private bool _isInputDisabled = false;

        private MovementParameters _parameters;

        public bool IsInputDisabled
        {
            get => _isInputDisabled;
            set => _isInputDisabled = value;
        }

        private Camera _mainCamera;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody2D = GetComponent<Rigidbody2D>();

            _animationOverrides = GetComponent<AnimationOverrides>();
            _armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.Arms, PartVariantColour.None, PartVariantType.None);
            _characterAttributeCustomisationList = new List<CharacterAttribute>();

            _mainCamera = Camera.main;
        }

        private void Update()
        {
            #region 处理输入

            if (!IsInputDisabled)
            {
                ResetAnimationTriggers();
                PlayerMovementInput();
                PlayerWalkInput();

                SetMovementParameters();
                EventHandler.CallMovementEvent(_parameters);
            }

            #endregion
        }

        private void FixedUpdate()
        {
            PlayerMovement();
        }

        public void DisablePlayerInputAndResetMovement()
        {
            DisablePlayerInput();
            ResetMovement();

            EventHandler.CallMovementEvent(_parameters);
        }

        private void ResetAnimationTriggers()
        {
            _toolEffect = ToolEffect.None;
            _isUsingToolUp = _isUsingToolDown = _isUsingToolLeft = _isUsingToolRight = false;
            _isLiftingToolUp = _isLiftingToolDown = _isLiftingToolLeft = _isLiftingToolRight = false;
            _isPickingUp = _isPickingDown = _isPickingLeft = _isPickingRight = false;
            _isSwingingToolUp = _isSwingingToolDown = _isSwingingToolLeft = _isSwingingToolRight = false;
        }

        private void PlayerMovementInput()
        {
            _inputX = Input.GetAxisRaw("Horizontal");
            _inputY = Input.GetAxisRaw("Vertical");

            // 如果同时在两个轴上移动，调整速度以保持总速度一致
            if (_inputX != 0 && _inputY != 0)
            {
                // 确保斜向移动
                _inputX *= 0.71f;
                _inputY *= 0.71f;
            }

            // 检查是否有移动输入
            bool hasInput = _inputX != 0 || _inputY != 0;

            // 更新移动状态
            _isRunning = hasInput;
            _isWalking = false;
            _isIdle = !hasInput;

            // 设置移动速度
            _movementSpeed = hasInput ? Settings.runningSpeed : 0f;

            // 如果有输入，则更新玩家方向
            if (hasInput)
            {
                if (_inputX < 0)
                {
                    _direction = Direction.Left;
                }
                else if (_inputX > 0)
                {
                    _direction = Direction.Right;
                }
                else if (_inputY < 0)
                {
                    _direction = Direction.Down;
                }
                else
                {
                    _direction = Direction.Up;
                }
            }
        }

        private void PlayerWalkInput()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                _isRunning = true;
                _isWalking = false;
                _isIdle = false;
                _movementSpeed = Settings.runningSpeed;
            }
            else
            {
                _isRunning = false;
                _isWalking = true;
                _isIdle = false;
                _movementSpeed = Settings.walkingSpeed;
            }
        }

        public void EnablePlayerInput()
        {
            IsInputDisabled = false;
        }

        private void DisablePlayerInput()
        {
            IsInputDisabled = true;
        }

        /// <summary>
        /// 展示玩家正在使用的物品
        /// </summary>
        public void ShowCarriedItem(int itemCode)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
            if (itemDetails != null)
            {
                _equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
                _equippedItemSpriteRenderer.color = Color.white;

                _armsCharacterAttribute.partVariantType = PartVariantType.Carry;
                _characterAttributeCustomisationList.Clear();
                _characterAttributeCustomisationList.Add(_armsCharacterAttribute);
                _animationOverrides.ApplyCharacterCustomisationParameters(_characterAttributeCustomisationList);

                _isCarrying = true;
            }
        }

        /// <summary>
        /// 清除玩家正在使用的物品
        /// </summary>
        public void ClearCarriedItem()
        {
            _equippedItemSpriteRenderer.sprite = null;
            _equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

            _armsCharacterAttribute.partVariantType = PartVariantType.None;
            _characterAttributeCustomisationList.Clear();
            _characterAttributeCustomisationList.Add(_armsCharacterAttribute);
            _animationOverrides.ApplyCharacterCustomisationParameters(_characterAttributeCustomisationList);

            _isCarrying = false;
        }

        private void ResetMovement()
        {
            _inputX = 0f;
            _inputY = 0f;
            _isWalking = false;
            _isRunning = false;
            _isIdle = true;

            SetMovementParameters();
        }

        private void SetMovementParameters()
        {
            _parameters = new()
            {
                inputX = _inputX,
                inputY = _inputY,
                isWalking = _isWalking,
                isRunning = _isRunning,
                isIdle = _isIdle,
                isCarrying = _isCarrying,
                toolEffect = _toolEffect,

                isUsingToolUp = _isUsingToolUp,
                isUsingToolDown = _isUsingToolDown,
                isUsingToolLeft = _isUsingToolLeft,
                isUsingToolRight = _isUsingToolRight,

                isLiftingToolUp = _isLiftingToolUp,
                isLiftingToolDown = _isLiftingToolDown,
                isLiftingToolLeft = _isLiftingToolLeft,
                isLiftingToolRight = _isLiftingToolRight,

                isPickingUp = _isPickingUp,
                isPickingDown = _isPickingDown,
                isPickingLeft = _isPickingLeft,
                isPickingRight = _isPickingRight,

                isSwingingToolUp = _isSwingingToolUp,
                isSwingingToolDown = _isSwingingToolDown,
                isSwingingToolLeft = _isSwingingToolLeft,
                isSwingingToolRight = _isSwingingToolRight,

                isIdleUp = false,
                isIdleDown = false,
                isIdleLeft = false,
                isIdleRight = false
            };
        }

        private void PlayerMovement()
        {
            Vector2 moveDistance = new(_inputX * _movementSpeed * Time.deltaTime, _inputY * _movementSpeed * Time.deltaTime);
            _rigidbody2D.MovePosition(_rigidbody2D.position + moveDistance);
        }

        public Vector3 GetPlayerViewporPosition()
        {
            return _mainCamera.WorldToViewportPoint(transform.position);
        }
    }
}
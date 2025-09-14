using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Animation;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Map;
using Assets.Scripts.Misc;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AnimationOverrides))]
    public class PlayerUnit : SingletonMonoBehaviour<PlayerUnit>
    {
        #region Fields

        #region Serialization Fields
        [SerializeField] private SpriteRenderer _equippedItemSpriteRenderer = null;
        #endregion

        #region Private Fields - Animation
        // 使用工具动画
        private WaitForSeconds _useToolAnimationPause;
        private WaitForSeconds _afterUseToolAnimationPause;
        private bool _toolUseDisabled = false;

        // 动画覆盖
        private AnimationOverrides _animationOverrides;

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
        #endregion

        #region Private Fields - Movement
        // 基本移动输入
        private float _inputX;
        private float _inputY;
        private bool _isWalking;
        private bool _isRunning;
        private bool _isIdle;
        private bool _isCarrying = false;
        private ToolEffect _toolEffect;
        private Direction _direction;
        private Rigidbody2D _rigidbody2D;
        private float _movementSpeed;
        private MovementParameters _parameters;
        #endregion

        #region Private Fields - Components
        // 网格光标高亮
        private GridCursorHighlight _gridCursorHighlight;
        private Camera _mainCamera;
        #endregion

        #region Private Fields - Character Customization
        private List<CharacterAttribute> _characterAttributeCustomisationList;
        private CharacterAttribute _armsCharacterAttribute;
        private CharacterAttribute _toolCharacterAttribute;
        #endregion

        #region Private Fields - State
        private bool _isInputDisabled = false;
        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// 获取或设置玩家输入是否被禁用
        /// </summary>
        public bool IsInputDisabled
        {
            get => _isInputDisabled;
            set => _isInputDisabled = value;
        }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 初始化组件引用和基本设置
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _rigidbody2D = GetComponent<Rigidbody2D>();

            _animationOverrides = GetComponent<AnimationOverrides>();
            _armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.Arms, PartVariantColour.None, PartVariantType.None);
            _characterAttributeCustomisationList = new List<CharacterAttribute>();

            _mainCamera = Camera.main;
        }

        /// <summary>
        /// 初始化游戏对象引用和动画暂停时间
        /// </summary>
        private void Start()
        {
            _gridCursorHighlight = FindObjectOfType<GridCursorHighlight>();
            _useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
            _afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        }

        /// <summary>
        /// 每帧处理玩家输入和动画触发器
        /// </summary>
        private void Update()
        {
            if (!IsInputDisabled)
            {
                ResetAnimationTriggers();
                PlayerMovementInput();
                PlayerWalkInput();
                PlayerClickInput();

                SetMovementParameters();
                EventHandler.CallMovementEvent(_parameters);
            }
        }

        /// <summary>
        /// 固定时间步长更新，处理玩家移动
        /// </summary>
        private void FixedUpdate()
        {
            PlayerMovement();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 禁用玩家输入并重置移动状态
        /// </summary>
        public void DisablePlayerInputAndResetMovement()
        {
            DisablePlayerInput();
            ResetMovement();

            EventHandler.CallMovementEvent(_parameters);
        }

        /// <summary>
        /// 展示玩家正在使用的物品
        /// </summary>
        /// <param name="itemCode">物品代码</param>
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

        /// <summary>
        /// 启用玩家输入
        /// </summary>
        public void EnablePlayerInput()
        {
            IsInputDisabled = false;
        }

        /// <summary>
        /// 获取玩家在视口中的位置
        /// </summary>
        /// <returns>玩家在视口中的位置</returns>
        public Vector3 GetPlayerViewporPosition()
        {
            return _mainCamera.WorldToViewportPoint(transform.position);
        }
        #endregion

        #region Private Methods - Input Handling
        /// <summary>
        /// 重置所有动画触发器
        /// </summary>
        private void ResetAnimationTriggers()
        {
            _toolEffect = ToolEffect.None;
            _isUsingToolUp = _isUsingToolDown = _isUsingToolLeft = _isUsingToolRight = false;
            _isLiftingToolUp = _isLiftingToolDown = _isLiftingToolLeft = _isLiftingToolRight = false;
            _isPickingUp = _isPickingDown = _isPickingLeft = _isPickingRight = false;
            _isSwingingToolUp = _isSwingingToolDown = _isSwingingToolLeft = _isSwingingToolRight = false;
        }

        /// <summary>
        /// 处理玩家基本移动输入
        /// </summary>
        private void PlayerMovementInput()
        {
            _inputX = Input.GetAxisRaw("Horizontal");
            _inputY = Input.GetAxisRaw("Vertical");

            // 如果同时在两个轴上移动，调整速度以保持总速度一致
            if (_inputX != 0 && _inputY != 0)
            {
                // 确保斜向移动时的速度与单轴移动相同
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
            if (!hasInput) 
                return;

            // 根据输入确定玩家方向
            if (_inputX != 0)
            {
                _direction = _inputX < 0 ? Direction.Left : Direction.Right;
            }
            else if (_inputY != 0)
            {
                _direction = _inputY < 0 ? Direction.Down : Direction.Up;
            }
            else
            {
                // 默认方向为上
                _direction = Direction.Up;
            }
        }

        /// <summary>
        /// 处理玩家行走输入（与跑步切换）
        /// </summary>
        private void PlayerWalkInput()
        {
            // 检查是否按下Shift键（跑步键）
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (isRunning)
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

        /// <summary>
        /// 处理玩家鼠标点击输入
        /// </summary>
        private void PlayerClickInput()
        {
            // 如果工具使用被禁用，则不处理点击输入
            if (_toolUseDisabled) 
                return;
            
            // 如果没有按下鼠标左键，则不处理点击输入
            if (!Input.GetMouseButton(0)) 
                return;
            
            // 如果光标未启用，则不处理点击输入
            if (!_gridCursorHighlight.CursorEnabled) 
                return;

            Vector3Int cursorGridPosition = _gridCursorHighlight.GetGridPositionForCursor();
            Vector3Int playerGridPosition = _gridCursorHighlight.GetGridPositionForPlayer();

            ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
        }

        /// <summary>
        /// 处理玩家点击输入的具体逻辑
        /// </summary>
        /// <param name="cursorGridPosition">光标网格位置</param>
        /// <param name="playerGridPosition">玩家网格位置</param>
        private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
        {
            ResetMovement();

            Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

            GridPropertyDetails gridPropertyDetails = GridPropertyManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

            ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.Player);

            // 如果没有选中物品，则不处理
            if (itemDetails == null)
                return;

            // 处理需要鼠标按下事件的物品类型
            if (itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    DropItem(itemDetails);
                }
            }
            // 处理工具类型物品
            else if (itemDetails.itemType == ItemType.HoeingTool)
            {
                UseTool(gridPropertyDetails, itemDetails, playerDirection);
            }
        }
        #endregion

        #region Private Methods - Movement
        /// <summary>
        /// 处理玩家物理移动
        /// </summary>
        private void PlayerMovement()
        {
            Vector2 moveDistance = new(_inputX * _movementSpeed * Time.deltaTime, _inputY * _movementSpeed * Time.deltaTime);
            _rigidbody2D.MovePosition(_rigidbody2D.position + moveDistance);
        }

        /// <summary>
        /// 重置玩家移动状态
        /// </summary>
        private void ResetMovement()
        {
            _inputX = 0f;
            _inputY = 0f;
            _isWalking = false;
            _isRunning = false;
            _isIdle = true;

            SetMovementParameters();
        }

        /// <summary>
        /// 设置移动参数并更新事件系统
        /// </summary>
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
        #endregion

        #region Private Methods - Actions
        /// <summary>
        /// 丢弃选中的物品
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        private void DropItem(ItemDetails itemDetails)
        {
            if (itemDetails.canBeDropped && _gridCursorHighlight.CursorPositionIsValid)
            {
                EventHandler.CallDropSelectedItemEvent();
            }
        }

        /// <summary>
        /// 使用工具处理网格属性
        /// </summary>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        /// <param name="itemDetails">物品详情</param>
        /// <param name="playerDirection">玩家方向</param>
        private void UseTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
        {
            // 只有锄头工具且光标位置有效时才处理
            if (itemDetails.itemType == ItemType.HoeingTool && _gridCursorHighlight.CursorPositionIsValid)
            {
                HoeGroundAtCursor(gridPropertyDetails, playerDirection);
            }
        }

        /// <summary>
        /// 在光标位置锄地
        /// </summary>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        /// <param name="playerDirection">玩家方向</param>
        private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
        {
            StartCoroutine(HoeGroundAtCursorCoroutine(gridPropertyDetails, playerDirection));
        }

        /// <summary>
        /// 锄地协程，处理动画和网格属性更新
        /// </summary>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        /// <param name="playerDirection">玩家方向</param>
        /// <returns>IEnumerator用于协程</returns>
        private IEnumerator HoeGroundAtCursorCoroutine(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
        {
            // 禁用输入和工具使用，防止重复操作
            IsInputDisabled = true;
            _toolUseDisabled = true;

            // 设置工具动画参数
            _toolCharacterAttribute.partVariantType = PartVariantType.Hoe;
            _characterAttributeCustomisationList.Clear();
            _characterAttributeCustomisationList.Add(_toolCharacterAttribute);
            _animationOverrides.ApplyCharacterCustomisationParameters(_characterAttributeCustomisationList);

            // 根据玩家方向设置对应的动画状态
            if (playerDirection == Vector3Int.right)
            {
                _isUsingToolRight = true;
            }
            else if (playerDirection == Vector3Int.left)
            {
                _isUsingToolLeft = true;
            }
            else if (playerDirection == Vector3Int.up)
            {
                _isUsingToolUp = true;
            }
            else if (playerDirection == Vector3Int.down)
            {
                _isUsingToolDown = true;
            }

            // 等待工具使用动画播放
            yield return _useToolAnimationPause;

            // 更新网格属性，标记为已锄地
            if (gridPropertyDetails.daysSinceLastDig == -1)
            {
                gridPropertyDetails.daysSinceLastDig = 0;
            }

            GridPropertyManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

            // 等待动画后延迟
            yield return _afterUseToolAnimationPause;

            // 重新启用输入和工具使用
            IsInputDisabled = false;
            _toolUseDisabled = false;
        }
        #endregion

        #region Private Methods - Utilities
        /// <summary>
        /// 根据光标位置和玩家位置计算玩家点击方向
        /// </summary>
        /// <param name="cursorGridPosition">光标网格位置</param>
        /// <param name="playerGridPosition">玩家网格位置</param>
        /// <returns>玩家点击方向</returns>
        private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
        {
            // 检查水平方向差异
            if (cursorGridPosition.x > playerGridPosition.x)
            {
                return Vector3Int.right;
            }
            if (cursorGridPosition.x < playerGridPosition.x)
            {
                return Vector3Int.left;
            }

            // 检查垂直方向差异
            if (cursorGridPosition.y > playerGridPosition.y)
            {
                return Vector3Int.up;
            }

            // 默认返回向下方向
            return Vector3Int.down;
        }

        /// <summary>
        /// 禁用玩家输入
        /// </summary>
        private void DisablePlayerInput()
        {
            IsInputDisabled = true;
        }
        #endregion
    }
}
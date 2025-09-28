using System;
using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Misc;
using Assets.Scripts.TimeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NPCPath))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class NPCMovement : MonoBehaviour
    {
        #region Fields
        [HideInInspector] public SceneName npcCurrentScene;
        [HideInInspector] public SceneName npcTargetScene;
        [HideInInspector] public Vector3Int npcCurrentGridPosition;
        [HideInInspector] public Vector3Int npcTargetGridPosition;
        [HideInInspector] public Vector3 npcTargetWorldPosition;
        [HideInInspector] public Direction npcFacingDirectionAtDestination;

        private SceneName _npcPreviousMovementStepScene;
        private Vector3Int _npcNextGridPosition;
        private Vector3 _npcNextWorldPosition;

        [Header("NPC Movement")]
        public float npcNormalSpeed = 2f;

        [SerializeField] private float _npcMinSpeed = 1f;
        [SerializeField] private float _npcMaxSpeed = 3f;
        private bool _isMoving = false;

        [HideInInspector] public AnimationClip npcTargetAnimationClip;

        [Header("NPC Animation")]
        [SerializeField] private AnimationClip _blankAnimation = null;

        private Grid _grid;
        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private NPCPath _npcPath;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider2D;
        private WaitForFixedUpdate _waitForFixedUpdate;
        private AnimatorOverrideController _animatorOverrideController;
        private int _lastMoveAnimationParameter;
        private bool _npcInitialised = false;
        [HideInInspector] public bool npcActiveInScene = false;

        private bool _sceneLoaded = false;

        private Coroutine _moveToGridPositionCoroutine;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _npcPath = GetComponent<NPCPath>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();

            _animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _animatorOverrideController;

            npcTargetScene = npcCurrentScene;
            npcTargetGridPosition = npcCurrentGridPosition;
            npcTargetWorldPosition = transform.position;
        }

        private void OnEnable()
        {
            Events.EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
            Events.EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        }

        private void OnDisable()
        {
            Events.EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
            Events.EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
        }

        private void Start()
        {
            _waitForFixedUpdate = new WaitForFixedUpdate();

            SetIdleAnimation();
        }

        /// <summary>
        /// FixedUpdate - 处理NPC移动和动画状态的主函数
        /// </summary>
        private void FixedUpdate()
        {
            if (!ValidateMovementPreConditions())
                return;

            UpdateNPCGridPosition();
            ProcessNPCMovement();
        }
        #endregion

        #region Public Methods
        public void SetNPCActiveInScene()
        {
            _spriteRenderer.enabled = true;
            _boxCollider2D.enabled = true;
            npcActiveInScene = true;
        }

        public void SetNPCInactiveInScene()
        {
            _spriteRenderer.enabled = false;
            _boxCollider2D.enabled = false;
            npcActiveInScene = false;
        }

        /// <summary>
        /// 设置NPC事件细节
        /// </summary>
        /// <param name="npcScheduleEvent">NPC事件</param>
        public void SetScheduleEventDetails(NPCScheduleEvent npcScheduleEvent)
        {
            npcTargetScene = npcScheduleEvent.toSceneName;
            npcTargetGridPosition = (Vector3Int)npcScheduleEvent.toGridCoordinate;
            npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
            npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination;
            npcTargetAnimationClip = npcScheduleEvent.animationAtDestination;

            ClearNPCEventAnimation();
        }

        /// <summary>
        /// 清除NPC事件动画
        /// </summary>
        private void ClearNPCEventAnimation()
        {
            _animatorOverrideController[_blankAnimation] = _blankAnimation;
            _animator.SetBool(Settings.eventAnimation, false);

            transform.rotation = Quaternion.identity;
        }
        #endregion

        #region Private Methods
        private void AfterSceneLoad()
        {
            _grid = FindObjectOfType<Grid>();

            if (!_npcInitialised)
            {
                InitialiseNPC();
                _npcInitialised = true;
            }

            _sceneLoaded = true;
        }

        private void BeforeSceneUnload()
        {
            _sceneLoaded = false;
        }

        #region Used For Fixed Update()
        /// <summary>
        /// 验证NPC移动的前置条件
        /// </summary>
        /// <returns>如果满足移动条件返回true，否则返回false</returns>
        private bool ValidateMovementPreConditions()
        {
            if (!_sceneLoaded)
                return false;

            if (_isMoving)
                return false;

            return true;
        }

        /// <summary>
        /// 更新NPC当前网格位置
        /// </summary>
        private void UpdateNPCGridPosition()
        {
            npcCurrentGridPosition = GetGridPosition(transform.position);
            _npcNextGridPosition = npcCurrentGridPosition;
        }

        /// <summary>
        /// 处理NPC移动的主逻辑
        /// </summary>
        private void ProcessNPCMovement()
        {
            if (_npcPath.npcMovementStepStack.Count > 0)
                ProcessMovementStep();
            else
                HandleNoMovementSteps();
        }

        /// <summary>
        /// 处理单个移动步骤
        /// </summary>
        private void ProcessMovementStep()
        {
            NPCMovementStep npcMovementStep = _npcPath.npcMovementStepStack.Peek();
            npcCurrentScene = npcMovementStep.sceneName;

            HandleSceneTransition(npcMovementStep);
            HandleMovementExecution(npcMovementStep);
        }

        /// <summary>
        /// 处理场景转换逻辑
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        private void HandleSceneTransition(NPCMovementStep npcMovementStep)
        {
            // 如果当前场景与上一步场景相同，则无需处理场景转换
            if (npcCurrentScene == _npcPreviousMovementStepScene)
                return;

            // 重置NPC位置到新场景的起始位置
            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
            _npcNextGridPosition = npcCurrentGridPosition;
            transform.position = GetWorldPosition(npcCurrentGridPosition);
            _npcPreviousMovementStepScene = npcCurrentScene;
            _npcPath.UpdateTimesOnPath();
        }

        /// <summary>
        /// 执行移动逻辑
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        private void HandleMovementExecution(NPCMovementStep npcMovementStep)
        {
            bool isInActiveScene = IsNPCInActiveScene();

            if (isInActiveScene)
            {
                HandleActiveSceneMovement(npcMovementStep);
            }
            else
            {
                HandleInactiveSceneMovement(npcMovementStep);
            }
        }

        /// <summary>
        /// 检查NPC是否在当前活动场景中
        /// </summary>
        /// <returns>如果在活动场景中返回true，否则返回false</returns>
        private bool IsNPCInActiveScene()
        {
            return npcCurrentScene.ToString() == SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// 处理活动场景中的移动
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        private void HandleActiveSceneMovement(NPCMovementStep npcMovementStep)
        {
            SetNPCActiveInScene();

            npcMovementStep = _npcPath.npcMovementStepStack.Pop();
            _npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;

            TimeSpan npcMovementStepTime = CreateTimeSpanFromMovementStep(npcMovementStep);
            MoveToGridPosition(_npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
        }

        /// <summary>
        /// 处理非活动场景中的移动
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        private void HandleInactiveSceneMovement(NPCMovementStep npcMovementStep)
        {
            SetNPCInactiveInScene();

            UpdateNPCPositionToMovementStep(npcMovementStep);

            TimeSpan npcMovementStepTime = CreateTimeSpanFromMovementStep(npcMovementStep);
            TimeSpan gameTime = TimeManager.Instance.GetGameTime();

            // 如果移动时间已过，则直接跳到下一个步骤
            if (ShouldSkipToNextStep(npcMovementStepTime, gameTime))
            {
                ProcessSkipToNextStep();
            }
        }

        /// <summary>
        /// 更新NPC位置到移动步骤指定的位置
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        private void UpdateNPCPositionToMovementStep(NPCMovementStep npcMovementStep)
        {
            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
            _npcNextGridPosition = npcCurrentGridPosition;
            transform.position = GetWorldPosition(npcCurrentGridPosition);
        }

        /// <summary>
        /// 检查是否应跳过到下一个步骤
        /// </summary>
        /// <param name="npcMovementStepTime">移动步骤时间</param>
        /// <param name="gameTime">游戏时间</param>
        /// <returns>如果应跳过返回true，否则返回false</returns>
        private bool ShouldSkipToNextStep(TimeSpan npcMovementStepTime, TimeSpan gameTime)
        {
            return npcMovementStepTime < gameTime;
        }

        /// <summary>
        /// 处理跳过到下一个步骤的逻辑
        /// </summary>
        private void ProcessSkipToNextStep()
        {
            NPCMovementStep npcMovementStep = _npcPath.npcMovementStepStack.Pop();
            UpdateNPCPositionToMovementStep(npcMovementStep);
        }

        /// <summary>
        /// 从移动步骤创建TimeSpan对象
        /// </summary>
        /// <param name="npcMovementStep">NPC移动步骤</param>
        /// <returns>TimeSpan对象</returns>
        private TimeSpan CreateTimeSpanFromMovementStep(NPCMovementStep npcMovementStep)
        {
            return new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
        }

        /// <summary>
        /// 处理没有移动步骤时的状态
        /// </summary>
        private void HandleNoMovementSteps()
        {
            ResetMoveAnimation();
            SetNPCFacingDirection();
            SetNPCEventAnimation();
        }
        #endregion

        private void InitialiseNPC()
        {
            if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
            {
                SetNPCActiveInScene();
            }
            else
            {
                SetNPCInactiveInScene();
            }

            _npcPreviousMovementStepScene = npcCurrentScene;

            npcCurrentGridPosition = GetGridPosition(transform.position);

            _npcNextGridPosition = npcCurrentGridPosition;
            npcTargetGridPosition = npcCurrentGridPosition;
            npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
            _npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
        }

        /// <summary>
        /// 获取网格位置
        /// </summary>
        /// <param name="worldPosition">世界位置</param>
        /// <returns>网格位置</returns>
        private Vector3Int GetGridPosition(Vector3 worldPosition)
        {
            if (_grid == null)
                return Vector3Int.zero;

            return _grid.WorldToCell(worldPosition);
        }

        /// <summary>
        /// 获取世界位置
        /// </summary>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>世界位置</returns>
        private Vector3 GetWorldPosition(Vector3Int gridPosition)
        {
            Vector3 worldPosition = _grid.CellToWorld(gridPosition);

            return new(worldPosition.x + Settings.halfGridCellSize, worldPosition.y + Settings.halfGridCellSize, worldPosition.z);
        }

        private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
        {
            if (_moveToGridPositionCoroutine != null)
                StopCoroutine(_moveToGridPositionCoroutine);

            _moveToGridPositionCoroutine = StartCoroutine(MoveToGridPositionCoroutine(gridPosition, npcMovementStepTime, gameTime));
        }

        /// <summary>
        /// 移动到网格位置的协程
        /// </summary>
        /// <param name="gridPosition">目标网格位置</param>
        /// <param name="npcMovementStepTime">NPC移动步长时间</param>
        /// <param name="gameTime">游戏时间</param>
        /// <returns></returns>
        private IEnumerator MoveToGridPositionCoroutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
        {
            _isMoving = true;
            SetMoveAnimation(gridPosition);
            _npcNextWorldPosition = GetWorldPosition(gridPosition);

            if (npcMovementStepTime > gameTime)
            {
                float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);
                float npcCalculatedSpeed = Vector3.Distance(transform.position, _npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond;

                if (npcCalculatedSpeed <= _npcMaxSpeed)
                {
                    // 主要的移动逻辑
                    while (Vector3.Distance(transform.position, _npcNextWorldPosition) > Settings.pixelSize)
                    {
                        Vector3 unitVector = Vector3.Normalize(_npcNextWorldPosition - transform.position);
                        Vector2 move = new(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime,
                                          unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);

                        _rigidbody2D.MovePosition(_rigidbody2D.position + move);
                        yield return _waitForFixedUpdate;
                    }
                }
            }

            _rigidbody2D.position = _npcNextWorldPosition;
            npcCurrentGridPosition = gridPosition;
            _npcNextGridPosition = npcCurrentGridPosition;
            _isMoving = false;
        }

        /// <summary>
        /// 设置移动动画
        /// </summary>
        /// <param name="gridPosition">目标网格位置</param>
        private void SetMoveAnimation(Vector3Int gridPosition)
        {
            ResetIdleAnimation();
            ResetMoveAnimation();

            Vector3 toWorldPosition = GetWorldPosition(gridPosition);
            Vector3 direction = toWorldPosition - transform.position;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                {
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Right), true);
                }
                else
                {
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Left), true);
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Up), true);
                }
                else
                {
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Down), true);
                }
            }
        }

        /// <summary>
        /// 设置空闲动画
        /// </summary>
        private void SetIdleAnimation()
        {
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Down), true);
        }

        /// <summary>
        /// 设置NPC朝向
        /// </summary>
        private void SetNPCFacingDirection()
        {
            ResetIdleAnimation();

            switch (npcFacingDirectionAtDestination)
            {
                case Direction.Up:
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Up), true);
                    break;
                case Direction.Down:
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Down), true);
                    break;
                case Direction.Left:
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Left), true);
                    break;
                case Direction.Right:
                    _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Right), true);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置NPC事件动画
        /// </summary>
        private void SetNPCEventAnimation()
        {
            if (npcTargetAnimationClip != null)
            {
                ResetIdleAnimation();
                _animatorOverrideController[_blankAnimation] = npcTargetAnimationClip;
                _animator.SetBool(Settings.eventAnimation, true);
            }
            else
            {
                _animatorOverrideController[_blankAnimation] = _blankAnimation;
                _animator.SetBool(Settings.eventAnimation, false);
            }
        }

        /// <summary>   
        /// 重置移动动画
        /// </summary>
        private void ResetMoveAnimation()
        {
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Right), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Left), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Up), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Walk, Settings.Direction.Down), false);
        }

        /// <summary>
        /// 重置空闲动画
        /// </summary>
        private void ResetIdleAnimation()
        {
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Right), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Left), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Up), false);
            _animator.SetBool(Settings.GetDirectionalAnimation(Settings.ActionType.Idle, Settings.Direction.Down), false);
        }
        #endregion
    }
}

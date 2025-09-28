using System;
using System.Collections.Generic;
using Assets.Scripts.Map;
using Assets.Scripts.Misc;
using Assets.Scripts.Scene;
using Assets.Scripts.TimeSystem;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(NPCMovement))]
    public class NPCPath : MonoBehaviour
    {
        #region Fields
        private NPCMovement _npcMovement;
        public Stack<NPCMovementStep> npcMovementStepStack;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _npcMovement = GetComponent<NPCMovement>();
            npcMovementStepStack = new();
        }
        #endregion

        #region Public Methods
        public void ClearPath()
        {
            npcMovementStepStack.Clear();
        }

        /// <summary>
        /// 构建路径
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        public void BuildPath(NPCScheduleEvent npcScheduleEvent)
        {
            ClearPath();

            // 检查是否为同场景路径构建
            if (IsSameSceneMovement(npcScheduleEvent))
            {
                BuildSameScenePath(npcScheduleEvent);
            }
            else
            {
                BuildCrossScenePath(npcScheduleEvent);
            }

            FinalizePathIfValid(npcScheduleEvent);
        }

        /// <summary>
        /// 更新路径上的时间
        /// </summary>
        public void UpdateTimesOnPath()
        {
            TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();
            ProcessMovementStepsWithTiming(currentGameTime);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 检查是否为同场景移动
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        /// <returns>是否为同场景移动</returns>
        private bool IsSameSceneMovement(NPCScheduleEvent npcScheduleEvent)
        {
            return npcScheduleEvent.toSceneName == _npcMovement.npcCurrentScene;
        }

        /// <summary>
        /// 构建同场景路径
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        private void BuildSameScenePath(NPCScheduleEvent npcScheduleEvent)
        {
            Vector2Int npcCurrentGridPosition = (Vector2Int)_npcMovement.npcCurrentGridPosition;
            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            NPCManager.Instance.BuildPath(
                npcScheduleEvent.toSceneName, 
                npcCurrentGridPosition, 
                npcTargetGridPosition, 
                npcMovementStepStack);
        }

        /// <summary>
        /// 构建跨场景路径
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        private void BuildCrossScenePath(NPCScheduleEvent npcScheduleEvent)
        {
            SceneRoute sceneRoute = GetSceneRoute(npcScheduleEvent);

            if (sceneRoute == null)
                return;

            ProcessScenePathList(sceneRoute, npcScheduleEvent);
        }

        /// <summary>
        /// 获取场景路线
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        /// <returns>场景路线</returns>
        private SceneRoute GetSceneRoute(NPCScheduleEvent npcScheduleEvent)
        {
            return NPCManager.Instance.GetSceneRoute(
                _npcMovement.npcCurrentScene.ToString(), 
                npcScheduleEvent.toSceneName.ToString());
        }

        /// <summary>
        /// 处理场景路径列表
        /// </summary>
        /// <param name="sceneRoute">场景路线</param>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        private void ProcessScenePathList(SceneRoute sceneRoute, NPCScheduleEvent npcScheduleEvent)
        {
            for (int i = sceneRoute.scenePathList.Count - 1; i >= 0; i--)
            {
                ScenePath scenePath = sceneRoute.scenePathList[i];
                ProcessSingleScenePath(scenePath, npcScheduleEvent);
            }
        }

        /// <summary>
        /// 处理单个场景路径
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        private void ProcessSingleScenePath(ScenePath scenePath, NPCScheduleEvent npcScheduleEvent)
        {
            Vector2Int toGridPosition = CalculateToGridPosition(scenePath, npcScheduleEvent);
            Vector2Int fromGridPosition = CalculateFromGridPosition(scenePath);

            NPCManager.Instance.BuildPath(
                scenePath.sceneName, 
                fromGridPosition, 
                toGridPosition, 
                npcMovementStepStack);
        }

        /// <summary>
        /// 计算目标网格位置
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        /// <returns>目标网格位置</returns>
        private Vector2Int CalculateToGridPosition(ScenePath scenePath, NPCScheduleEvent npcScheduleEvent)
        {
            // 如果目标网格超出边界，使用事件坐标
            if (IsGridCellOutOfBounds(scenePath.toGridCell))
                return (Vector2Int)npcScheduleEvent.toGridCoordinate;

            return (Vector2Int)scenePath.toGridCell;
        }

        /// <summary>
        /// 计算起始网格位置
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <returns>起始网格位置</returns>
        private Vector2Int CalculateFromGridPosition(ScenePath scenePath)
        {
            // 如果起始网格超出边界，使用当前位置
            if (IsGridCellOutOfBounds(scenePath.fromGridCell))
                return (Vector2Int)_npcMovement.npcCurrentGridPosition;

            return (Vector2Int)scenePath.fromGridCell;
        }

        /// <summary>
        /// 检查网格单元是否超出边界
        /// </summary>
        /// <param name="gridCell">网格单元</param>
        /// <returns>是否超出边界</returns>
        private bool IsGridCellOutOfBounds(GridCoordinate gridCell)
        {
            return gridCell.x >= Settings.maxGridWidth || gridCell.y >= Settings.maxGridHeight;
        }

        /// <summary>
        /// 如果路径有效则完成路径构建
        /// </summary>
        /// <param name="npcScheduleEvent">NPC调度事件</param>
        private void FinalizePathIfValid(NPCScheduleEvent npcScheduleEvent)
        {
            // 如果路径步骤少于2步则不处理
            if (npcMovementStepStack.Count <= 1)
                return;

            UpdateTimesOnPath();
            npcMovementStepStack.Pop();
            _npcMovement.SetScheduleEventDetails(npcScheduleEvent);
        }

        /// <summary>
        /// 处理移动步骤并计算时间
        /// </summary>
        /// <param name="currentGameTime">当前游戏时间</param>
        private void ProcessMovementStepsWithTiming(TimeSpan currentGameTime)
        {
            NPCMovementStep previousStep = null;

            foreach (NPCMovementStep currentStep in npcMovementStepStack)
            {
                // 第一步使用自身作为参考点
                previousStep ??= currentStep;

                UpdateStepTimeValues(currentStep, currentGameTime);

                TimeSpan movementDuration = CalculateMovementDuration(currentStep, previousStep);
                currentGameTime = currentGameTime.Add(movementDuration);

                previousStep = currentStep;
            }
        }

        /// <summary>
        /// 更新单个移动步骤的时间值
        /// </summary>
        /// <param name="step">移动步骤</param>
        /// <param name="gameTime">游戏时间</param>
        private void UpdateStepTimeValues(NPCMovementStep step, TimeSpan gameTime)
        {
            step.hour = gameTime.Hours;
            step.minute = gameTime.Minutes;
            step.second = gameTime.Seconds;
        }

        /// <summary>
        /// 计算移动持续时间
        /// </summary>
        /// <param name="currentStep">当前步骤</param>
        /// <param name="previousStep">前一步骤</param>
        /// <returns>移动持续时间</returns>
        private TimeSpan CalculateMovementDuration(NPCMovementStep currentStep, NPCMovementStep previousStep)
        {
            float gridSize = MovementIsDiagonal(currentStep, previousStep)
                ? Settings.gridDiagonalSize
                : Settings.gridCellSize;

            int movementSeconds = CalculateMovementSeconds(gridSize);
            return new TimeSpan(0, 0, movementSeconds);
        }

        /// <summary>
        /// 判断移动是否为对角线
        /// </summary>
        /// <param name="npcMovementStep"></param>
        /// <param name="previousNPCMovementStep"></param>
        /// <returns></returns>
        private bool MovementIsDiagonal(NPCMovementStep npcMovementStep, NPCMovementStep previousNPCMovementStep)
        {
            return npcMovementStep.gridCoordinate.x != previousNPCMovementStep.gridCoordinate.x && npcMovementStep.gridCoordinate.y != previousNPCMovementStep.gridCoordinate.y;
        }

        /// <summary>
        /// 计算移动所需秒数
        /// </summary>
        /// <param name="gridSize">网格大小</param>
        /// <returns>移动秒数</returns>
        private int CalculateMovementSeconds(float gridSize)
        {
            return (int)(gridSize / Settings.secondsPerGameSecond / _npcMovement.npcNormalSpeed);
        }
        #endregion
    }
}
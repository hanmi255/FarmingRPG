using System;
using System.Collections.Generic;
using Assets.Scripts.Misc;
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
        /// <param name="npcScheduleEvent"></param>
        public void BuildPath(NPCScheduleEvent npcScheduleEvent)
        {
            ClearPath();

            if (npcScheduleEvent.toSceneName != _npcMovement.npcCurrentScene)
                return;

            Vector2Int npcCurrentGridPosition = (Vector2Int)_npcMovement.npcCurrentGridPosition;
            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, npcTargetGridPosition, npcMovementStepStack);

            if (npcMovementStepStack.Count > 1)
            {
                UpdateTimesOnPath();
                npcMovementStepStack.Pop();

                _npcMovement.SetScheduleEventDetails(npcScheduleEvent);
            }
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
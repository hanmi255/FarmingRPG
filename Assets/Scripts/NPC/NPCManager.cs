using System.Collections.Generic;
using Assets.Scripts.AStarAlgorithm;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(AStar))]
    public class NPCManager : SingletonMonoBehaviour<NPCManager>
    {
        [HideInInspector] public NPCUnit[] npcUnits;
        private AStar _aStar;

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            _aStar = GetComponent<AStar>();
            npcUnits = FindObjectsOfType<NPCUnit>();
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        }
        #endregion

        #region Public Methods
        public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition, Stack<NPCMovementStep> npcMovementStepStack)
        {
            return _aStar.BuildPath(sceneName, startGridPosition, targetGridPosition, npcMovementStepStack);
        }
        #endregion

        #region Private Methods
        private void AfterSceneLoad()
        {
            SetNPCsActiveStatus();
        }

        /// <summary>
        /// 设置NPC的活动状态
        /// </summary>
        private void SetNPCsActiveStatus()
        {
            foreach (NPCUnit npcUnit in npcUnits)
            {
                NPCMovement npcMovement = npcUnit.GetComponent<NPCMovement>();

                if (npcMovement.npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
                {
                    npcMovement.SetNPCActiveInScene();
                }
                else
                {
                    npcMovement.SetNPCInactiveInScene();
                }
            }
        }
        #endregion
    }
}

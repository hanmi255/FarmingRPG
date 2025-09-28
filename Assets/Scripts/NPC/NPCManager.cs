using System.Collections.Generic;
using Assets.Scripts.AStarAlgorithm;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(AStar))]
    public class NPCManager : SingletonMonoBehaviour<NPCManager>
    {
        [SerializeField] private SO_SceneRouteList so_SceneRouteList = null;
        private Dictionary<string, SceneRoute> sceneRouteDictionary;

        [HideInInspector] public NPCUnit[] npcUnits;
        private AStar _aStar;

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            sceneRouteDictionary = new Dictionary<string, SceneRoute>();

            if (so_SceneRouteList.sceneRouteList.Count > 0)
            {
                foreach (SceneRoute sceneRoute in so_SceneRouteList.sceneRouteList)
                {
                    string sceneRouteKey = sceneRoute.fromSceneName.ToString() + sceneRoute.toSceneName.ToString();

                    if (sceneRouteDictionary.ContainsKey(sceneRouteKey))
                    {
                        Debug.LogWarning($"Duplicate scene route found: {sceneRouteKey}");
                        continue;
                    }

                    sceneRouteDictionary.Add(sceneRouteKey, sceneRoute);
                }
            }

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

        /// <summary>
        /// 获取场景路径
        /// </summary>
        /// <param name="fromSceneName"></param>
        /// <param name="toSceneName"></param>
        /// <returns></returns>
        public SceneRoute GetSceneRoute(string fromSceneName, string toSceneName)
        {
            string sceneRouteKey = fromSceneName + toSceneName;

            if (sceneRouteDictionary.TryGetValue(sceneRouteKey, out SceneRoute sceneRoute))
            {
                return sceneRoute;
            }

            return null;
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

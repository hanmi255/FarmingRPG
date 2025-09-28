using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Enums;
using Assets.Scripts.Misc;
using Assets.Scripts.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(GenerateGUID))]
    public class NPCUnit : MonoBehaviour, ISaveable
    {
        #region Fields
        private string _iSaveableUniqueID;  // 保存ID
        private GameObjectSave _gameObjectSave;  // 游戏对象保存

        public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

        public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

        private NPCMovement _npcMovement;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _iSaveableUniqueID = GetComponent<GenerateGUID>().GUID;
            _gameObjectSave = new GameObjectSave();
        }

        private void Start()
        {
            _npcMovement = GetComponent<NPCMovement>();
        }

        private void OnEnable()
        {
            ISaveableRegister();
        }

        private void OnDisable()
        {
            ISaveableDeregister();
        }
        #endregion

        #region ISaveable Interface Methods
        /// <summary>
        /// 注册可保存对象到保存管理器
        /// </summary>
        public void ISaveableRegister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Add(this);
        }

        /// <summary>
        /// 从保存管理器中注销可保存对象
        /// </summary>
        public void ISaveableDeregister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <returns>游戏对象保存数据</returns>
        public GameObjectSave ISaveableSave()
        {
            _gameObjectSave.sceneData.Remove(Settings.PersistentSceneName);

            SceneSave sceneSave = new()
            {
                vector3Dictionary = new Dictionary<string, Vector3Serializable>()
                {
                    { "npcTargetGridPosition", new Vector3Serializable(_npcMovement.npcTargetGridPosition.x, _npcMovement.npcTargetGridPosition.y, _npcMovement.npcTargetGridPosition.z) },
                    { "npcTargetWorldPosition", new Vector3Serializable(_npcMovement.npcTargetWorldPosition.x, _npcMovement.npcTargetWorldPosition.y, _npcMovement.npcTargetWorldPosition.z) }
                },
                stringDictionary = new Dictionary<string, string>()
                {
                    { "npcTargetScene", _npcMovement.npcTargetScene.ToString() },
                }
            };

            _gameObjectSave.sceneData.Add(Settings.PersistentSceneName, sceneSave);

            return _gameObjectSave;
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <param name="gameSave">游戏保存数据</param>
        public void ISaveableLoad(GameSave gameSave)
        {
            if (!gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out var gameObjectSave))
                return;

            _gameObjectSave = gameObjectSave;

            if (!_gameObjectSave.sceneData.TryGetValue(Settings.PersistentSceneName, out var sceneSave))
                return;

            if (sceneSave.vector3Dictionary == null || sceneSave.stringDictionary == null)
                return;

            if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition", out Vector3Serializable savedNPCTargetGridPosition))
            {
                _npcMovement.npcTargetGridPosition = new Vector3Int((int)savedNPCTargetGridPosition.x, (int)savedNPCTargetGridPosition.y, (int)savedNPCTargetGridPosition.z);
                _npcMovement.npcCurrentGridPosition = _npcMovement.npcTargetGridPosition;
            }

            if (sceneSave.vector3Dictionary.TryGetValue("npcTargetWorldPosition", out Vector3Serializable savedNPCTargetWorldPosition))
            {
                _npcMovement.npcTargetWorldPosition = new Vector3((int)savedNPCTargetWorldPosition.x, (int)savedNPCTargetWorldPosition.y, (int)savedNPCTargetWorldPosition.z);
                transform.position = _npcMovement.npcTargetWorldPosition;
            }

            if (sceneSave.stringDictionary.TryGetValue("npcTargetScene", out string savedNPCTargetScene))
            { 
                _npcMovement.npcTargetScene = (SceneName)Enum.Parse(typeof(SceneName), savedNPCTargetScene);
                _npcMovement.npcCurrentScene = _npcMovement.npcTargetScene;
            }

            _npcMovement.CancelNPCMovement();
        }

        public void ISaveableStoreScene(string sceneName)
        {
            // Nothing to store
        }

        public void ISaveableRestoreScene(string sceneName)
        {
            // Nothing to restore
        }
        #endregion
    }
}

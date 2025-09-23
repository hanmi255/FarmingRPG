using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.Misc;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.SaveSystem
{
    public class SaveLoadManager : SingletonMonoBehaviour<SaveLoadManager>
    {
        private const string SAVE_FILE_NAME = "WildHopeCreek.dat";
        private const string SAVE_FILE_NOT_FOUND_MESSAGE = "Save file not found";

        public GameSave gameSave;
        public List<ISaveable> iSaveableObjectList;

        protected override void Awake()
        {
            base.Awake();

            iSaveableObjectList = new List<ISaveable>();
        }

        #region Public Methods

        /// <summary>
        /// 保存游戏数据到文件
        /// </summary>
        public void SaveDataToFile()
        {
            gameSave = new GameSave();

            foreach (ISaveable iSaveableObject in iSaveableObjectList)
            {
                gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.ISaveableSave());
            }

            SerializeDataToFile(GetSaveFilePath());
            UIManager.Instance.DisablePauseMenu();
        }

        /// <summary>
        /// 从文件加载游戏数据
        /// </summary>
        public void LoadDataFromFile()
        {
            string saveFilePath = GetSaveFilePath();
            if (!File.Exists(saveFilePath))
            {
                Debug.Log(SAVE_FILE_NOT_FOUND_MESSAGE);
                return;
            }

            DeserializeDataFromFile(saveFilePath);
            ProcessSaveableObjectList();
            UIManager.Instance.DisablePauseMenu();
        }

        public void StoreCurrentSceneData()
        {
            foreach (ISaveable iSaveableObject in iSaveableObjectList)
            {
                iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
            }
        }

        public void RestoreCurrentSceneData()
        {
            foreach (ISaveable iSaveableObject in iSaveableObjectList)
            {
                iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 获取保存文件路径
        /// </summary>
        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        /// <summary>
        /// 序列化数据
        /// </summary>
        private void SerializeDataToFile(string filePath)
        {
            BinaryFormatter binaryFormatter = new();

            FileStream fileStream = File.Open(filePath, FileMode.Create);
            binaryFormatter.Serialize(fileStream, gameSave);
            fileStream.Close();
        }

        /// <summary>
        /// 反序列化数据
        /// </summary>
        private void DeserializeDataFromFile(string filePath)
        {
            BinaryFormatter binaryFormatter = new();

            FileStream fileStream = File.Open(filePath, FileMode.Open);
            gameSave = binaryFormatter.Deserialize(fileStream) as GameSave;
            fileStream.Close();
        }

        /// <summary>
        /// 处理可保存对象列表
        /// </summary>
        private void ProcessSaveableObjectList()
        {
            if (gameSave?.gameObjectData == null)
                return;

            for (int i = iSaveableObjectList.Count - 1; i >= 0; i--)
            {
                ISaveable saveableObject = iSaveableObjectList[i];
                
                if (saveableObject == null)
                {
                    iSaveableObjectList.RemoveAt(i);
                    continue;
                }

                string uniqueID = saveableObject.ISaveableUniqueID;
                if (string.IsNullOrEmpty(uniqueID))
                {
                    continue;
                }

                if (gameSave.gameObjectData.ContainsKey(uniqueID))
                {
                    saveableObject.ISaveableLoad(gameSave);
                }
                else
                {
                    RemoveSaveableObject(saveableObject, i);
                }
            }
        }

        /// <summary>
        /// 移除并销毁可保存对象
        /// </summary>
        private void RemoveSaveableObject(ISaveable saveableObject, int index)
        {
            Component component = saveableObject as Component;
            if (component != null)
            {
                Destroy(component.gameObject);
            }

            iSaveableObjectList.RemoveAt(index);
        }
        #endregion
    }
}
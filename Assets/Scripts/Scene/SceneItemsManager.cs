using System.Collections.Generic;
using Assets.Scripts.Events;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using Assets.Scripts.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.Scene
{
    [RequireComponent(typeof(GenerateGUID))]
    public class SceneItemsManager : SingletonMonoBehaviour<SceneItemsManager>, ISaveble
    {
        #region Fields

        [SerializeField] private GameObject _itemPrefab = null;
        private Transform _parentItem;

        private string _iSaveableUniqueID;
        public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

        private GameObjectSave _gameObjectSave;
        public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            base.Awake();

            ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
            GameObjectSave = new GameObjectSave();
        }

        private void OnEnable()
        {
            ISaveableRegister();
            EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        }

        private void OnDisable()
        {
            ISaveableDeregister();
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 注册可保存对象到保存管理器中
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
        /// 存储场景中的物品数据到保存系统中
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void ISaveableStoreScene(string sceneName)
        {
            // 移除场景中已存在的数据，避免重复存储
            GameObjectSave.sceneData.Remove(sceneName);

            // 创建用于存储场景物品的列表
            List<SceneItem> sceneItems = new();
            
            // 查找场景中所有的物品单位
            ItemUnit[] itemUnitsInScene = FindObjectsOfType<ItemUnit>();

            // 遍历所有物品单位，将其信息保存到SceneItem列表中
            foreach (ItemUnit item in itemUnitsInScene)
            {
                SceneItem sceneItem = new()
                {
                    itemCode = item.ItemCode,
                    itemName = item.name,
                    position = new Vector3Serializable(item.transform.position.x, item.transform.position.y, item.transform.position.z)
                };

                sceneItems.Add(sceneItem);
            }

            // 创建场景保存数据对象
            SceneSave sceneSave = new()
            {
                listSceneItem = sceneItems
            };

            // 将场景数据添加到游戏对象保存数据中
            GameObjectSave.sceneData.Add(sceneName, sceneSave);
        }

        /// <summary>
        /// 恢复场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void ISaveableRestoreScene(string sceneName)
        {
            if (!GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave)) 
                return;
            
            if (sceneSave.listSceneItem == null)
                return;

            DestroySceneItems();
            InstantiateSceneItems(sceneSave.listSceneItem);
        }

        /// <summary>
        /// 实例化指定场景物品
        /// </summary>
        /// <param name="itemCode">物品代码</param>
        /// <param name="position">物品位置</param>
        public void InstantiateSceneItem(int itemCode, Vector3 position)
        {
            GameObject itemGameObject = Instantiate(_itemPrefab, position, Quaternion.identity, _parentItem);
            ItemUnit itemUnit = itemGameObject.GetComponent<ItemUnit>();
            itemUnit.Init(itemCode);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 场景加载完成后获取物品父级变换组件
        /// </summary>
        private void AfterSceneLoad()
        {
            _parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
        }

        /// <summary>
        /// 销毁所有场景物品
        /// </summary>
        private void DestroySceneItems()
        {
            ItemUnit[] itemUnitsInScene = FindObjectsOfType<ItemUnit>();
            foreach (ItemUnit item in itemUnitsInScene)
            {
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// 实例化所有场景物品
        /// </summary>
        /// <param name="sceneItems">场景物品列表</param>
        private void InstantiateSceneItems(List<SceneItem> sceneItems)
        {
            foreach (SceneItem sceneItem in sceneItems)
            {
                GameObject itemGameObject = Instantiate(_itemPrefab, 
                    new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), 
                    Quaternion.identity, _parentItem);

                ItemUnit itemUnit = itemGameObject.GetComponent<ItemUnit>();
                itemUnit.ItemCode = sceneItem.itemCode;
                itemUnit.name = sceneItem.itemName;
            }
        }

        #endregion
    }
}
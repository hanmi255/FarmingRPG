using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.Scene;
using UnityEngine;

namespace Assets.Scripts.Map
{
    [RequireComponent(typeof(GenerateGUID))]
    public class GridPropertyManager : SingletonMonoBehaviour<GridPropertyManager>, ISaveble
    {
        [HideInInspector] public Grid grid;
        private Dictionary<string, GridPropertyDetails> _gridPropertyDetailsDictionary;
        [SerializeField] private SO_GridProperties[] _so_gridPropertiesArray = null;

        private string _iSaveableUniqueID;
        public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

        private GameObjectSave _gameObjectSave;
        public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

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

        private void Start()
        {
            InitialiseGridProperties();
        }

        public void ISaveableRegister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Add(this);
        }
        public void ISaveableDeregister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
        }

        public void ISaveableStoreScene(string sceneName)
        {
            GameObjectSave.sceneData.Remove(sceneName);

            SceneSave sceneSave = new()
            {
                gridPropertyDetailsDictionary = _gridPropertyDetailsDictionary
            };

            GameObjectSave.sceneData.Add(sceneName, sceneSave);
        }

        public void ISaveableRestoreScene(string sceneName)
        {
            if (!GameObjectSave.sceneData.TryGetValue(sceneName, out var sceneSave))
                return;

            if (sceneSave.gridPropertyDetailsDictionary == null)
                return;

            _gridPropertyDetailsDictionary = sceneSave.gridPropertyDetailsDictionary;
        }

        private void AfterSceneLoad()
        {
            grid = FindObjectOfType<Grid>();
        }

        /// <summary>
        /// 初始化网格属性字典，将SO_GridProperties中的数据转换为可保存的字典格式
        /// </summary>
        private void InitialiseGridProperties()
        {
            // 遍历所有网格属性配置
            foreach (var so_gridProperties in _so_gridPropertiesArray)
            {
                // 为当前场景创建网格属性字典
                Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary = new();

                // 遍历当前配置中的所有网格属性
                foreach (var gridProperty in so_gridProperties.gridPropertyList)
                {
                    // 获取指定坐标的网格属性详情，如果不存在则创建新的
                    GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(
                        gridProperty.gridCoordinate.x, 
                        gridProperty.gridCoordinate.y, 
                        gridPropertyDetailsDictionary) ?? new GridPropertyDetails();

                    // 根据属性类型设置相应的布尔值
                    switch (gridProperty.gridBoolProperty)
                    {
                        case GridBoolProperty.Diggable:
                            gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                            break;
                        case GridBoolProperty.CanDropItem:
                            gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                            break;
                        case GridBoolProperty.CanPlaceFurniture:
                            gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                            break;
                        case GridBoolProperty.IsPath:
                            gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                            break;
                        case GridBoolProperty.IsNPCObstacle:
                            gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                            break;
                        default:
                            break;
                    }

                    // 更新网格坐标信息并保存到字典中
                    SetGridPropertyDetails(
                        gridProperty.gridCoordinate.x, 
                        gridProperty.gridCoordinate.y, 
                        gridPropertyDetails, 
                        gridPropertyDetailsDictionary);
                }

                // 创建场景保存数据
                SceneSave sceneSave = new()
                {
                    gridPropertyDetailsDictionary = gridPropertyDetailsDictionary
                };

                // 如果是起始场景，则设置当前场景的网格属性字典
                if (so_gridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingScene.ToString())
                {
                    _gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;
                }

                // 将场景数据添加到游戏对象保存数据中
                GameObjectSave.sceneData.Add(so_gridProperties.sceneName.ToString(), sceneSave);
            }
        }

        public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary)
        {
            string key = "x" + gridX + "y" + gridY;

            if (gridPropertyDetailsDictionary.TryGetValue(key, out GridPropertyDetails gridPropertyDetails))
            {
                return gridPropertyDetails;
            }
            else
            {
                return null;
            }
        }

        public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
        {
            return GetGridPropertyDetails(gridX, gridY, _gridPropertyDetailsDictionary);
        }

        public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary)
        {
            string key = "x" + gridX + "y" + gridY;

            gridPropertyDetails.gridX = gridX;
            gridPropertyDetails.gridY = gridY;

            gridPropertyDetailsDictionary[key] = gridPropertyDetails;
        }

        public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
        {
            SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, _gridPropertyDetailsDictionary);
        }
    }
}
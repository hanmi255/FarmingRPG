using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.Scene;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Map
{
    /// <summary>
    /// 网格属性管理器，负责管理游戏场景中网格的各种属性，如可挖掘、可放置家具等
    /// </summary>
    [RequireComponent(typeof(GenerateGUID))]
    public class GridPropertyManager : SingletonMonoBehaviour<GridPropertyManager>, ISaveble
    {
        #region Fields

        private Grid _grid;
        private Tilemap _groundDecoration1;
        private Tilemap _groundDecoration2;

        // 创建位掩码查找表，索引是位掩码值，值是Ground数组的索引
        private static readonly int[] TileMaskToIndex = new int[]
        {
            0,  // 0000: 没有使用
            13, // 0001: 只有右边使用
            15, // 0010: 只有左边使用
            14, // 0011: 左右使用
            4,  // 0100: 只有下面使用
            1,  // 0101: 下面和右边使用
            3,  // 0110: 下面和左边使用
            2,  // 0111: 下面、左边、右边使用
            12, // 1000: 只有上面使用
            9,  // 1001: 上面和右边使用
            11, // 1010: 上面和左边使用
            10, // 1011: 上面、左边、右边使用
            8,  // 1100: 上下使用
            5,  // 1101: 上下右使用
            7,  // 1110: 上下左使用
            6   // 1111: 四个方向都使用
        };

        /// <summary>
        /// 网格属性详情字典，用于存储场景中各个网格的属性信息
        /// </summary>
        private Dictionary<string, GridPropertyDetails> _gridPropertyDetailsDictionary;

        /// <summary>
        /// 网格属性配置数组，包含各个场景的网格属性配置
        /// </summary>
        [SerializeField] private SO_GridProperties[] _so_gridPropertiesArray = null;

        [SerializeField] private Tile[] _dugGround = null;
        [SerializeField] private Tile[] _wateredGround = null;

        /// <summary>
        /// 可保存对象的唯一ID
        /// </summary>
        private string _ISaveableUniqueID;

        /// <summary>
        /// 游戏对象保存数据
        /// </summary>
        private GameObjectSave _gameObjectSave;

        #endregion

        #region Properties

        /// <summary>
        /// ISaveable接口的唯一ID属性
        /// </summary>
        public string ISaveableUniqueID
        {
            get => _ISaveableUniqueID;
            set => _ISaveableUniqueID = value;
        }

        /// <summary>
        /// ISaveable接口的游戏对象保存数据属性
        /// </summary>
        public GameObjectSave GameObjectSave
        {
            get => _gameObjectSave;
            set => _gameObjectSave = value;
        }

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
            EventHandler.AdvanceGameDayEvent += AdvanceGameDay;
        }

        private void OnDisable()
        {
            ISaveableDeregister();
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
            EventHandler.AdvanceGameDayEvent -= AdvanceGameDay;
        }

        private void Start()
        {
            InitialiseGridProperties();
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
        /// 存储场景数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void ISaveableStoreScene(string sceneName)
        {
            GameObjectSave.sceneData.Remove(sceneName);

            SceneSave sceneSave = new()
            {
                gridPropertyDetailsDictionary = _gridPropertyDetailsDictionary
            };

            GameObjectSave.sceneData.Add(sceneName, sceneSave);
        }

        /// <summary>
        /// 恢复场景数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void ISaveableRestoreScene(string sceneName)
        {
            if (!GameObjectSave.sceneData.TryGetValue(sceneName, out var sceneSave))
                return;

            if (sceneSave.gridPropertyDetailsDictionary == null)
                return;

            _gridPropertyDetailsDictionary = sceneSave.gridPropertyDetailsDictionary;

            if (_gridPropertyDetailsDictionary.Count > 0)
            {
                ClearDisplayGridPropertyDetails();

                DisplayGridPropertyDetails();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 场景加载完成后的处理函数
        /// </summary>
        private void AfterSceneLoad()
        {
            _grid = FindObjectOfType<Grid>();

            _groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
            _groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
        }

        /// <summary>
        /// 游戏天数增加后的处理函数
        /// </summary>
        private void AdvanceGameDay(TimeEventParameters parameters)
        {
            ClearDisplayGridPropertyDetails();

            foreach (var so_GridProperties in _so_gridPropertiesArray)
            {
                if (!GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out var sceneSave))
                    continue;

                if (sceneSave.gridPropertyDetailsDictionary == null)
                    continue;

                foreach (var item in sceneSave.gridPropertyDetailsDictionary)
                {
                    var gridPropertyDetails = item.Value;

                    if (gridPropertyDetails.daysSinceLastWater > -1)
                    {
                        gridPropertyDetails.daysSinceLastWater = -1;
                    }
                }
            }

            DisplayGridPropertyDetails();
        }

        #endregion

        #region Grid Property Management Methods

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

        /// <summary>
        /// 获取指定坐标的网格属性详情（从指定字典中获取）
        /// </summary>
        /// <param name="gridX">网格X坐标</param>
        /// <param name="gridY">网格Y坐标</param>
        /// <param name="gridPropertyDetailsDictionary">网格属性详情字典</param>
        /// <returns>网格属性详情对象，如果不存在则返回null</returns>
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

        /// <summary>
        /// 获取指定坐标的网格属性详情（从当前场景字典中获取）
        /// </summary>
        /// <param name="gridX">网格X坐标</param>
        /// <param name="gridY">网格Y坐标</param>
        /// <returns>网格属性详情对象</returns>
        public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
        {
            return GetGridPropertyDetails(gridX, gridY, _gridPropertyDetailsDictionary);
        }

        /// <summary>
        /// 设置指定坐标的网格属性详情（到指定字典中设置）
        /// </summary>
        /// <param name="gridX">网格X坐标</param>
        /// <param name="gridY">网格Y坐标</param>
        /// <param name="gridPropertyDetails">网格属性详情对象</param>
        /// <param name="gridPropertyDetailsDictionary">网格属性详情字典</param>
        public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary)
        {
            string key = "x" + gridX + "y" + gridY;

            gridPropertyDetails.gridX = gridX;
            gridPropertyDetails.gridY = gridY;

            gridPropertyDetailsDictionary[key] = gridPropertyDetails;
        }

        /// <summary>
        /// 设置指定坐标的网格属性详情（到当前场景字典中设置）
        /// </summary>
        /// <param name="gridX">网格X坐标</param>
        /// <param name="gridY">网格Y坐标</param>
        /// <param name="gridPropertyDetails">网格属性详情对象</param>
        public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
        {
            SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, _gridPropertyDetailsDictionary);
        }

        #endregion

        #region Ground Decoration Management

        /// <summary>
        /// 显示已 dug 的地面装饰
        /// </summary>
        /// <param name="gridPropertyDetails"></param>
        public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
        {
            if (gridPropertyDetails.daysSinceLastDig > -1)
            {
                ConnectGround(GridPropertyDetailsType.Diggable, gridPropertyDetails);
            }
        }

        /// <summary>
        /// 显示已 water 的地面装饰
        /// <param name="gridPropertyDetails"></param>
        /// </summary>
        public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
        {
            if (gridPropertyDetails.daysSinceLastWater > -1)
            {
                ConnectGround(GridPropertyDetailsType.Waterable, gridPropertyDetails);
            }
        }

        private void DisplayGridPropertyDetails()
        {
            foreach (var item in _gridPropertyDetailsDictionary)
            {
                GridPropertyDetails gridPropertyDetails = item.Value;

                DisplayDugGround(gridPropertyDetails);

                DisplayWateredGround(gridPropertyDetails);
            }
        }

        /// <summary>
        /// 清除显示的装饰
        /// </summary>
        private void ClearDisplayGroundDecorations()
        {
            _groundDecoration1.ClearAllTiles();
            _groundDecoration2.ClearAllTiles();
        }

        /// <summary>
        /// 清除显示的属性
        /// </summary>
        private void ClearDisplayGridPropertyDetails()
        {
            ClearDisplayGroundDecorations();
        }

        /// <summary>
        /// 连接地面装饰（通用方法处理挖掘和浇水）
        /// </summary>
        /// <param name="type">地面类型（挖掘或浇水）</param>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        private void ConnectGround(GridPropertyDetailsType type, GridPropertyDetails gridPropertyDetails)
        {
            // 设置当前格子的贴图
            Tile tile0 = SetGroundTile(type, gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            Tilemap tilemap = type == GridPropertyDetailsType.Diggable ? _groundDecoration1 : _groundDecoration2;
            tilemap.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), tile0);

            // 定义四个相邻方向的偏移量：上、下、左、右
            Vector2Int[] adjacentOffsets = {
                new(0, 1),   // 上
                new(0, -1),  // 下
                new(-1, 0),  // 左
                new(1, 0)    // 右
            };

            // 遍历四个相邻方向，检查是否需要更新贴图
            foreach (Vector2Int offset in adjacentOffsets)
            {
                int adjacentX = gridPropertyDetails.gridX + offset.x;
                int adjacentY = gridPropertyDetails.gridY + offset.y;

                GridPropertyDetails adjacentGridPropertyDetails = GetGridPropertyDetails(adjacentX, adjacentY);
                if (adjacentGridPropertyDetails != null && IsGridSquareModified(type, adjacentX, adjacentY))
                {
                    Tile adjacentTile = SetGroundTile(type, adjacentX, adjacentY);
                    tilemap.SetTile(new Vector3Int(adjacentX, adjacentY, 0), adjacentTile);
                }
            }
        }

        /// <summary>
        /// 根据相邻格子的状态设置当前格子的贴图（通用方法处理挖掘和浇水）
        /// </summary>
        /// <param name="type">地面类型（挖掘或浇水）</param>
        /// <param name="gridX">当前格子X坐标</param>
        /// <param name="gridY">当前格子Y坐标</param>
        /// <returns>对应的贴图</returns>
        private Tile SetGroundTile(GridPropertyDetailsType type, int gridX, int gridY)
        {
            bool upModified = IsGridSquareModified(type, gridX, gridY + 1);
            bool downModified = IsGridSquareModified(type, gridX, gridY - 1);
            bool leftModified = IsGridSquareModified(type, gridX - 1, gridY);
            bool rightModified = IsGridSquareModified(type, gridX + 1, gridY);

            // 构建位掩码：右(bit0) + 左(bit1) + 下(bit2) + 上(bit3)
            int mask = (rightModified ? 1 : 0) |
                       (leftModified ? 2 : 0) |
                       (downModified ? 4 : 0) |
                       (upModified ? 8 : 0);

            Tile[] groundTiles = type == GridPropertyDetailsType.Diggable ? _dugGround : _wateredGround;
            return groundTiles[TileMaskToIndex[mask]];
        }

        /// <summary>
        /// 检查指定坐标的格子是否已被修改（通用方法处理挖掘和浇水）
        /// </summary>
        /// <param name="type">地面类型（挖掘或浇水）</param>
        /// <param name="gridX">格子X坐标</param>
        /// <param name="gridY">格子Y坐标</param>
        /// <returns>如果已被修改返回true，否则返回false</returns>
        private bool IsGridSquareModified(GridPropertyDetailsType type, int gridX, int gridY)
        {
            GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(gridX, gridY);
            if (gridPropertyDetails == null) return false;

            return type switch
            {
                GridPropertyDetailsType.Diggable => gridPropertyDetails.daysSinceLastDig > -1,
                GridPropertyDetailsType.Waterable => gridPropertyDetails.daysSinceLastWater > -1,
                _ => false
            };
        }

        #endregion
    }
}
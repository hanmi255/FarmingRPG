using System.Collections.Generic;
using Assets.Scripts.Inventory;
using Assets.Scripts.Map;
using Assets.Scripts.Misc;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class SceneSave
    {
        public List<SceneItem> listSceneItem;  // 用于保存场景物品
        public List<InventoryItem>[] listInventoryItems;  // 用于保存物品
        public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;  // 用于保存网格属性
        public Dictionary<string, bool> boolDictionary;  // 布尔值字典 用于保存场景是否第一次加载
        public Dictionary<string, string> stringDictionary;  // 字符串字典 用于保存玩家数据和时间数据
        public Dictionary<string, int> intDictionary;  // 整数字典 用于保存时间数据
        public Dictionary<string, int[]> intArrayDictionary;  // 整数数组字典 用于保存库存容量
        public Dictionary<string, Vector3Serializable> vector3Dictionary;  // Vector3字典 用于保存玩家位置方向
    }
}
using System.Collections.Generic;
using Assets.Scripts.Inventory;
using Assets.Scripts.Map;
using Assets.Scripts.Misc;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class SceneSave
    {
        public List<SceneItem> listSceneItem;
        public List<InventoryItem>[] listInventoryItems;
        public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
        public Dictionary<string, bool> boolDictionary;
        public Dictionary<string, string> stringDictionary;
        public Dictionary<string, int> intDictionary;
        public Dictionary<string, int[]> intArrayDictionary;
        public Dictionary<string, Vector3Serializable> vector3Dictionary;
    }
}
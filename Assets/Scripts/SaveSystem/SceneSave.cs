using System.Collections.Generic;
using Assets.Scripts.Map;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class SceneSave
    {
        public List<SceneItem> listSceneItem;
        public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
    }
}
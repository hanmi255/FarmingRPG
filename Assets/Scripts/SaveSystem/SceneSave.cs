using System.Collections.Generic;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class SceneSave
    {
        public Dictionary<string, List<SceneItem>> listSceneItemDictionary;
    }
}
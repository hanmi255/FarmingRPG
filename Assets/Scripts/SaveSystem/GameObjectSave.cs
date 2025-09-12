using System.Collections.Generic;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class GameObjectSave
    {
        public Dictionary<string, SceneSave> sceneData;

        public GameObjectSave()
        {
            sceneData = new Dictionary<string, SceneSave>();
        }

        public GameObjectSave(Dictionary<string, SceneSave> sceneData)
        {
            this.sceneData = sceneData;
        }
    }
}
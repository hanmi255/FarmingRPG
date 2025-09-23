using System.Collections.Generic;

namespace Assets.Scripts.SaveSystem
{
    [System.Serializable]
    public class GameSave
    {
        public Dictionary<string, GameObjectSave> gameObjectData;

        public GameSave()
        {
            gameObjectData = new Dictionary<string, GameObjectSave>();
        }
    }
}

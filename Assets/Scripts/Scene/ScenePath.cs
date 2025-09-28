using Assets.Scripts.Enums;
using Assets.Scripts.Map;

namespace Assets.Scripts.Scene
{
    [System.Serializable]
    public class ScenePath
    {
        public SceneName sceneName;
        public GridCoordinate fromGridCell;
        public GridCoordinate toGridCell;
    }
}
using System.Collections.Generic;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Scene
{
    [System.Serializable]
    public class SceneRoute
    {
        public SceneName fromSceneName;
        public SceneName toSceneName;
        public List<ScenePath> scenePathList;
    }
}

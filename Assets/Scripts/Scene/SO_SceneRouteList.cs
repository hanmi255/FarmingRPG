using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Scene
{
    [CreateAssetMenu(fileName = "so_SceneRouteList", menuName = "ScriptableObjects/Scene/SceneRouteList")]
    public class SO_SceneRouteList : ScriptableObject
    {
        public List<SceneRoute> sceneRouteList;
    }
}


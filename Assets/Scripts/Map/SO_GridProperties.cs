using System.Collections.Generic;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Map
{
    [CreateAssetMenu(fileName = "so_GridProperties", menuName = "ScriptableObjects/GridProperties")]
    public class SO_GridProperties : ScriptableObject
    {
        public SceneName sceneName;
        public int gridWidth;
        public int gridHeight;
        public int originX;
        public int originY;

        public List<GridProperty> gridPropertyList;
    }
}
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Lighting
{
    [CreateAssetMenu(fileName = "so_LightingSchedule_", menuName = "ScriptableObjects/LightingSchedule")]
    public class LightingSchedule : ScriptableObject
    {
        public LightingBrightness[] lightingBrightnessArray;
    }

    [System.Serializable]
    public struct LightingBrightness
    {
        public Season season;
        public int hour;
        public float lightIntensity;
    }
}

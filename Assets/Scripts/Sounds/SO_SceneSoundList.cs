using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Sounds
{
    [CreateAssetMenu(fileName = "so_SceneSoundList", menuName = "ScriptableObjects/Sounds/SO_SceneSoundList")]
    public class SO_SceneSoundList : ScriptableObject
    {
        public List<SceneSoundItem> sceneSoundDetails;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Sounds
{
    [CreateAssetMenu(fileName = "so_SoundList", menuName = "ScriptableObjects/Sounds/SO_SoundList")]
    public class SO_SoundList : ScriptableObject
    {
        public List<SoundItem> soundDetails;
    }
}

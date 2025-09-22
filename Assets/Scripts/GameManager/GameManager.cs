using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.GameManager
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();

            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
    }
}

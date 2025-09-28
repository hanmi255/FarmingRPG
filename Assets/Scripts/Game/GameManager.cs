using Assets.Scripts.Enums;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        public Weather currentWeather;

        protected override void Awake()
        {
            base.Awake();

            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

            currentWeather = Weather.Dry;
        }
    }
}

using Assets.Scripts.Enums;

namespace Assets.Scripts.Sounds
{
    [System.Serializable]
    public class SceneSoundItem
    {
        public SceneName sceneName;
        public SoundName ambientSoundForScene;
        public SoundName musicForScene;
    }
}
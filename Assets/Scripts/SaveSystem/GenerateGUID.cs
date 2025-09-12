using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    [ExecuteAlways]
    public class GenerateGUID : MonoBehaviour
    {
        [SerializeField] private string _guid;

        public string GUID { get => _guid; set => _guid = value; }

        private void Awake()
        {
            // 只在编辑模式下生成GUID
            if (Application.IsPlaying(gameObject))
                return;

            // 如果GUID为空，则生成新的GUID
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = System.Guid.NewGuid().ToString();
            }
        }
    }
}
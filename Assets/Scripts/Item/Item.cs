using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts.Item
{
    public class Item : MonoBehaviour
    {
        [SerializeField]
        private int _itemCode;
        private SpriteRenderer _spriteRenderer;

        public int ItemCode { get { return _itemCode; } set { _itemCode = value; } }

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            if (ItemCode != 0)
            {
                Init(ItemCode);
            }
        }

        public void Init(int itemCode)
        {
            
        }
    }
}

using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.UI.UIInventory
{
    [RequireComponent(typeof(RectTransform))]
    public class UIInventoryBar : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private bool _isInventoryBarPositionAtBottom = true;
        public bool IsInventoryBarPositionAtBottom
        {
            get => _isInventoryBarPositionAtBottom;
            set => _isInventoryBarPositionAtBottom = value;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            SwitchInventoryBarPosition();
        }

        private void SwitchInventoryBarPosition()
        {
            Vector3 playerViewportPosition = PlayerUnit.Instance.GetPlayerViewporPosition();
            bool shouldPositionAtBottom = playerViewportPosition.y > 0.3f;

            // 只有当位置需要改变时才执行变换操作
            if (shouldPositionAtBottom && !IsInventoryBarPositionAtBottom)
            {
                SetInventoryBarToBottom();
            }
            else if (!shouldPositionAtBottom && IsInventoryBarPositionAtBottom)
            {
                SetInventoryBarToTop();
            }
        }

        private void SetInventoryBarToBottom()
        {
            _rectTransform.pivot = new Vector2(0.5f, 0f);
            _rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionAtBottom = true;
        }

        private void SetInventoryBarToTop()
        {
            _rectTransform.pivot = new Vector2(0.5f, 1f);
            _rectTransform.anchorMin = new Vector2(0.5f, 1f);
            _rectTransform.anchorMax = new Vector2(0.5f, 1f);
            _rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionAtBottom = false;
        }
    }
}
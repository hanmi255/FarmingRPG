using System;
using Assets.Scripts.Misc;
using Assets.Scripts.Player;
using Assets.Scripts.UI.UIInventory;
using Assets.Scripts.UI.UIPauseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        #region Fields
        private bool _pauseMenuActive = false;
        [SerializeField] private UIInventoryBar _inventoryBar = null;
        [SerializeField] private PauseMenuInventoryManagement _pauseMenuInventoryManagement = null;
        [SerializeField] private GameObject _pauseMenu = null;
        [SerializeField] private GameObject[] _menuTabs = null;
        [SerializeField] private Button[] _menuTabButtons = null;

        public bool PauseMenuActive { get => _pauseMenuActive; set => _pauseMenuActive = value; }

        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            _pauseMenu.SetActive(false);
        }

        private void Update()
        {
            PauseMenu();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 切换暂停菜单标签页
        /// </summary>
        /// <param name="tabNum">标签页索引</param>
        public void SwitchPauseMenuTab(int tabNum)
        {
            // 设置所有标签页的激活状态
            for (int i = 0; i < _menuTabs.Length; i++)
            {
                _menuTabs[i].SetActive(i == tabNum);
            }

            HighlighteButtonForSelectedTab();
        }

        public void DisablePauseMenu()
        {
            _pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

            SetPauseMenuState(false);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
        }
        #endregion

        #region Private Methods

        private void PauseMenu()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            TogglePauseMenu();
        }

        private void TogglePauseMenu()
        {
            if (_pauseMenuActive)
            {
                DisablePauseMenu();
            }
            else
            {
                EnablePauseMenu();
            }
        }

        private void EnablePauseMenu()
        {
            _inventoryBar.DestroyCurrentlyDraggedItems();
            _inventoryBar.ClearCurrentlySelectedItems();

            SetPauseMenuState(true);

            GC.Collect();

            HighlighteButtonForSelectedTab();
        }

        private void SetPauseMenuState(bool isActive)
        {
            _pauseMenuActive = isActive;

            PlayerUnit.Instance.IsInputDisabled = isActive;

            Time.timeScale = isActive ? 0f : 1f;

            if (_pauseMenu != null)
            {
                _pauseMenu.SetActive(isActive);
            }
        }

        private void HighlighteButtonForSelectedTab()
        {
            for (int i = 0; i < _menuTabs.Length; i++)
            {
                if (_menuTabs[i] != null && _menuTabButtons[i] != null)
                {
                    SetButtonColorState(_menuTabButtons[i], _menuTabs[i].activeSelf);
                }
            }
        }

        private void SetButtonColorState(Button button, bool isActive)
        {
            if (button == null)
                return;

            ColorBlock colors = button.colors;
            colors.normalColor = isActive ? colors.pressedColor : colors.disabledColor;
            button.colors = colors;
        }

        #endregion
    }
}
using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.Player;
using Assets.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Scene
{
    /// <summary>
    /// 场景控制器管理器 - 负责场景之间的切换和淡入淡出效果
    /// </summary>
    public class SceneControllerManager : SingletonMonoBehaviour<SceneControllerManager>
    {
        private bool _isFading;
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private CanvasGroup _fadeCanvasGroup = null;
        [SerializeField] private Image _fadeImage = null;
        public SceneName startingScene;

        /// <summary>
        /// 初始化场景 - 在游戏开始时加载起始场景
        /// </summary>
        private IEnumerator Start()
        {
            // 设置初始黑色屏幕覆盖
            _fadeImage.color = new Color(0, 0, 0, 1);
            _fadeCanvasGroup.alpha = 1;

            // 异步加载起始场景并设为激活状态
            yield return StartCoroutine(LoadSceneAndSetActive(startingScene.ToString()));

            // 调用场景加载完成事件
            EventHandler.CallAfterSceneLoadEvent();

            // 恢复新场景数据
            SaveLoadManager.Instance.RestoreCurrentSceneData();

            // 开始淡出效果
            StartCoroutine(Fade(0f));
        }

        /// <summary>
        /// 淡出并加载新场景
        /// </summary>
        public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
        {
            // 如果正在淡入淡出，则不执行新的场景切换
            if (_isFading)
                return;

            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }

        /// <summary>
        /// 淡入淡出并切换场景的核心逻辑
        /// </summary>
        private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
        {
            // 调用场景卸载前淡出事件
            EventHandler.CallBeforeSceneUnloadFadeOutEvent();

            // 淡入（屏幕变黑）
            yield return StartCoroutine(Fade(1f));

            // 存储当前场景数据
            SaveLoadManager.Instance.StoreCurrentSceneData();

            // 设置玩家在新场景中的位置
            PlayerUnit.Instance.gameObject.transform.position = spawnPosition;

            // 调用场景卸载前事件
            EventHandler.CallBeforeSceneUnloadEvent();

            // 卸载当前场景
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            // 加载新场景并设为激活状态
            yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

            // 调用场景加载完成事件
            EventHandler.CallAfterSceneLoadEvent();

            // 恢复新场景数据
            SaveLoadManager.Instance.RestoreCurrentSceneData();

            // 淡出（屏幕变透明）
            yield return StartCoroutine(Fade(0f));

            // 调用场景加载后淡入事件
            EventHandler.CallAfterSceneLoadFadeInEvent();
        }

        /// <summary>
        /// 加载场景并设为激活状态
        /// </summary>
        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            // 异步加载场景（使用Additive模式）
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            // 获取刚刚加载的场景（场景列表中的最后一个）
            var newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            // 将新场景设为激活状态
            SceneManager.SetActiveScene(newlyLoadedScene);
        }

        /// <summary>
        /// 淡入淡出效果实现
        /// </summary>
        private IEnumerator Fade(float finalAlpha)
        {
            // 标记正在进行淡入淡出操作
            _isFading = true;

            // 在淡入淡出过程中阻止射线检测（防止用户交互）
            _fadeCanvasGroup.blocksRaycasts = true;

            // 计算淡入淡出速度
            float fadeSpeed = Mathf.Abs(_fadeCanvasGroup.alpha - finalAlpha) / _fadeDuration;

            // 执行淡入淡出动画，直到达到目标透明度
            while (!Mathf.Approximately(_fadeCanvasGroup.alpha, finalAlpha))
            {
                _fadeCanvasGroup.alpha = Mathf.MoveTowards(_fadeCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            // 淡入淡出操作完成
            _isFading = false;

            // 恢复射线检测（允许用户交互）
            _fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}
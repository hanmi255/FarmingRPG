using System.Collections;
using Assets.Scripts.Misc;
using Assets.Scripts.Events;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.VFX
{
    public class VFXManager : SingletonMonoBehaviour<VFXManager>
    {
        #region Fields
        private WaitForSeconds _twoSeconds;
        [SerializeField] private GameObject _reapingPrefab = null;
        [SerializeField] private GameObject _deciduousLeavesFallingPrefab = null;
        [SerializeField] private GameObject _pineConesFallingPrefab = null;
        [SerializeField] private GameObject _choppingTreeTrunkPrefab = null;
        [SerializeField] private GameObject _breakingStonePrefab = null;
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();
            _twoSeconds = new WaitForSeconds(2.0f);
        }

        private void OnEnable()
        {
            EventHandler.HarvestActionEffectEvent += DisplayHarvestActionEffect;
        }

        private void OnDisable()
        {
            EventHandler.HarvestActionEffectEvent -= DisplayHarvestActionEffect;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 显示收获动作效果
        /// </summary>
        /// <param name="position">效果生成位置</param>
        /// <param name="harvestActionEffect">收获动作效果</param>
        private void DisplayHarvestActionEffect(Vector3 position, HarvestActionEffect harvestActionEffect)
        {
            switch (harvestActionEffect)
            {
                case HarvestActionEffect.Reaping:
                    SpawnAndActivateHarvestEffect(_reapingPrefab, position);
                    break;
                case HarvestActionEffect.DeciduousLeavesFalling:
                    SpawnAndActivateHarvestEffect(_deciduousLeavesFallingPrefab, position);
                    break;
                case HarvestActionEffect.PineConesFalling:
                    SpawnAndActivateHarvestEffect(_pineConesFallingPrefab, position);
                    break;
                case HarvestActionEffect.ChoppingTreeTrunk:
                    SpawnAndActivateHarvestEffect(_choppingTreeTrunkPrefab, position);
                    break;
                case HarvestActionEffect.BreakingStone:
                    SpawnAndActivateHarvestEffect(_breakingStonePrefab, position);
                    break;
                case HarvestActionEffect.None:
                default:
                    break;
            }
        }

        /// <summary>
        /// 生成并激活收获效果
        /// </summary>
        /// <param name="effectPrefab">效果预制体</param>
        /// <param name="position">生成位置</param>
        private void SpawnAndActivateHarvestEffect(GameObject effectPrefab, Vector3 position)
        {
            if (effectPrefab == null) return;

            GameObject effect = PoolManager.Instance.ReuseObject(effectPrefab, position, Quaternion.identity);
            effect.SetActive(true);
            StartCoroutine(DisplayHarvestActionEffectCoroutine(effect, _twoSeconds));
        }

        private IEnumerator DisplayHarvestActionEffectCoroutine(GameObject effect, WaitForSeconds twoSeconds)
        {
            yield return twoSeconds;
            effect.SetActive(false);
        }
        #endregion
    }
}
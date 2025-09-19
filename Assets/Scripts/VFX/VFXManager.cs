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
        private void DisplayHarvestActionEffect(Vector3 position, HarvestActionEffect harvestActionEffect)
        {
            switch (harvestActionEffect)
            {
                case HarvestActionEffect.Reaping:
                    GameObject reaping = PoolManager.Instance.ReuseObject(_reapingPrefab, position, Quaternion.identity);
                    reaping.SetActive(true);
                    StartCoroutine(DisplayHarvestActionEffectCoroutine(reaping, _twoSeconds));
                    break;
                case HarvestActionEffect.DeciduousLeavesFalling:
                    GameObject deciduousLeavesFalling = PoolManager.Instance.ReuseObject(_deciduousLeavesFallingPrefab, position, Quaternion.identity);
                    deciduousLeavesFalling.SetActive(true);
                    StartCoroutine(DisplayHarvestActionEffectCoroutine(deciduousLeavesFalling, _twoSeconds));
                    break;
                case HarvestActionEffect.None:
                default:
                    break;
            }
        }

        private IEnumerator DisplayHarvestActionEffectCoroutine(GameObject effect, WaitForSeconds twoSeconds)
        {
            yield return twoSeconds;
            effect.SetActive(false);
        }
        #endregion
    }
}
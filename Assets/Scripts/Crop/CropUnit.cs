using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Map;
using Assets.Scripts.Scene;
using UnityEngine;

namespace Assets.Scripts.Crop
{
    public class CropUnit : MonoBehaviour
    {
        #region Fields
        private int _harvestActionCount = 0;
        [SerializeField] private SpriteRenderer _cropHarvestedSpriteRenderer = null;
        [HideInInspector] public Vector2Int cropGridPosition;
        private static readonly int _harvestedStateHash = Animator.StringToHash("Harvested");
        private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
        #endregion

        #region Public Methods

        /// <summary>
        /// 处理工具对作物的操作
        /// </summary>
        /// <param name="itemDetails">使用的工具物品详情</param>
        public void ProcessToolAction(ItemDetails itemDetails, bool isToolRight, bool isToolLeft, bool isToolUp, bool isToolDown)
        {
            GridPropertyDetails gridPropertyDetails = GridPropertyManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

            if (gridPropertyDetails == null)
                return;

            ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
            if (seedItemDetails == null)
                return;

            CropDetails cropDetails = GridPropertyManager.Instance.GetCropDetails(seedItemDetails.itemCode);
            if (cropDetails == null)
                return;

            Animator animator = GetComponentInChildren<Animator>();
            if (animator == null)
                return;

            if (isToolRight || isToolUp)
            {
                animator.SetTrigger("usetoolright");
            }
            else if (isToolLeft || isToolDown)
            {
                animator.SetTrigger("usetoolleft");
            }

            int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(itemDetails.itemCode);
            if (requiredHarvestActions == -1)
                return;

            _harvestActionCount += 1;
            if (_harvestActionCount >= requiredHarvestActions)
                HarvestCrop(isToolRight, isToolUp, cropDetails, gridPropertyDetails, animator);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 收获作物并更新网格属性
        /// </summary>
        /// <param name="cropDetails">作物详情</param>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        private void HarvestCrop(bool isUsingToolRight, bool isUsingToolUp, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
        {
            // 触发收获动画
            if (cropDetails.isHarvestedAnimation && animator != null)
            {
                if (cropDetails.harvestedSprite != null && _cropHarvestedSpriteRenderer != null)
                {
                    _cropHarvestedSpriteRenderer.sprite = cropDetails.harvestedSprite;
                }

                string triggerName = (isUsingToolRight || isUsingToolUp) ? "harvestright" : "harvestleft";
                animator.SetTrigger(triggerName);
            }

            // 重置网格属性
            ResetGridProperties(gridPropertyDetails);

            if (cropDetails.hideCropBeforeHarvestedAnimation)
            {
                GetComponentInChildren<SpriteRenderer>().enabled = false;
            }

            GridPropertyManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

            if (cropDetails.isHarvestedAnimation && animator != null)
            {
                StartCoroutine(ProcessHarvestActionsAfterAnimation(cropDetails, gridPropertyDetails, animator));
            }
            else
            {
                HarvestActions(cropDetails, gridPropertyDetails);
            }
        }

        private void ResetGridProperties(GridPropertyDetails gridPropertyDetails)
        {
            gridPropertyDetails.seedItemCode = -1;
            gridPropertyDetails.growthDays = -1;
            gridPropertyDetails.DaysSinceLastHarvest = -1;
            gridPropertyDetails.daysSinceLastWater = -1;
        }

        private IEnumerator ProcessHarvestActionsAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
        {
            while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _harvestedStateHash)
            {
                yield return _waitForSeconds0_1; 
            }

            HarvestActions(cropDetails, gridPropertyDetails);
        }

        /// <summary>
        /// 执行收获操作
        /// </summary>
        /// <param name="cropDetails">作物详情</param>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
        {
            SpawnHarvestedItems(cropDetails);

            Destroy(gameObject);
        }

        /// <summary>
        /// 生成收获的物品
        /// </summary>
        /// <param name="cropDetails">作物详情</param>
        private void SpawnHarvestedItems(CropDetails cropDetails)
        {
            // 遍历所有可生产的物品
            for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
            {
                // 计算要生产的物品数量
                int cropsToProduce = cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                                     cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i] ?
                                     cropDetails.cropProducedMinQuantity[i] :
                                     Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);

                // 生成相应数量的物品
                for (int j = 0; j < cropsToProduce; j++)
                {
                    if (cropDetails.spawnCropProducedAtPlayerPosition)
                    {
                        // 在玩家位置添加物品到背包
                        InventoryManager.Instance.AddItem(InventoryLocation.Player, cropDetails.cropProducedItemCode[i]);
                    }
                    else
                    {
                        // 在场景中随机位置生成物品
                        Vector3 spawnPosition = new(transform.position.x + Random.Range(-1f, 1f),
                                                    transform.position.y + Random.Range(-1f, 1f),
                                                    0f);
                        SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);
                    }
                }
            }
        }
        #endregion
    }
}
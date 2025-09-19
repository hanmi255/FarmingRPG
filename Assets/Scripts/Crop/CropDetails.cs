using System;
using Assets.Scripts.Enums;
using Assets.Scripts.Utilities.PropertyDrawers;
using UnityEngine;

namespace Assets.Scripts.Crop
{
    [System.Serializable]
    public class CropDetails
    {
        #region Fields

        [ItemCodeDescription]
        public int seedItemCode; // 种子物品代码
        public int[] growthDays; // 每个生长阶段所需的天数
        public GameObject[] growthPrefabs; // 每个生长阶段的预制体
        public Sprite[] growthSprites; // 每个生长阶段的精灵图
        public Season[] seasons; // 作物可生长的季节
        public Sprite harvestedSprite; // 收获后的精灵图

        [ItemCodeDescription]
        public int harvestedTransformItemCode; // 收获后转换成的物品代码
        public bool hideCropBeforeHarvestedAnimation; // 在收获动画前是否隐藏作物
        public bool disableCropCollidersBeforeHarvestedAnimation; // 在收获动画前是否禁用作物碰撞器
        public bool isHarvestedAnimation; // 是否有收获动画
        public bool isHarvestActionEffect; // 是否有收获特效
        public bool spawnCropProducedAtPlayerPosition; // 是否在玩家位置生成作物产品
        public HarvestActionEffect harvestActionEffect; // 收获特效

        [ItemCodeDescription]
        public int[] harvestToolItemCode; // 可用于收获的工具物品代码
        public int[] requireHarvestActions; // 收获所需的动作次数

        [ItemCodeDescription]
        public int[] cropProducedItemCode; // 作物产生的物品代码
        public int[] cropProducedMinQuantity; // 作物产生物品的最小数量
        public int[] cropProducedMaxQuantity; // 作物产生物品的最大数量
        public int daysToRegrow; // 重新生长所需的天数（-1表示收获后不重新生长）

        #endregion

        #region Public Methods

        /// <summary>
        /// 检查指定的工具是否可以用于收获此作物
        /// </summary>
        /// <param name="toolItemCode">工具物品代码</param>
        /// <returns>如果工具可以用于收获则返回true，否则返回false</returns>
        public bool CanUseToolToHarvestCrop(int toolItemCode)
        {
            return RequiredHarvestActionsForTool(toolItemCode) != -1;
        }

        /// <summary>
        /// 获取使用指定工具收获此作物所需的动作次数
        /// </summary>
        /// <param name="toolItemCode">工具物品代码</param>
        /// <returns>所需的动作次数，如果工具不适用则返回-1</returns>
        public int RequiredHarvestActionsForTool(int toolItemCode)
        {
            if (harvestToolItemCode == null || requireHarvestActions == null)
                return -1;

            // 查找工具并返回所需动作次数
            for (int i = 0; i < harvestToolItemCode.Length; i++)
            {
                if (i < requireHarvestActions.Length && toolItemCode == harvestToolItemCode[i])
                    return requireHarvestActions[i];
            }

            return -1;
        }

        #endregion
    }
}
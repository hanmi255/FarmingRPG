using Assets.Scripts.Enums;

namespace Assets.Scripts.Animation
{
    /// <summary>
    /// 角色属性结构体，用于定义角色各部分的动画和外观属性
    /// </summary>
    [System.Serializable]
    public struct CharacterAttribute
    {
        #region Fields
        public CharacterPartAnimator characterPartAnimator;
        public PartVariantColour partVariantColour;
        public PartVariantType partVariantType;
        #endregion

        #region Constructors
        /// <summary>
        /// 创建一个新的角色属性实例
        /// </summary>
        /// <param name="characterPartAnimator">角色部位动画器</param>
        /// <param name="partVariantColour">部位变体颜色</param>
        /// <param name="partVariantType">部位变体类型</param>
        public CharacterAttribute(CharacterPartAnimator characterPartAnimator, PartVariantColour partVariantColour, PartVariantType partVariantType)
        {
            this.characterPartAnimator = characterPartAnimator;
            this.partVariantColour = partVariantColour;
            this.partVariantType = partVariantType;
        }
        #endregion
    }
}
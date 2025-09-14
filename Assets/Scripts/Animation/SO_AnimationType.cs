using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    /// <summary>
    /// 动画类型ScriptableObject，用于定义特定动画的相关属性
    /// </summary>
    [CreateAssetMenu(fileName = "so_AnimationType", menuName = "ScriptableObjects/AnimationType")]
    public class SO_AnimationType : ScriptableObject
    {
        #region Fields
        public AnimationClip animationClip;
        public AnimationName animationName;
        public CharacterPartAnimator characterPartAnimator;
        public PartVariantColour partVariantColour;
        public PartVariantType partVariantType;
        #endregion
    }
}
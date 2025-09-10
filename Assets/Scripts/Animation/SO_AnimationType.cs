using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    [CreateAssetMenu(fileName = "so_AnimationType", menuName = "ScriptableObjects/AnimationType")]
    public class SO_AnimationType : ScriptableObject
    {
        public AnimationClip animationClip;
        public AnimationName animationName;
        public CharacterPartAnimator characterPartAnimator;
        public PartVariantColour partVariantColour;
        public PartVariantType partVariantType;
    }
}

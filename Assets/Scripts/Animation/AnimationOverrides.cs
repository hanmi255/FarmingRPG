using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Animation
{
    public class AnimationOverrides : MonoBehaviour
    {
        [SerializeField] private GameObject _character = null;
        [SerializeField] private SO_AnimationType[] so_AnimationTypes = null;

        private Dictionary<AnimationClip, SO_AnimationType> _animationTypeDictionaryByAnimationClip;
        private Dictionary<string, SO_AnimationType> _animationTypeDictionaryByCompositeAttributeKey;
        private Animator[] _animators;

        private void Start()
        {
            if (so_AnimationTypes == null || so_AnimationTypes.Length == 0) return;

            _animationTypeDictionaryByAnimationClip = new Dictionary<AnimationClip, SO_AnimationType>();
            _animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();

            foreach (var type in so_AnimationTypes)
            {
                _animationTypeDictionaryByAnimationClip.Add(type.animationClip, type);

                // 使用下划线分隔符确保键的唯一性
                string compositeAttribute = $"{type.characterPartAnimator}_{type.partVariantColour}_{type.partVariantType}_{type.animationName}";

                _animationTypeDictionaryByCompositeAttributeKey.Add(compositeAttribute, type);
            }

            // 预先获取所有Animator以提高查找效率
            _animators = _character.GetComponentsInChildren<Animator>();
        }

        public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributes)
        {
            foreach (var attribute in characterAttributes)
            {
                ApplyAttributeCustomisation(attribute);
            }
        }

        private void ApplyAttributeCustomisation(CharacterAttribute attribute)
        {
            Animator currentAnimator = FindAnimator(attribute.characterPartAnimator);

            if (currentAnimator == null)
            {
                Debug.LogWarning($"Animator with name {attribute.characterPartAnimator} not found.");
                return;
            }

            AnimatorOverrideController controller = new(currentAnimator.runtimeAnimatorController);
            
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairs = 
                GenerateAnimationOverrides(attribute, controller);

            controller.ApplyOverrides(animsKeyValuePairs);
            currentAnimator.runtimeAnimatorController = controller;
        }

        private Animator FindAnimator(CharacterPartAnimator characterPartAnimator)
        {
            string animationSOAssetName = characterPartAnimator.ToString();

            // 优化Animator查找过程
            foreach (var animator in _animators)
            {
                if (animator.name == animationSOAssetName)
                {
                    return animator;
                }
            }

            return null;
        }

        private List<KeyValuePair<AnimationClip, AnimationClip>> GenerateAnimationOverrides(
            CharacterAttribute attribute, AnimatorOverrideController controller)
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairs = new();
            List<AnimationClip> clips = new(controller.animationClips);

            // 缓存attribute的字符串值以避免重复调用ToString()
            string characterPartAnimatorStr = attribute.characterPartAnimator.ToString();
            string partVariantColourStr = attribute.partVariantColour.ToString();
            string partVariantTypeStr = attribute.partVariantType.ToString();

            foreach (var clip in clips)
            {
                if (_animationTypeDictionaryByAnimationClip.TryGetValue(clip, out SO_AnimationType type))
                {
                    // 使用下划线分隔符保持一致性
                    string compositeAttribute = $"{characterPartAnimatorStr}_{partVariantColourStr}_{partVariantTypeStr}_{type.animationName}";

                    if (_animationTypeDictionaryByCompositeAttributeKey.TryGetValue(compositeAttribute, out SO_AnimationType swapType))
                    {
                        AnimationClip swapClip = swapType.animationClip;
                        animsKeyValuePairs.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, swapClip));
                    }
                }
            }

            return animsKeyValuePairs;
        }
    }
}
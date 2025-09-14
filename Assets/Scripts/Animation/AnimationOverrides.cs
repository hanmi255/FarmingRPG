using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Animation
{
    public class AnimationOverrides : MonoBehaviour
    {
        #region Fields

        #region Serialization Fields
        [SerializeField] private GameObject _character = null;
        [SerializeField] private SO_AnimationType[] so_AnimationTypes = null;
        #endregion

        #region Private Fields
        private Dictionary<AnimationClip, SO_AnimationType> _animationTypeDictionaryByAnimationClip;
        private Dictionary<string, SO_AnimationType> _animationTypeDictionaryByCompositeAttributeKey;
        private Animator[] _animators;
        #endregion

        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 初始化动画类型字典和获取角色的所有Animator组件
        /// </summary>
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
        #endregion

        #region Public Methods
        /// <summary>
        /// 应用角色自定义参数到所有指定的角色属性
        /// </summary>
        /// <param name="characterAttributes">角色属性列表</param>
        public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributes)
        {
            foreach (var attribute in characterAttributes)
            {
                ApplyAttributeCustomisation(attribute);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 应用单个属性的自定义设置
        /// </summary>
        /// <param name="attribute">角色属性</param>
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

        /// <summary>
        /// 根据角色部位查找对应的Animator组件
        /// </summary>
        /// <param name="characterPartAnimator">角色部位动画器枚举</param>
        /// <returns>找到的Animator组件，未找到则返回null</returns>
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

        /// <summary>
        /// 生成动画覆盖键值对列表
        /// </summary>
        /// <param name="attribute">角色属性</param>
        /// <param name="controller">动画控制器</param>
        /// <returns>动画覆盖键值对列表</returns>
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
        #endregion
    }
}
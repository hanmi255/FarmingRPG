using Assets.Scripts.Enums;

namespace Assets.Scripts.Animation
{
    [System.Serializable]
    public struct CharacterAttribute
    {
        public CharacterPartAnimator characterPartAnimator;
        public PartVariantColour partVariantColour;
        public PartVariantType partVariantType;

        public CharacterAttribute(CharacterPartAnimator characterPartAnimator, PartVariantColour partVariantColour, PartVariantType partVariantType)
        {
            this.characterPartAnimator = characterPartAnimator;
            this.partVariantColour = partVariantColour;
            this.partVariantType = partVariantType;
        }
    }
}
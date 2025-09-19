using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crop
{
    [CreateAssetMenu(fileName = "so_CropDetailsList", menuName = "ScriptableObjects/Crop/CropDetailsList")]
    public class SO_CropDetailsList : ScriptableObject
    {
        public List<CropDetails> cropDetails;

        public CropDetails GetCropDetails(int seedItemCode)
        {
            return cropDetails.Find(x => x.seedItemCode == seedItemCode);
        }
    }
}
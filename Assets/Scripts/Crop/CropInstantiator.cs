using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Events;
using Assets.Scripts.Map;
using Assets.Scripts.Utilities.PropertyDrawers;
using UnityEngine;

namespace Assets.Scripts.Crop
{
    public class CropInstantiator : MonoBehaviour
    {
        #region Fields
        private Grid _grid;
        [SerializeField] private int _daysSinceLastDig = -1;
        [SerializeField] private int _daysSinceLastWater = -1;

        [ItemCodeDescription]
        [SerializeField] private int _seedItemCode = 0;
        [SerializeField] private int _growthDays = 0;
        #endregion

        #region LifeCycle Methods
        private void OnEnable()
        {
            EventHandler.InstantiateCropPrefabsEvent += InstantiateCropPrefabs;
        }

        private void OnDisable()
        {
            EventHandler.InstantiateCropPrefabsEvent -= InstantiateCropPrefabs;
        }
        #endregion

        #region Private Methods
        private void InstantiateCropPrefabs()
        {
            _grid = FindObjectOfType<Grid>();

            Vector3Int cropGridPosition = _grid.WorldToCell(transform.position);

            SetCropGridProperties(cropGridPosition);

            Destroy(gameObject);
        }

        private void SetCropGridProperties(Vector3Int cropGridPosition)
        {
            if (_seedItemCode > 0)
            {
                GridPropertyDetails gridPropertyDetails = GridPropertyManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

                gridPropertyDetails.daysSinceLastDig = _daysSinceLastDig;
                gridPropertyDetails.daysSinceLastWater = _daysSinceLastWater;
                gridPropertyDetails.seedItemCode = _seedItemCode;
                gridPropertyDetails.growthDays = _growthDays;

                GridPropertyManager.Instance.SetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y, gridPropertyDetails);
            }
        }
        #endregion
    }
}
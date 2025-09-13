using Assets.Scripts.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Map
{
    [ExecuteAlways]
    [RequireComponent(typeof(Tilemap))]
    public class TilemapGridProperties : MonoBehaviour
    {
        private Tilemap _tilemap;
        private Grid _grid;
        [SerializeField] private SO_GridProperties _gridProperties = null;
        [SerializeField] private GridBoolProperty _gridBoolProperty = GridBoolProperty.Diggable;

        private void OnEnable()
        {
            if (Application.IsPlaying(gameObject)) return;

            _tilemap = GetComponent<Tilemap>();

            if (_gridProperties == null) return;

            _gridProperties.gridPropertyList.Clear();
        }

        private void OnDisable()
        {
            if (Application.IsPlaying(gameObject)) return;

            UpdateGridProperties();

            if (_gridProperties == null) return;

            EditorUtility.SetDirty(_gridProperties);
        }

        private void UpdateGridProperties()
        {
            _tilemap.CompressBounds();

            if (Application.IsPlaying(gameObject)) return;

            if (_gridProperties == null) return;

            Vector3Int startCell = _tilemap.cellBounds.min;
            Vector3Int endCell = _tilemap.cellBounds.max;

            for (int x = startCell.x; x < endCell.x; x++)
            {
                for (int y = startCell.y; y < endCell.y; y++)
                {
                    TileBase tile = _tilemap.GetTile(new Vector3Int(x, y, 0));

                    if (tile != null)
                    {
                        _gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), _gridBoolProperty, true));
                    }
                }
            }
        }
    }
}
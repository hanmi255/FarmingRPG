using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Map
{
    /// <summary>
    /// 瓦片地图网格属性类，用于在编辑器中设置和更新瓦片地图的网格属性
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Tilemap))]
    public class TilemapGridProperties : MonoBehaviour
    {
        #region Fields

        private Tilemap _tilemap;
        [SerializeField] private SO_GridProperties _gridProperties = null;
        [SerializeField] private GridBoolProperty _gridBoolProperty = GridBoolProperty.Diggable;

        #endregion

        #region Lifecycle Methods

        private void OnEnable()
        {
#if UNITY_EDITOR
            // 在播放模式下不执行
            if (Application.IsPlaying(gameObject)) return;

            _tilemap = GetComponent<Tilemap>();

            if (_gridProperties == null) return;

            // 清空网格属性列表
            _gridProperties.gridPropertyList.Clear();
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            // 在播放模式下不执行
            if (Application.IsPlaying(gameObject)) return;

            // 更新网格属性
            UpdateGridProperties();

            if (_gridProperties == null) return;

            // 标记资源为 dirty，确保在编辑器中保存更改
            EditorUtility.SetDirty(_gridProperties);
#endif
        }

        #endregion

        #region Private Methods

#if UNITY_EDITOR
        /// <summary>
        /// 更新网格属性，根据瓦片地图中的瓦片设置相应的网格属性
        /// </summary>
        private void UpdateGridProperties()
        {
            // 压缩瓦片地图边界以提高性能
            _tilemap.CompressBounds();

            // 在播放模式下不执行
            if (Application.IsPlaying(gameObject)) return;

            if (_gridProperties == null) return;

            // 获取瓦片地图的边界
            Vector3Int startCell = _tilemap.cellBounds.min;
            Vector3Int endCell = _tilemap.cellBounds.max;

            // 遍历所有网格单元
            for (int x = startCell.x; x < endCell.x; x++)
            {
                for (int y = startCell.y; y < endCell.y; y++)
                {
                    // 获取指定位置的瓦片
                    TileBase tile = _tilemap.GetTile(new Vector3Int(x, y, 0));

                    // 如果瓦片存在，则添加相应的网格属性
                    if (tile != null)
                    {
                        _gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), _gridBoolProperty, true));
                    }
                }
            }
        }
#endif

        #endregion
    }
}
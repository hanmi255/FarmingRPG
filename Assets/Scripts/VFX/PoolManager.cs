using System.Collections.Generic;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.VFX
{
    public class PoolManager : SingletonMonoBehaviour<PoolManager>
    {
        # region Fields
        private Dictionary<int, Queue<GameObject>> _poolDictionary = new();
        [SerializeField] private Pool[] pool = null;
        [SerializeField] private Transform objectPoolTransform = null;

        [System.Serializable]
        public struct Pool
        {
            public int poolSize;
            public GameObject prefab;
        }

        #endregion

        # region Lifecycle Methods
        private void Start()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                CreatePool(pool[i].prefab, pool[i].poolSize);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取重复使用的对象
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            int poolKey = prefab.GetInstanceID();

            if (_poolDictionary.ContainsKey(poolKey))
            {
                GameObject objectToReuse = GetObjectFromPool(poolKey);

                ResetObject(objectToReuse, prefab, position, rotation);

                return objectToReuse;
            }
            else
            {
                Debug.LogError($"PoolManager: ReuseObject: prefab:{prefab} not found in pool");
                return null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="poolSize"></param>
        private void CreatePool(GameObject prefab, int poolSize)
        {
            if (prefab == null) return;

            int poolKey = prefab.GetInstanceID();

            // 如果已存在该对象池，则跳过创建
            if (_poolDictionary.ContainsKey(poolKey)) return;

            GameObject parentGameObject = new(prefab.name + "Anchor");
            parentGameObject.transform.SetParent(objectPoolTransform);

            _poolDictionary.Add(poolKey, new Queue<GameObject>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform);
                newObject.SetActive(false);

                _poolDictionary[poolKey].Enqueue(newObject);
            }
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="poolKey"></param>
        private GameObject GetObjectFromPool(int poolKey)
        {
            GameObject objectToReuse = _poolDictionary[poolKey].Dequeue();
            _poolDictionary[poolKey].Enqueue(objectToReuse);

            if (objectToReuse.activeSelf)
            {
                objectToReuse.SetActive(false);
            }

            return objectToReuse;
        }

        /// <summary>
        /// 重置对象位置，旋转，缩放
        /// </summary>
        /// <param name="objectToReuse"></param>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        private void ResetObject(GameObject objectToReuse, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            objectToReuse.transform.SetPositionAndRotation(position, rotation);

            objectToReuse.transform.localScale = prefab.transform.localScale;
        }

        #endregion
    }
}
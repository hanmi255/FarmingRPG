using System;
using UnityEngine;

namespace Assets.Scripts.AStarAlgorithm
{
    public class Node : IComparable<Node>
    {
        #region Fields
        public Vector2Int gridPosition; // 网格位置
        public int gCost = 0;  // 从起点到当前节点的实际代价
        public int hCost = 0;  // 从当前节点到终点的估计代价
        public bool isObstacle = false; // 是否为障碍物
        public int movementPenalty; // 移动代价
        public Node parentNode; // 父节点
        #endregion

        #region Properties
        public int FCost
        {
            get { return gCost + hCost; }
        }
        #endregion

        #region Public Methods
        public Node(Vector2Int gridPosition)
        {
            this.gridPosition = gridPosition;
            parentNode = null;
        }

        /// <summary>
        /// 比较两个节点的 FCost
        /// </summary>
        /// <param name="other"></param>
        /// <returns> FCost 相同则比较 hCost，否则比较 FCost </returns>
        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare != 0)
            {
                return compare;
            }
            return hCost.CompareTo(other.hCost);
        }
        #endregion
    }
}

using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Map;
using Assets.Scripts.NPC;
using Assets.Scripts.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.AStarAlgorithm
{
    public class AStar : MonoBehaviour
    {
        #region Fields
        [Header("Tiles & Tilemap References")]
        [Header("Options")]
        [SerializeField] private bool _observeMovementPenalties = true;

        [Range(0, 20)]
        [SerializeField] private int _pathMovementPenalty = 0;

        [Range(0, 20)]
        [SerializeField] private int _defaultMovementPenalty = 0;

        private GridNodes _gridNodes;
        private Node _startNode;
        private Node _targetNode;
        private int _gridWidth;
        private int _gridHeight;
        private int _originX;
        private int _originY;

        private List<Node> _openList;
        private HashSet<Node> _closedList;

        private bool _pathFound = false;
        #endregion

        #region Public Methods
        public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition, Stack<NPCMovementStep> npcMovementStepStack)
        {
            if (!PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, targetGridPosition))
                return false;

            if (!FindShortestPath())
                return false;

            UpdatePathOnNPCMovementStepStack(sceneName, npcMovementStepStack);
            return true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 从网格属性字典填充网格节点数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="startGridPosition">起始网格位置</param>
        /// <param name="targetGridPosition">目标网格位置</param>
        /// <returns>是否成功填充</returns>
        private bool PopulateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition)
        {
            if (!TryGetSceneSaveData(sceneName, out SceneSave sceneSave))
                return false;

            if (!InitializeGridNodes(sceneName, out Vector2Int gridOrigin))
                return false;

            // 设置起始和目标节点
            SetStartAndTargetNodes(startGridPosition, targetGridPosition, gridOrigin);

            // 填充节点属性
            PopulateNodeProperties(sceneSave.gridPropertyDetailsDictionary, gridOrigin);

            return true;
        }

        /// <summary>
        /// 查找从起始节点到目标节点的最短路径
        /// </summary>
        /// <returns>是否找到路径</returns>
        private bool FindShortestPath()
        {
            InitializePathfinding();

            return ExecutePathfindingLoop();
        }

        /// <summary>
        /// 更新NPC移动步骤栈
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="npcMovementStepStack">NPC移动步骤栈</param>
        private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
        {
            Node nextNode = _targetNode;

            while(nextNode != null)
            {
                NPCMovementStep npcMovementStep = new()
                {
                    sceneName = sceneName,
                    gridCoordinate = new(nextNode.gridPosition.x + _originX, nextNode.gridPosition.y + _originY)
                };

                npcMovementStepStack.Push(npcMovementStep);

                nextNode = nextNode.parentNode;
            }
        }
        #endregion

        #region Used For PopulateGridNodesFromGridPropertiesDictionary()
        /// <summary>
        /// 尝试获取场景保存数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="sceneSave">输出的场景保存数据</param>
        /// <returns>是否成功获取</returns>
        private bool TryGetSceneSaveData(SceneName sceneName, out SceneSave sceneSave)
        {
            if (!GridPropertyManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName.ToString(), out sceneSave))
                return false;

            if (sceneSave.gridPropertyDetailsDictionary == null)
                return false;

            return true;
        }

        /// <summary>
        /// 初始化网格节点和相关数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="gridOrigin">输出的网格原点</param>
        /// <returns>是否成功初始化</returns>
        private bool InitializeGridNodes(SceneName sceneName, out Vector2Int gridOrigin)
        {
            if (!GridPropertyManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimensions, out gridOrigin))
                return false;

            // 初始化网格数据
            _gridNodes = new(gridDimensions.x, gridDimensions.y);
            _gridWidth = gridDimensions.x;
            _gridHeight = gridDimensions.y;
            _originX = gridOrigin.x;
            _originY = gridOrigin.y;

            // 初始化A*算法列表
            _openList = new();
            _closedList = new();

            return true;
        }

        /// <summary>
        /// 设置起始和目标节点
        /// </summary>
        /// <param name="startGridPosition">起始位置</param>
        /// <param name="targetGridPosition">目标位置</param>
        /// <param name="gridOrigin">网格原点</param>
        private void SetStartAndTargetNodes(Vector2Int startGridPosition, Vector2Int targetGridPosition, Vector2Int gridOrigin)
        {
            _startNode = _gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x, startGridPosition.y - gridOrigin.y);
            _targetNode = _gridNodes.GetGridNode(targetGridPosition.x - gridOrigin.x, targetGridPosition.y - gridOrigin.y);
        }

        /// <summary>
        /// 填充节点属性（障碍物、路径、移动代价等）
        /// </summary>
        /// <param name="gridPropertyDetailsDictionary">网格属性字典</param>
        /// <param name="gridOrigin">网格原点</param>
        private void PopulateNodeProperties(Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary, Vector2Int gridOrigin)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    ProcessSingleNodeProperty(x, y, gridPropertyDetailsDictionary, gridOrigin);
                }
            }
        }

        /// <summary>
        /// 处理单个节点的属性设置
        /// </summary>
        /// <param name="x">节点X坐标</param>
        /// <param name="y">节点Y坐标</param>
        /// <param name="gridPropertyDetailsDictionary">网格属性字典</param>
        /// <param name="gridOrigin">网格原点</param>
        private void ProcessSingleNodeProperty(int x, int y, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary, Vector2Int gridOrigin)
        {
            GridPropertyDetails gridPropertyDetails = GridPropertyManager.Instance.GetGridPropertyDetails(
                x + gridOrigin.x, y + gridOrigin.y, gridPropertyDetailsDictionary);

            if (gridPropertyDetails == null)
                return;

            Node node = _gridNodes.GetGridNode(x, y);

            if (gridPropertyDetails.isNPCObstacle)
            {
                node.isObstacle = true;
            }
            else if (gridPropertyDetails.isPath)
            {
                node.movementPenalty = _pathMovementPenalty;
            }
            else
            {
                node.movementPenalty = _defaultMovementPenalty;
            }
        }
        #endregion

        #region Used For FindShortestPath()
        /// <summary>
        /// 初始化路径查找算法
        /// </summary>
        private void InitializePathfinding()
        {
            _openList.Clear();
            _closedList.Clear();
            _pathFound = false;

            _openList.Add(_startNode);
        }

        /// <summary>
        /// 执行主要的路径查找循环
        /// </summary>
        /// <returns>是否找到路径</returns>
        private bool ExecutePathfindingLoop()
        {
            while (_openList.Count > 0)
            {
                // 获取并处理当前最优节点
                Node currentNode = GetAndProcessCurrentNode();

                // 检查是否到达目标
                if (IsTargetReached(currentNode))
                    return true;

                // 评估当前节点的邻居
                EvaluateNeighbours(currentNode);
            }

            return _pathFound;
        }

        /// <summary>
        /// 获取并处理当前最优节点
        /// </summary>
        /// <returns>当前处理的节点</returns>
        private Node GetAndProcessCurrentNode()
        {
            _openList.Sort();
            Node currentNode = _openList[0];
            _openList.RemoveAt(0);
            _closedList.Add(currentNode);

            return currentNode;
        }

        /// <summary>
        /// 检查是否到达目标节点
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <returns>是否到达目标</returns>
        private bool IsTargetReached(Node currentNode)
        {
            if (currentNode == _targetNode)
            {
                _pathFound = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 评估当前节点的所有邻居节点
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        private void EvaluateNeighbours(Node currentNode)
        {
            if (currentNode == null)
                return;

            Vector2Int currentPosition = currentNode.gridPosition;

            // 遍历所有8个方向的邻居
            ProcessNeighboursInAllDirections(currentNode, currentPosition);
        }

        /// <summary>
        /// 处理所有方向的邻居节点
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <param name="currentPosition">当前位置</param>
        private void ProcessNeighboursInAllDirections(Node currentNode, Vector2Int currentPosition)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    ProcessSingleNeighbour(currentNode, currentPosition.x + i, currentPosition.y + j);
                }
            }
        }

        /// <summary>
        /// 处理单个邻居节点
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <param name="neighbourX">邻居X坐标</param>
        /// <param name="neighbourY">邻居Y坐标</param>
        private void ProcessSingleNeighbour(Node currentNode, int neighbourX, int neighbourY)
        {
            Node neighbourNode = GetValidNeighbourNode(neighbourX, neighbourY);
            if (neighbourNode == null)
                return;

            // 计算到邻居节点的新成本
            int newCostToNeighbour = CalculateMovementCost(currentNode, neighbourNode);
            bool isInOpenList = _openList.Contains(neighbourNode);

            if (newCostToNeighbour >= neighbourNode.gCost && isInOpenList)
                return;

            // 更新邻居节点信息
            UpdateNeighbourNodeInfo(currentNode, neighbourNode, newCostToNeighbour, isInOpenList);
        }

        /// <summary>
        /// 获取有效的邻居节点
        /// </summary>
        /// <param name="x">邻居X坐标</param>
        /// <param name="y">邻居Y坐标</param>
        /// <returns>有效的邻居节点</returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            if (x >= _gridWidth || x < 0 || y >= _gridHeight || y < 0)
                return null;

            Node node = _gridNodes.GetGridNode(x, y);

            if (node.isObstacle || _closedList.Contains(node))
                return null;

            return node;
        }

        /// <summary>
        /// 计算从当前节点到邻居节点的移动成本
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <param name="neighbourNode">邻居节点</param>
        /// <returns>移动成本</returns>
        private int CalculateMovementCost(Node currentNode, Node neighbourNode)
        {
            int baseCost = currentNode.gCost + GetDistance(currentNode, neighbourNode);

            return _observeMovementPenalties
                ? baseCost + neighbourNode.movementPenalty
                : baseCost;
        }

        /// <summary>
        /// 计算两个节点之间的曼哈顿距离
        /// </summary>
        /// <param name="nodeA">节点A</param>
        /// <param name="nodeB">节点B</param>
        /// <returns>曼哈顿距离</returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// 更新邻居节点的路径信息
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <param name="neighbourNode">邻居节点</param>
        /// <param name="newCost">新的G成本</param>
        /// <param name="isInOpenList">是否已在开放列表中</param>
        private void UpdateNeighbourNodeInfo(Node currentNode, Node neighbourNode, int newCost, bool isInOpenList)
        {
            neighbourNode.gCost = newCost;
            neighbourNode.hCost = GetDistance(neighbourNode, _targetNode);
            neighbourNode.parentNode = currentNode;

            if (!isInOpenList)
            {
                _openList.Add(neighbourNode);
            }
        }
        #endregion
    }
}

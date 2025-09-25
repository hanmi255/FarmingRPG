using UnityEngine;

namespace Assets.Scripts.AStarAlgorithm
{
    public class GridNodes
    {
        #region Fields
        private readonly int _width;
        private readonly int _height;

        private readonly Node[,] _gridNode;
        #endregion

        #region Public Methods
        public GridNodes(int width, int height)
        {
            _width = width;
            _height = height;
            _gridNode = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        public Node GetGridNode(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                return _gridNode[x, y];
            }
            Debug.LogError($"GridNode at ({x}, {y}) is out of bounds.");
            return null;
        }
        #endregion
    }
}

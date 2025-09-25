using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.NPC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.AStarAlgorithm
{
    [RequireComponent(typeof(AStar))]
    public class AStarTest : MonoBehaviour
    {
        #region Fields
        [SerializeField] private AStar _astar;
        [SerializeField] private Vector2Int _startPosition;
        [SerializeField] private Vector2Int _targetPosition;
        [SerializeField] private Tilemap _tileMapToDisplayPath = null;
        [SerializeField] private TileBase _pathTile = null;
        [SerializeField] private bool _displayStartAndTarget = false;
        [SerializeField] private bool _displayPath = false;

        private Stack<NPCMovementStep> _npcMovementStepStack;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _astar = GetComponent<AStar>();
            _npcMovementStepStack = new Stack<NPCMovementStep>();
        }

        private void Update()
        {
            if (_startPosition != null && _targetPosition != null && _tileMapToDisplayPath != null && _pathTile != null)
            {
                if (_displayStartAndTarget)
                {
                    _tileMapToDisplayPath.SetTile(new Vector3Int(_startPosition.x, _startPosition.y, 0), _pathTile);
                    _tileMapToDisplayPath.SetTile(new Vector3Int(_targetPosition.x, _targetPosition.y, 0), _pathTile);
                }
                else
                {
                    _tileMapToDisplayPath.SetTile(new Vector3Int(_startPosition.x, _startPosition.y, 0), null);
                    _tileMapToDisplayPath.SetTile(new Vector3Int(_targetPosition.x, _targetPosition.y, 0), null);
                }

                if (_displayPath)
                {
                    Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, out SceneName sceneName);
                    _astar.BuildPath(sceneName, _startPosition, _targetPosition, _npcMovementStepStack);

                    foreach (var npcMovementStep in _npcMovementStepStack)
                    {
                        _tileMapToDisplayPath.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), _pathTile);
                    }
                }
                else
                {
                    if (_npcMovementStepStack.Count > 0)
                    {
                        foreach (var npcMovementStep in _npcMovementStepStack)
                        {
                            _tileMapToDisplayPath.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), null);
                        }

                        _npcMovementStepStack.Clear();
                    }
                }
            }
        }
        #endregion
    }
}
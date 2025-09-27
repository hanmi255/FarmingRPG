using Assets.Scripts.Enums;
using Assets.Scripts.Map;
using Assets.Scripts.NPC;
using UnityEngine;

namespace Assets.Scripts.AStarAlgorithm
{
    [RequireComponent(typeof(AStar))]
    public class AStarTest : MonoBehaviour
    {
        #region Fields
        [SerializeField] private NPCPath _npcPath = null;
        [SerializeField] private bool _moveNPC = false;
        [SerializeField] private Vector2Int _finishPosition;
        [SerializeField] private AnimationClip _idleDownAnimationClip = null;
        [SerializeField] private AnimationClip _eventAnimationClip = null;
        private NPCMovement _npcMovement;
        #endregion

        #region Lifecycle Methods
        private void Start()
        {
            _npcMovement = _npcPath.GetComponent<NPCMovement>();
            _npcMovement.npcFacingDirectionAtDestination = Direction.Down;
            _npcMovement.npcTargetAnimationClip = _idleDownAnimationClip;
        }

        private void Update()
        {
            if (_moveNPC)
            {
                _moveNPC = false;

                NPCScheduleEvent npcScheduleEvent = new(
                    0,
                    0,
                    0,
                    0,
                    Weather.None,
                    Season.None,
                    SceneName.Farm,
                    new GridCoordinate(_finishPosition.x, _finishPosition.y),
                    _eventAnimationClip);

                _npcPath.BuildPath(npcScheduleEvent);
            }
        }
        #endregion
    }
}
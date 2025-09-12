using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Scene
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SceneTeleport : MonoBehaviour
    {
        [SerializeField] private SceneName _sceneNameGoto = SceneName.Farm;
        [SerializeField] private Vector3 _spawnPosition = Vector3.zero;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerUnit playerUnit = collision.GetComponent<PlayerUnit>();

            if (playerUnit != null)
            {
                float xPosition = Mathf.Approximately(_spawnPosition.x, 0f) ? playerUnit.transform.position.x : _spawnPosition.x;
                float yPosition = Mathf.Approximately(_spawnPosition.y, 0f) ? playerUnit.transform.position.y : _spawnPosition.y;
                float zPosition = 0f;

                SceneControllerManager.Instance.FadeAndLoadScene(_sceneNameGoto.ToString(), new Vector3(xPosition, yPosition, zPosition));
            }
        }
    }
}
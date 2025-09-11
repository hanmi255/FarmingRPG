using Cinemachine;
using UnityEngine;
using Assets.Scripts.Misc;
using Assets.Scripts.Events;

namespace Assets.Scripts.Scene
{
    [RequireComponent(typeof(CinemachineConfiner2D))]
    public class SwitchConfineBoundingShape : MonoBehaviour
    {
        private void OnEnable() {
            EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
        }

        private void OnDisable() {
            EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
        }

        /// <summary>
        /// 切换碰撞器用于Cinemachine定义边界
        /// </summary>
        private void SwitchBoundingShape()
        {
            PolygonCollider2D polygonCollider2D = GameObject.FindWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

            CinemachineConfiner2D confiner2D = GetComponent<CinemachineConfiner2D>();
            confiner2D.m_BoundingShape2D = polygonCollider2D;

            confiner2D.InvalidateCache();
        }
    }
}
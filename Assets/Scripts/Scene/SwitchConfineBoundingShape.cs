using Cinemachine;
using UnityEngine;
using Assets.Scripts.Misc;

namespace Assets.Scripts.Scene
{
    [RequireComponent(typeof(CinemachineConfiner2D))]
    public class SwitchConfineBoundingShape : MonoBehaviour
    {
        void Start()
        {
            SwitchBoundingShape();
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
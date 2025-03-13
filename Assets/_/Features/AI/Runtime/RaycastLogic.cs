using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Runtime
{
    public class RaycastLogic
    {
        #region Exposed

        public RaycastHit m_hit;
        public int m_priority;
        public float m_angle;
        public float m_distance;
        public LayerMask m_layer;

        #endregion


        #region Constructor

        public RaycastLogic(RaycastHit hit, int priority, float angle, float distance, LayerMask layer)
        {
            m_hit = hit;
            m_priority = priority;
            m_angle = angle;
            m_distance = distance;
            m_layer = layer;
        }

        #endregion
    }
}
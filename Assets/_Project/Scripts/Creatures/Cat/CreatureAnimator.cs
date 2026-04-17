using System;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CreatureAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private string m_VerticalID = "Vert";
        [SerializeField] private string m_StateID = "State";

        [SerializeField]
        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        private Animator m_Animator;
        private Vector2 m_FlowAxis;
        private float m_FlowState;

        private const float k_InputFlow = 6f;

        private Vector3 m_LookTarget;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        public void UpdateAnimation(Vector3 worldVelocity, bool isRunning, float deltaTime)
        {
            Vector3 local = transform.InverseTransformDirection(worldVelocity);
            Vector2 axis = new Vector2(local.x, local.z);

            // Smooth animation
            m_FlowAxis = Vector2.Lerp(m_FlowAxis, axis, deltaTime * k_InputFlow);
            m_FlowState = Mathf.Lerp(m_FlowState, isRunning ? 1f : 0f, deltaTime * k_InputFlow);

            m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
            m_Animator.SetFloat(m_StateID, m_FlowState);
        }

        public void SetLookTarget(Vector3 target)
        {
            m_LookTarget = target;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            m_Animator.SetLookAtPosition(m_LookTarget);
            m_Animator.SetLookAtWeight(
                m_LookWeight.weight,
                m_LookWeight.body,
                m_LookWeight.head,
                m_LookWeight.eyes);
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }
    }
}
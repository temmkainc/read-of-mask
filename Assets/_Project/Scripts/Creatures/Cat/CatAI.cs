using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CreatureAnimator))]
    public class CatAI : MonoBehaviour
    {
        [Header("Wandering")]
        [SerializeField] private float m_WanderRadius = 10f;
        [SerializeField] private float m_IdleTimeMin = 2f;
        [SerializeField] private float m_IdleTimeMax = 5f;
        [SerializeField] private float m_RunChance = 0.3f;
        [SerializeField] private float m_RunDurationMin = 1.5f;
        [SerializeField] private float m_RunDurationMax = 4f;

        [Header("Movement")]
        [SerializeField] private float m_RunSpeed = 4f;
        [SerializeField] private float m_WalkSpeed = 1.5f;

        private enum AIState { Idle, Wander, Flee }
        private AIState m_State = AIState.Idle;

        private NavMeshAgent m_Agent;
        private CreatureAnimator m_Animator;

        private Coroutine m_FleeRoutine;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<CreatureAnimator>();

            // Start disabled - the NavMeshAgent tries to place itself onto the NavMesh the
            // moment it's enabled, and Unity doesn't guarantee this GameObject's Awake/OnEnable
            // runs after the level's NavMeshSurface has registered its data. Enabling it
            // ourselves after a short delay (see OnEnable) guarantees the surface is ready first.
            m_Agent.enabled = false;
        }

        private void OnEnable()
        {
            StartCoroutine(InitializeAgentThenRun());
        }

        private IEnumerator InitializeAgentThenRun()
        {
            // Give every other object in the scene (including the level's NavMeshSurface) a full
            // chance to finish Awake/OnEnable/Start before this agent tries to place itself.
            yield return new WaitForSeconds(0.3f);

            if (m_Agent != null)
            {
                m_Agent.enabled = true;
                m_Agent.updatePosition = true;
                m_Agent.updateRotation = true;
            }

            yield return BehaviourLoop();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (m_Agent == null || !m_Agent.enabled || !m_Agent.isOnNavMesh)
                return;

            bool isRunning = m_State == AIState.Flee || m_Agent.speed > m_WalkSpeed + 0.1f;

            m_Animator.UpdateAnimation(m_Agent.velocity, isRunning, Time.deltaTime);

            if (m_Agent.velocity.sqrMagnitude > 0.01f)
            {
                m_Animator.SetLookTarget(transform.position + m_Agent.velocity);
            }
        }

        // ─────────────── BEHAVIOUR LOOP ───────────────

        private IEnumerator BehaviourLoop()
        {
            while (true)
            {
                switch (m_State)
                {
                    case AIState.Idle:
                        yield return DoIdle();
                        break;

                    case AIState.Wander:
                        yield return DoWander();
                        break;

                    case AIState.Flee:
                        yield return null;
                        break;
                }
            }
        }

        private IEnumerator DoIdle()
        {
            m_Agent.isStopped = true;

            float time = Random.Range(m_IdleTimeMin, m_IdleTimeMax);
            yield return new WaitForSeconds(time);

            m_State = AIState.Wander;
        }

        private IEnumerator DoWander()
        {
            if (!TryGetRandomPoint(out Vector3 destination))
            {
                m_State = AIState.Idle;
                yield break;
            }

            bool willRun = Random.value < m_RunChance;
            float runTimer = 0f;
            float runDuration = Random.Range(m_RunDurationMin, m_RunDurationMax);

            m_Agent.isStopped = false;
            m_Agent.SetDestination(destination);

            float timeout = 15f;
            float timer = 0f;

            while (timer < timeout && m_State == AIState.Wander)
            {
                timer += Time.deltaTime;

                // Handle run / walk
                if (willRun && runTimer < runDuration)
                {
                    m_Agent.speed = m_RunSpeed;
                    runTimer += Time.deltaTime;
                }
                else
                {
                    m_Agent.speed = m_WalkSpeed;
                }

                if (!m_Agent.pathPending && m_Agent.remainingDistance < 0.3f)
                    break;

                yield return null;
            }

            if (m_State == AIState.Wander)
                m_State = AIState.Idle;
        }

        // ─────────────── FLEE ───────────────

        public void FleeTo(Vector3 position)
        {
            if (m_Agent == null || !m_Agent.enabled || !m_Agent.isOnNavMesh)
                return;

            // Stop previous flee if running
            if (m_FleeRoutine != null)
                StopCoroutine(m_FleeRoutine);

            m_State = AIState.Flee;

            m_Agent.speed = m_RunSpeed;
            m_Agent.isStopped = false;
            m_Agent.SetDestination(position);

            m_FleeRoutine = StartCoroutine(FleeRoutine());
        }

        private IEnumerator FleeRoutine()
        {
            float timeout = 20f;
            float timer = 0f;

            while (timer < timeout && m_State == AIState.Flee)
            {
                timer += Time.deltaTime;

                if (!m_Agent.pathPending && m_Agent.remainingDistance < 0.5f)
                    break;

                yield return null;
            }

            // Return to normal behavior
            m_State = AIState.Idle;
            m_Agent.isStopped = true;

            m_FleeRoutine = null;
        }

        // ─────────────── NAVMESH ───────────────

        private bool TryGetRandomPoint(out Vector3 result)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 rand = Random.insideUnitCircle * m_WanderRadius;
                Vector3 point = transform.position + new Vector3(rand.x, 0, rand.y);

                if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = transform.position;
            return false;
        }
    }
}

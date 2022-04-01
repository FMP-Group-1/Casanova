using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Sleeping,
    Pursuing,
    Patrolling,
    ReturningToPatrol,
    InCombat,
    Dead
}

public enum PatrolState
{
    Patrol,
    ReversePatrol,
    Waiting
}

public enum CombatState
{
    Strafing,
    MaintainDist,
    BackingUp,
    MovingToAttack,
    Attacking
}

public enum WakeTrigger
{
    Attack,
    Standard
}

enum StrafeDir
{
    Left,
    Right
}

// Enemy AI Script, will likely be reworked to use inheritance once base functionality is polished
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent m_navMeshAgent;
    private AIState m_state = AIState.Idle;
    private CombatState m_combatState = CombatState.Strafing;
    private AIState m_stateBeforeHit = AIState.Idle;
    [Header("Movement Values")]
    [SerializeField]
    private float m_walkSpeed = 1.5f;
    [SerializeField]
    private float m_runSpeed = 3.0f;

    // Animation Relevant Variables
    private Animator m_animController;
    private float m_prevAnimSpeed;

    // Patrol Relevant Variables
    [Header("Patrol Values")]
    [SerializeField]
    private bool m_spawnAsleep = false;
    [SerializeField]
    private GameObject m_patrolRoute;
    private PatrolState m_patrolState = PatrolState.Patrol;
    private List<Transform> m_patrolRoutePoints = new List<Transform>();
    private Transform m_nextPatrolPoint;
    private Vector3 m_lastPointOnPatrol;
    private float m_patrolTimer = 0.0f;
    private float m_patrolWaitTime = 2.5f;
    private int m_patrolDestinationIndex = 1;
    [SerializeField]
    private float m_patrolStoppingDistance = 1.5f;

    // Player/Detection Relevant Variables
    private GameObject m_player;
    private CapsuleCollider m_playerCollider;

    // Strafe Relevant Variables
    [Header("Combat Values")]
    [SerializeField]
    private float m_health = 100.0f;
    [SerializeField]
    private float m_playerStoppingDistance = 1.75f;
    private StrafeDir m_strafeDir = StrafeDir.Left;
    [SerializeField]
    private float m_strafeSpeed = 1.5f;
    [SerializeField]
    private float m_minStrafeRange = 3.0f;
    [SerializeField]
    private float m_maxStrafeRange = 5.0f;
    private float m_strafeAtDist;

    // Combat Variables
    private float m_attackTimer;
    [SerializeField]
    private float m_minAttackTime = 3.5f;
    [SerializeField]
    private float m_maxAttackTime = 7.5f;
    private float m_timeSinceLastAttack = 0.0f;
    [SerializeField]
    private GameObject m_weapon;
    private BoxCollider m_weaponCollider;

    // Vision Detection Relevant Variables
    [Header("Player Detection Values")]
    [SerializeField]
    private bool m_playerDetectionEnabled = true;
    [SerializeField]
    private float m_viewRadius = 7.5f;    
    [SerializeField]
    [Range(0.0f,360.0f)]
    private float m_viewAngle = 145.0f;

    [SerializeField]
    private LayerMask m_obstacleMask;


    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();

        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerCollider = m_player.GetComponent<CapsuleCollider>();
        m_weaponCollider = m_weapon.GetComponent<BoxCollider>();

        DisableCollision();

        if (m_spawnAsleep)
        {
            SetAIState(AIState.Sleeping);
        }

        RandomiseStrafeRange();
    }

    private void Update()
    {
        // Setting the player mat color to see if attack is colliding. Will need removing later on.
        m_player.GetComponent<Player>().SetHitVisual( IsAttackCollidingWithPlayer() );

        TestingInputs();

        switch ( m_state )
        {
            // Idle State
            case AIState.Idle:
            {
                if ( IsPlayerVisible() )
                {
                    // Disabled Detection in Idle for now
                    //SetAIState(AIState.Pursuing);
                }
                break;
            }
            case AIState.Sleeping:
            {

                break;
            }
            // Chase after target/player
            case AIState.Pursuing:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Very basic detection for reaching destination, will need to be expanded upon
                // i.e. in case of path being blocked
                // Logic from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
                if ( IsInStrafeRange() )
                {
                    SetAIState(AIState.InCombat);
                    //Debug.Log("Destination Reached");
                }
                break;
            }
            // Patrol Logic
            case AIState.Patrolling:
            {
                if (IsPlayerVisible())
                {
                    SetAIState(AIState.Pursuing);
                }
                PatrolUpdate();
                break;
            }
            case AIState.ReturningToPatrol:
            {
                if ( HasReachedDestination() )
                {
                    SetAIState(AIState.Patrolling);
                }
                break;
            }
            case AIState.InCombat:
            {
                CombatUpdate();
                break;
            }
        }
    }

    private void PatrolUpdate()
    {
        if (m_patrolRoute == null)
        {
            Debug.Log("There is no patrol route attached to the AI. Please attach one.");
            SetAIState(AIState.Idle);
        }

        switch (m_patrolState)
        {
            case PatrolState.Patrol:
            {
                if (HasReachedDestination())
                {
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    else
                    {
                        m_patrolDestinationIndex++;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.ReversePatrol:
            {
                if (HasReachedDestination())
                {
                    if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    else
                    {
                        m_patrolDestinationIndex--;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.Waiting:
            {
                m_patrolTimer += Time.deltaTime;

                if (m_patrolTimer >= m_patrolWaitTime)
                {
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.ReversePatrol);
                    }
                    else if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Patrol);
                    }
                }
                break;
            }
        }
    }

    private void CombatUpdate()
    {
        m_timeSinceLastAttack += Time.deltaTime;

        switch ( m_combatState )
        {
            case CombatState.Strafing:
            {
                AttackCheck();
                StrafeRangeCheck();
                Strafe();
                break;
            }
            case CombatState.MaintainDist:
            {
                AttackCheck();
                StrafeRangeCheck();
                transform.LookAt(m_player.transform.position);
                break;
            }
            case CombatState.BackingUp:
            {
                BackUp();
                StrafeRangeCheck();
                transform.LookAt(m_player.transform.position);

                // Todo: Disgusting check, fix asap
                if (Vector3.Distance(m_player.transform.position, transform.position) > m_strafeAtDist)
                {
                    StrafeOrMaintain();
                }

                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.Attacking);
                }
                break;
            }
            case CombatState.Attacking:
            {
                transform.LookAt(m_player.transform.position);
                break;
            }
        }
    }

    private void SetAIState( AIState stateToSet )
    {
        // If changing FROM patrol state, store the last position in the patrol route
        if (m_state == AIState.Patrolling)
        {
            m_lastPointOnPatrol = gameObject.transform.position;
        }

        m_state = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case AIState.Idle:
            {
                StartIdleAnim();
                break;
            }
            case AIState.Sleeping:
            {
                SetToPlayDeadAnim();
                break;
            }
            case AIState.Pursuing:
            {
                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
                m_navMeshAgent.autoBraking = true;
                RandomiseStrafeRange();
                StartRunAnim();
                break;
            }
            case AIState.Patrolling:
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;

                if (m_patrolState == PatrolState.Patrol || m_patrolState == PatrolState.ReversePatrol)
                {
                    StartWalkAnim();
                }
                else if (m_patrolState == PatrolState.Waiting)
                {
                    StartIdleAnim();
                }
                break;
            }
            case AIState.ReturningToPatrol:
            {
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
                m_navMeshAgent.autoBraking = false;
                StartWalkAnim();
                break;
            }
            case AIState.InCombat:
            {
                //SetCombatState(CombatState.Strafing);
                StrafeOrMaintain();
                m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
                break;
            }
            case AIState.Dead:
            {
                StartDeathAnim();
                break;
            }
        }
    }

    private void SetPatrolState(PatrolState stateToSet)
    {
        m_patrolState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case PatrolState.Patrol:
            {
                StartWalkAnim();
                break;
            }
            case PatrolState.ReversePatrol:
            {
                StartWalkAnim();
                break;
            }
            case PatrolState.Waiting:
            {
                m_patrolTimer = 0.0f;
                StartIdleAnim();
                break;
            }
        }
    }

    private void SetCombatState(CombatState stateToSet)
    {
        m_combatState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case CombatState.Strafing:
            {
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            case CombatState.MaintainDist:
            {
                StartCombatIdleAnim();
                break;
            }
            case CombatState.Attacking:
            {
                StartAttackAnim();
                break;
            }
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;
                StartRunAnim();
                break;
            }
            case CombatState.BackingUp:
            {
                RandomiseStrafeRange();
                StartWalkBackAnim();
                break;
            }
        }
    }

    private void Strafe()
    {
        Vector3 offset;
        // Basic start to strafe logic
        if (m_strafeDir == StrafeDir.Left)
        {
            offset = m_player.transform.position - transform.position;
        }
        else
        {
            offset = transform.position - m_player.transform.position;
        }
        Vector3 dir = Vector3.Cross(offset, Vector3.up);
        m_navMeshAgent.SetDestination(transform.position + dir);
        transform.LookAt(m_player.transform.position);
    }

    private void BackUp()
    {
        Vector3 dir = (transform.position - m_player.transform.position).normalized;
        m_navMeshAgent.SetDestination(transform.position + (dir * 2.0f));
        transform.LookAt(m_player.transform.position);
    }

    private void StrafeRangeCheck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);

        if (distanceToPlayer > m_maxStrafeRange)
        {
            SetAIState(AIState.Pursuing);
        }
        // Player moved closer than strafe range
        if (distanceToPlayer < m_minStrafeRange && m_combatState != CombatState.BackingUp)
        {
            SetCombatState(CombatState.BackingUp);
            Debug.Log("Test");
        }
    }

    private void AttackCheck()
    {
        if (m_timeSinceLastAttack >= m_attackTimer)
        {
            SetCombatState(CombatState.MovingToAttack);
        }
    }

    private void StrafeOrMaintain()
    {
        int strafeOrMaintain = Random.Range(0, 2);
        if (strafeOrMaintain == 0)
        {
            SetCombatState(CombatState.Strafing);
        }
        else
        {
            SetCombatState(CombatState.MaintainDist);
        }
    }

    private void RandomiseStrafeRange()
    {
        m_strafeAtDist = Random.Range(m_minStrafeRange, m_maxStrafeRange);
    }

    private bool HasReachedDestination()
    {
        bool destinationReached = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (m_navMeshAgent.remainingDistance < m_navMeshAgent.stoppingDistance)
        {
            destinationReached = true;
        }

        // Very basic detection for reaching destination, will need to be expanded upon
        // i.e. in case of path being blocked
        // Logic from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
        //if (!m_navMeshAgent.pathPending)
        //{
        //    if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
        //    {
        //        if (!m_navMeshAgent.hasPath || m_navMeshAgent.velocity.sqrMagnitude == 0f)
        //        {
        //            destinationReached = true;
        //        }
        //    }
        //}

        return destinationReached;
    }

    private bool IsInStrafeRange()
    {
        bool inStrafeRange = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (m_navMeshAgent.remainingDistance < m_strafeAtDist)
        {
            inStrafeRange = true;
        }

        return inStrafeRange;
    }

    private void DisableCollision()
    {
        m_weaponCollider.enabled = false;
    }

    private void EnableCollision()
    {
        m_weaponCollider.enabled = true;
    }

    public bool IsAttackCollidingWithPlayer()
    {
        bool isColliding = false;

        // If using this method for actual collision, needs a collider.enabled check
        // But for demonstrating the collision, this is not present currently

        if (m_weaponCollider.bounds.Intersects(m_playerCollider.bounds))
        {
            isColliding = true;
        }

        return isColliding;
    }

    // DirFromAngle() and IsPlayerVisible() functions use logic from https://www.youtube.com/watch?v=rQG9aUWarwE
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public bool IsPlayerVisible()
    {
        bool playerIsVisible = false;

        // If in combat, just return true since no point redoing detection
        // Will need changing if de-aggro functionality is implemented
        if (m_state == AIState.InCombat || m_state == AIState.Pursuing)
        {
            return true;
        }

        // Todo: Look into using sqr root distance checks for optimisation
        if(m_playerDetectionEnabled)
        {
            // Checking if player is in range
            if (Vector3.Distance(transform.position, m_player.transform.position) <= m_viewRadius)
            {
                // Once player is in range, getting the direction to the player and checking if it's within the AI's FOV
                Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToPlayer) < m_viewAngle * 0.5f )
                {
                    // Once player is in range and in FOV, using Raycast to check if any obstacles are in the way
                    float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);
                    if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, m_obstacleMask))
                    {
                        playerIsVisible = true;
                    }
                }
            }
        }

        return playerIsVisible;
    }

    public void WakeUpAI(WakeTrigger wakeTrigger)
    {
        switch ( wakeTrigger )
        {
            case WakeTrigger.Attack:
            {
                // Todo: Maybe add "WakingUp" state?
                StartStandUpAnim();
                break;
            }
            case WakeTrigger.Standard:
            {
                break;
            }
        }
    }

    public AIState GetState()
    {
        return m_state;
    }

    public CombatState GetCombatState()
    {
        return m_combatState;
    }

    public PatrolState GetPatrolState()
    {
        return m_patrolState;
    }

    public float GetViewRadius()
    {
        return m_viewRadius;
    }

    public float GetViewAngle()
    {
        return m_viewAngle;
    }

    public float GetHealth()
    {
        return m_health;
    }

    public float GetStrafeDist()
    {
        return m_strafeAtDist;
    }

    private void StartWalkAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Walk");
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = true;
    }

    private void StartStrafeAnim( StrafeDir dirToStrafe )
    {
        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.speed = m_strafeSpeed;
        m_navMeshAgent.updateRotation = false;

        switch (dirToStrafe)
        {
            case StrafeDir.Left:
            {
                m_animController.SetTrigger("StrafeLeft");
                break;
            }
            case StrafeDir.Right:
            {
                m_animController.SetTrigger("StrafeRight");
                break;
            }
        }
    }

    private void StartWalkBackAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("WalkBack");
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = false;
    }

    private void StartRunAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Run");
        m_navMeshAgent.speed = m_runSpeed;
        m_navMeshAgent.updateRotation = true;
    }

    private void StartIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Idle");
        m_navMeshAgent.updateRotation = true;
    }

    private void StartCombatIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("CombatIdle");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Attack");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartStandUpAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("StandUp");
        m_navMeshAgent.updateRotation = false;
    }

    private void SetToPlayDeadAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("LayDown");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartDeathAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Death");
        m_navMeshAgent.updateRotation = false;
    }

    private void PlayDamageAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("TakeHit");
        DisableCollision();
    }

    private void RecoverFromHit()
    {
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.Pursuing);
        }
        else
        {
            SetAIState(m_stateBeforeHit);
        }
    }

    // Can possibly remove these pause and resume functions, but leave for now
    private void PauseAnimation()
    {
        m_prevAnimSpeed = m_animController.speed;
        m_animController.speed = 0.0f;
    }

    private void ResumeAnimation()
    {
        m_animController.speed = m_prevAnimSpeed;
    }

    private void EndAttack()
    {
        SetCombatState(CombatState.BackingUp);
        m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
        m_timeSinceLastAttack = 0.0f;
    }

    public void TakeDamage( float damageToTake )
    {
        m_stateBeforeHit = m_state;

        m_health -= damageToTake;

        if ( m_state != AIState.Sleeping)
        {
            PlayDamageAnim();
        }

        if ( m_health <= 0.0f )
        {
            m_health = 0.0f;
            SetAIState(AIState.Dead);
        }
    }

    public void ChangeStateFromWake()
    {
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.Pursuing);

            // Had to put this setter here to force path recalculation, otherwise AI would attack immediately.
            m_navMeshAgent.SetDestination(m_player.transform.position);
        }
        else
        {
            SetAIState(AIState.Patrolling);
        }
    }

    private void ResetAnimTriggers()
    {
        m_animController.ResetTrigger("Walk");
        m_animController.ResetTrigger("Idle");
        m_animController.ResetTrigger("Attack");
        m_animController.ResetTrigger("Run");
        m_animController.ResetTrigger("StandUp");
        m_animController.ResetTrigger("LayDown");
        m_animController.ResetTrigger("TakeHit");
        m_animController.ResetTrigger("StrafeLeft");
        m_animController.ResetTrigger("StrafeRight");
        m_animController.ResetTrigger("CombatIdle");
    }

    private void SetupPatrolRoutes()
    {
        // Adding patrol points to a list that the ai can use to follow
        if (m_patrolRoute != null)
        {
            for (int i = 0; i < m_patrolRoute.transform.childCount; i++)
            {
                m_patrolRoutePoints.Add(m_patrolRoute.transform.GetChild(i).gameObject.transform);
            }
        }

        // Checking patrol route points is valid, then setting next patrol point to the second entry
        if (m_patrolRoutePoints.Count >= 2)
        {
            m_nextPatrolPoint = m_patrolRoutePoints[1];
            m_lastPointOnPatrol = m_nextPatrolPoint.position;
        }
    }

    private void TestingInputs()
    {
        // Start Patrolling Test Input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetAIState(AIState.Patrolling);
            m_patrolState = PatrolState.Patrol;
            ResetAnimTriggers();
            StartWalkAnim();
            if (m_patrolRoute != null)
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
            }
            //Debug.Log("Going to Next Destination");
        }

        // Start Pursuing Test Input
        if (Input.GetKeyDown(KeyCode.P))
        {
            //SetAIState(AIState.Pursuing);
            //ResetAnimTriggers();
            //StartRunAnim();
        }

        // Start Sleeping Test Input
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SetAIState(AIState.Sleeping);
        //}
    }
}
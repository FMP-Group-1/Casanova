using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//*******************************************
// Author: Dean Pearce
// Class: EnemyAI
// Description: Base enemy AI class which handles navigation, behaviour, and animation
//*******************************************

public enum AIState
{
    Idle,
    Sleeping,
    Waking,
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
    Pursuing,
    Strafing,
    StrafingToZone,
    RadialRunToZone,
    MaintainDist,
    ClosingDist,
    BackingUp,
    MovingToZone,
    MovingToAttack,
    Attacking
}

public enum WakeTrigger
{
    Attack,
    Standard
}

public enum AttackingType
{
    Unassigned,
    Passive,
    Active
}

public enum StrafeDir
{
    Left,
    Right
}

public class EnemyAI : MonoBehaviour
{
    private AIManager m_aiManager;
    private AttackZoneManager m_attackZoneManager;

    private NavMeshAgent m_navMeshAgent;
    [SerializeField]
    [Tooltip("AI's Current State")]
    private AIState m_mainState = AIState.Idle;
    private CombatState m_combatState = CombatState.Strafing;
    private AIState m_stateBeforeHit = AIState.Idle;
    [Header("Movement Values")]
    [SerializeField]
    [Tooltip("The walk speed of the AI")]
    private float m_walkSpeed = 1.5f;
    [SerializeField]
    [Tooltip("The run speed of the AI")]
    private float m_runSpeed = 3.0f;

    // Animation Relevant Variables
    private Animator m_animController;
    private float m_prevAnimSpeed;

    // Patrol Relevant Variables
    [Header("Patrol Values")]
    [SerializeField]
    [Tooltip("Should the AI spawn asleep?")]
    private bool m_spawnAsleep = false;
    [SerializeField]
    [Tooltip("The trigger zone which will wake the AI when the player enters it")]
    private GameObject m_wakeTriggerObj;
    private BoxCollider m_wakeTrigger;
    [SerializeField]
    [Tooltip("The GameObject which holds the position objects for patrolling")]
    private GameObject m_patrolRoute;
    private PatrolState m_patrolState = PatrolState.Patrol;
    private List<Transform> m_patrolRoutePoints = new List<Transform>();
    private Transform m_nextPatrolPoint;
    private Vector3 m_lastPointOnPatrol;
    private float m_patrolTimer = 0.0f;
    private float m_patrolWaitTime = 2.5f;
    private int m_patrolDestinationIndex = 1;
    [SerializeField]
    [Tooltip("The distance the AI will stop from the patrol points")]
    private float m_patrolStoppingDistance = 1.5f;

    // Player/Detection Relevant Variables
    private GameObject m_player;
    private Collider m_playerCollider;

    // Combat Relevant Variables
    [Header("Combat Values")]
    [SerializeField]
    [Tooltip("The total health of the AI")]
    private float m_health = 100.0f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop")]
    private float m_playerStoppingDistance = 1.75f;
    //[SerializeField]
    //private bool m_canStrafe = true;
    private float m_delayBeforeStrafe = 0.0f;
    private float m_timeUntilStrafe = 0.0f;
    [SerializeField]
    [Tooltip("The minimum time the AI will stand still in combat before strafing")]
    private float m_minDelayBeforeStrafe = 6.0f;
    [SerializeField]
    [Tooltip("The maximum time the AI will stand still in combat before strafing")]
    private float m_maxDelayBeforeStrafe = 10.0f;
    private StrafeDir m_strafeDir = StrafeDir.Left;
    [SerializeField]
    [Tooltip("The strafing speed of the AI")]
    private float m_strafeSpeed = 1.5f;
    //[SerializeField]
    //private float m_minStrafeRange = 3.0f;
    //[SerializeField]
    //private float m_maxStrafeRange = 5.0f;
    [SerializeField]
    [Tooltip("The distance the AI will check for other obstructing AI during combat")]
    private float m_checkForAIDist = 2.0f;
    [SerializeField]
    [Tooltip("The angles the AI will check for other obstructing AI during combat")]
    private float m_checkForAIAngles = 45.0f;
    [SerializeField]
    [Tooltip("The distance the AI will move away when attempting to avoid other AI")]
    private float m_AIAvoidanceDist = 1.5f;
    private float m_strafeDist;
    private float m_attackTimer;
    [SerializeField]
    [Tooltip("Whether the AI can attack. For debugging")]
    private bool m_attackEnabled = true;
    [SerializeField]
    [Tooltip("The minimum time that has to pass before an actively attacking AI can attack")]
    private float m_minAttackTime = 3.5f;
    [SerializeField]
    [Tooltip("The maximum time that can pass before an actively attacking AI will attack")]
    private float m_maxAttackTime = 7.5f;
    private float m_timeSinceLastAttack = 0.0f;
    [SerializeField]
    [Tooltip("The weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_weapon;
    private BoxCollider m_weaponCollider;
    private Vector3 m_attackZonePos;
    private AttackingType m_currentAttackingType = AttackingType.Passive;
    private AttackZone m_currentAttackZone;
    private AttackZone m_occupiedAttackZone;
    private float m_attackZoneCheckInterval = 5.0f;
    private float m_attackZoneTimer = 0.0f;
    private float m_strafeZoneCheckInterval = 2.0f;
    private float m_strafeZoneTimer = 0.0f;

    // Vision Detection Relevant Variables
    [Header("Player Detection Values")]
    [SerializeField]
    [Tooltip("Whether the AI can detect the player. For debugging")]
    private bool m_playerDetectionEnabled = true;
    [SerializeField]
    [Tooltip("The range that the AI can detect the player")]
    private float m_viewRadius = 7.5f;
    [SerializeField]
    [Range(0.0f, 360.0f)]
    [Tooltip("The angle that the AI can detect the player AKA field of view")]
    private float m_viewAngle = 145.0f;

    [SerializeField]
    [Tooltip("The layer mask for obstacles")]
    private LayerMask m_obstacleMask;
    [SerializeField]
    [Tooltip("The layer mask for AI")]
    private LayerMask m_aiMask;

    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();

        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerCollider = m_player.GetComponent<Collider>();
        m_weaponCollider = m_weapon.GetComponent<BoxCollider>();

        DisableCollision();

        if (m_spawnAsleep)
        {
            SetAIState(AIState.Sleeping);
        }

        if (m_wakeTriggerObj != null)
        {
            m_wakeTrigger = m_wakeTriggerObj.GetComponent<BoxCollider>();
        }

        SetAIState(m_mainState);
    }

    private void Update()
    {
        // Finding the attack zone the AI is currently in
        AttackZoneCheck();

        switch (m_mainState)
        {
            // Idle State
            case AIState.Idle:
            {
                if (IsPlayerVisible())
                {
                    // Disabled Detection in Idle for now
                    //SetAIState(AIState.Pursuing);
                }
                break;
            }
            case AIState.Sleeping:
            {
                // Check if player is in the wake trigger zone to wake up
                WakeTriggerCheck();
                break;
            }
            // Patrol Logic
            case AIState.Patrolling:
            {
                // Continue Patrol Update
                PatrolUpdate();

                // Start combat when AI sees player
                if (IsPlayerVisible())
                {
                    SetAIState(AIState.InCombat);
                }

                break;
            }
            case AIState.ReturningToPatrol:
            {
                // Carry on patrolling once previous point is reached
                if (HasReachedDestination())
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
                    // If reached the end of the patrol points, go to wait state
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    // Move to next patrol point in sequence
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
                    // If reached the end of the patrol points, go to wait state
                    if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    // Move to next patrol point in sequence
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

                // Using a timer to make the AI wait before patrolling the opposite direction
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
        // Condition to help space out attacks a bit more
        if (m_aiManager.CanAttack() && m_combatState != CombatState.Pursuing && m_currentAttackingType == AttackingType.Active)
        {
            m_timeSinceLastAttack += Time.deltaTime;
        }

        switch (m_combatState)
        {
            // Chase after target/player
            case CombatState.Pursuing:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Checking if within the overall zone range, and if they've been assigned an attacking type yet
                if (DistanceSqrCheck(m_player, m_aiManager.GetPassiveAttackerMaxDist()) && m_currentAttackingType == AttackingType.Unassigned)
                {
                    // If there's space for active attackers, become active
                    if(m_aiManager.ActiveSlotsOpen())
                    {
                        m_aiManager.MakeActiveAttacker(this);
                        m_currentAttackingType = AttackingType.Active;
                    }
                    // Else become passive
                    else
                    {
                        m_aiManager.MakePassiveAttacker(this);
                        m_currentAttackingType = AttackingType.Passive;
                    }

                    RandomiseStrafeRange();
                }

                // Checking if they've reached the strafe range yet
                if (IsInStrafeRange() && m_currentAttackingType != AttackingType.Unassigned)
                {
                    if (!m_attackZoneManager.AreZonesAvailable(GetZoneTypeFromAttackType()))
                    {
                        SetCombatState(CombatState.MaintainDist);
                        return;
                    }

                    AttackZone currentAttackZone = m_attackZoneManager.FindAttackZone(this);

                    // If the current zone exists and isn't occupied by another AI
                    if (currentAttackZone != null && currentAttackZone.IsAvailable())
                    {
                        // Set to maintain distance, then set this zone as the occupied zone
                        SetCombatState(CombatState.MaintainDist);
                        SetAttackZone(currentAttackZone);

                        OccupyCurrentZone();
                    }
                    // Carry on radial running to a zone
                    else
                    {
                        SetCombatState(CombatState.RadialRunToZone);
                    }
                }
                break;
            }
            // Strafe around the player
            case CombatState.Strafing:
            {
                // Timed check to determine whether the AI should try and occupy the zone they're currently in
                TimedAttackZoneCheck();

                // Function to continue strafing
                Strafe();

                AiToPlayerRangeCheck();
                AttackCheck();
                break;
            }
            // Strafe with the intention of finding a zone to occupy
            case CombatState.StrafingToZone:
            {
                Strafe();

                AiToPlayerRangeCheck();

                // Radial obstruction check looks for other AI to avoid bumping into
                RadialObstructionCheck();
                StrafeZoneCheck();
                AttackCheck();
                break;
            }
            // Running in circle to find zone
            case CombatState.RadialRunToZone:
            {
                RadialRun();

                AiToPlayerRangeCheck();

                RadialObstructionCheck();
                RadialZoneCheck();
                AttackCheck();
                break;
            }
            // Maintain the current distance between AI and player
            case CombatState.MaintainDist:
            {
                TimedAttackZoneCheck();

                transform.LookAt(new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z));

                AiToPlayerRangeCheck();
                TimedBeginStrafeCheck();
                AttackCheck();
                break;
            }
            // Close the distance between AI and player
            case CombatState.ClosingDist:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                AiToPlayerRangeCheck();

                if (DistanceSqrCheck(m_player, m_strafeDist))
                {
                    StrafeOrMaintain();
                }

                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            // Increase the distance between AI and player
            case CombatState.BackingUp:
            {
                if (!DistanceSqrCheck(m_player, m_strafeDist))
                {
                    StrafeOrMaintain();
                    return;
                }

                BackUp();
                AiToPlayerRangeCheck();
                transform.LookAt(new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z));
                //transform.LookAt(m_player.transform.position);


                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            // Begin moving in to range for an attack
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Once in range, begin attack
                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.Attacking);
                }
                break;
            }
            // Moving to a specific zone
            case CombatState.MovingToZone:
            {
                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.MaintainDist);
                    Debug.Log("AI: " + name + " reached destination.");
                }
                break;
            }
            // Currently in attack animation
            case CombatState.Attacking:
            {
                transform.LookAt(new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z));
                //transform.LookAt(m_player.transform.position);
                break;
            }
        }
    }

    public void SetAIState( AIState stateToSet )
    {
        // If changing FROM patrol state, store the last position in the patrol route
        if (m_mainState == AIState.Patrolling)
        {
            m_lastPointOnPatrol = gameObject.transform.position;
        }

        m_mainState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            // Idle State
            case AIState.Idle:
            {
                StartIdleAnim();
                break;
            }
            // Sleep State
            case AIState.Sleeping:
            {
                SetToPlayDeadAnim();
                break;
            }
            // Patrol State
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
            // Return to Patrol State
            case AIState.ReturningToPatrol:
            {
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
                m_navMeshAgent.autoBraking = false;
                StartWalkAnim();
                break;
            }
            // Combat State
            case AIState.InCombat:
            {

                // Registering the enemy as an attacker with the manager
                m_aiManager.RegisterAttacker(this);
                SetCombatState(CombatState.Pursuing);
                ResetAttackTimer();

                break;
            }
            // Death State
            case AIState.Dead:
            {
                StartDeathAnim();
                break;
            }
        }
    }

    private void SetPatrolState( PatrolState stateToSet )
    {
        m_patrolState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            // Patrol in normal direction
            case PatrolState.Patrol:
            {
                StartWalkAnim();
                break;
            }
            // Patrol in reverse direction
            case PatrolState.ReversePatrol:
            {
                StartWalkAnim();
                break;
            }
            // Waiting to resume patrol
            case PatrolState.Waiting:
            {
                m_patrolTimer = 0.0f;
                StartIdleAnim();
                break;
            }
        }
    }

    public void SetCombatState( CombatState stateToSet )
    {
        m_combatState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            // Pursue Player
            case CombatState.Pursuing:
            {
                m_attackZonePos = m_attackZoneManager.RandomiseAttackPosForEnemy(this);

                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
                m_navMeshAgent.autoBraking = true;
                RandomiseStrafeRange();
                StartRunAnim();
                break;
            }
            // Strafe around player
            case CombatState.Strafing:
            {
                // Randomise the direction to strafe
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            // Strafe around player in search of empty zone
            case CombatState.StrafingToZone:
            {
                // Randomise the direction to strafe
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            // Radial run around player in search of zone
            case CombatState.RadialRunToZone:
            {
                // Randomise the direction to run
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartRunAnim();
                break;
            }
            // Maintain current distance to player
            case CombatState.MaintainDist:
            {
                // Randomise delay before moving again
                m_timeUntilStrafe = Random.Range(m_minDelayBeforeStrafe, m_maxDelayBeforeStrafe);
                StartCombatIdleAnim();
                break;
            }
            // Attack player
            case CombatState.Attacking:
            {
                StartAttackAnim();
                break;
            }
            // Move directly to a specified zone
            case CombatState.MovingToZone:
            {
                StartRunAnim();
                break;
            }
            // Get in range of player for attack
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;
                StartRunAnim();
                break;
            }
            // Close distance to player
            case CombatState.ClosingDist:
            {
                RandomiseStrafeRange();
                StartWalkAnim();
                break;
            }
            // Increase distance to player
            case CombatState.BackingUp:
            {
                RandomiseStrafeRange();
                StartWalkBackAnim();
                break;
            }
        }
    }

    private void RadialRun()
    {
        Vector3 offset;

        // Determining nav mesh target based on strafe dir
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
    }

    private void Strafe()
    {
        Vector3 offset;
        // Determining nav mesh target based on strafe dir
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

        // LookAt so that it looks like actual strafing
        transform.LookAt(new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z));
        //transform.LookAt(m_player.transform.position);
    }

    private void BackUp()
    {
        Vector3 dir = (transform.position - m_player.transform.position).normalized;
        m_navMeshAgent.SetDestination(transform.position + (dir * 2.0f));

        // Keep facing player while back stepping
        transform.LookAt(new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z));
        //transform.LookAt(m_player.transform.position);
    }

    // Function for detecting if the zone the AI is about to enter is obstructed
    // Not currently in use, but will be implemented later
    private bool AdjacentZoneIsAvailable()
    {
        bool zoneAvailable = false;
        int nextZoneNum;

        // Getting num of the next zone based on strafe dir
        if (m_strafeDir == StrafeDir.Left)
        {
            nextZoneNum = m_currentAttackZone.GetZoneNum() + 1 % m_attackZoneManager.GetTotalZonesNum();
        }
        else
        {
            nextZoneNum = m_currentAttackZone.GetZoneNum() - 1;

            if (nextZoneNum < 0)
            {
                nextZoneNum = m_attackZoneManager.GetTotalZonesNum() - 1;
            }
        }

        // Checking if target zone is available
        if (m_currentAttackingType == AttackingType.Passive)
        {
            if (!m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Passive).IsObstructed() &&
                !m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Passive).IsOccupied())
            {
                zoneAvailable = true;
            }

        }
        else
        {
            if (!m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Active).IsObstructed() &&
                !m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Active).IsOccupied())
            {
                zoneAvailable = true;
            }

        }

        return zoneAvailable;
    }

    private void AiToPlayerRangeCheck()
    {
        float maxStrafeRange = 0.0f;
        float minStrafeRange = 0.0f;

        // Passive range check
        if (m_currentAttackingType == AttackingType.Passive)
        {
            maxStrafeRange = m_aiManager.GetPassiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMaxDist();

            // If enemy is too close to the player, tell the AI manager to make this AI an active attacker, and swap the furthest active attacker to a passive attacker
            if ( DistanceSqrCheck( m_player, m_aiManager.GetActiveAttackerMinDist() ) )
            {
                m_aiManager.SwapPassiveWithActive(this);
            }
        }
        // Active range check
        else
        {
            maxStrafeRange = m_aiManager.GetActiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMinDist();
        }

        // AI is out of zone, empty zone, resume pursuit
        if (!DistanceSqrCheck(m_player, maxStrafeRange))
        {
            if (m_occupiedAttackZone != null)
            {
                m_occupiedAttackZone.EmptyZone();
            }
            if (m_currentAttackingType == AttackingType.Passive)
            {
                m_aiManager.MakeUnasssignedAttacker(this);
                m_currentAttackingType = AttackingType.Unassigned;
            }
            SetCombatState(CombatState.Pursuing);
        }
        // Player moved closer than strafe range
        // Empty zone, then back up
        // Using minStrafeRange - (minStrafeRange * 0.25f) to act as a buffer for preventing the AI backing up prematurely
        // Todo: Could use a rework for the buffer logic, perhaps a member variable?
        if (DistanceSqrCheck(m_player, minStrafeRange - (minStrafeRange * 0.25f)) && m_combatState != CombatState.BackingUp)
        {
            if (m_occupiedAttackZone != null)
            {
                m_occupiedAttackZone.EmptyZone();
            }
            SetCombatState(CombatState.BackingUp);
        }
    }

    private void AttackCheck()
    {
        // Checking if it's been long enough to attack, if the current AI is an active attacker, and checking with the AI manager if attacking is allowed currently
        if (m_timeSinceLastAttack >= m_attackTimer && m_aiManager.CanAttack() && m_attackEnabled && m_currentAttackingType == AttackingType.Active)
        {
            SetCombatState(CombatState.MovingToAttack);

            // Disabled for now since control of it has been handed to AI manager
            //m_aiManager.SetCanAttack(false);
        }
    }

    private void StrafeOrMaintain()
    {
        // Decide whether to strafe or maintain distance based on whether the zone is the currently occupied zone
        if (m_currentAttackZone == m_occupiedAttackZone || !m_attackZoneManager.AreZonesAvailable(GetZoneTypeFromAttackType()))
        {
            SetCombatState(CombatState.MaintainDist);
        }
        else
        {
            m_combatState = CombatState.RadialRunToZone;
            StartRunAnim();
        }
    }

    private void RandomiseStrafeRange()
    {
        // Randomise the range for the AI to maintain based on attacker type
        if (m_currentAttackingType == AttackingType.Passive)
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist()) + m_aiManager.GetActiveAttackerMinDist();
        }
        else
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist());
        }
    }

    private bool HasReachedDestination()
    {
        bool destinationReached = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (!m_navMeshAgent.pathPending)
        {
            if (m_navMeshAgent.remainingDistance < m_navMeshAgent.stoppingDistance)
            {
                destinationReached = true;
            }
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
        return DistanceSqrCheck(m_player, m_strafeDist);
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

        if (m_weaponCollider.bounds.Intersects(m_playerCollider.bounds) && m_weaponCollider.enabled)
        {
            isColliding = true;
            m_weaponCollider.enabled = false;
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

    // Overloaded DirFromAngle to allow getting the direction from a specified object's position
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject dirFromObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += dirFromObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public bool IsPlayerVisible()
    {
        bool playerIsVisible = false;

        // If in combat, just return true since no point redoing detection
        // Will need changing if de-aggro functionality is implemented
        if (m_mainState == AIState.InCombat)
        {
            return true;
        }

        if (m_playerDetectionEnabled)
        {
            // Checking if player is in range
            if (DistanceSqrCheck(m_player, m_viewRadius))
            {
                // Once player is in range, getting the direction to the player and checking if it's within the AI's FOV
                Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToPlayer) < m_viewAngle * 0.5f)
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

    // Function for optimally checking a target is within a given distance
    private bool DistanceSqrCheck(GameObject targetToCheck, float distanceToCheck)
    {
        bool isInRange = false;

        // Getting the distance between this and the target
        Vector3 distance = transform.position - targetToCheck.transform.position;

        // Checking if sqrMagnitude is less than the distance squared
        if (distance.sqrMagnitude <= distanceToCheck * distanceToCheck)
        {
            isInRange = true;
        }

        return isInRange;
    }

    // Function for returning distance in float between target
    private float DistanceSqrValue( GameObject targetToCheck )
    {
        return (transform.position - targetToCheck.transform.position).sqrMagnitude;
    }

    // Todo: Rename this function or TimedAttackZoneCheck() to be clearer
    private void AttackZoneCheck()
    {
        m_currentAttackZone = m_attackZoneManager.FindAttackZone(this);
    }

    private void TimedAttackZoneCheck()
    {
        if (!m_attackZoneManager.AreZonesAvailable(GetZoneTypeFromAttackType()))
        {
            return;
        }

        m_attackZoneTimer += Time.deltaTime;

        if (m_attackZoneTimer >= m_attackZoneCheckInterval)
        {
            m_attackZoneTimer = 0.0f;

            if (m_currentAttackZone != m_occupiedAttackZone)
            {
                // If in the active zone, but not an active attacker, back up
                if (m_currentAttackZone.GetZoneType() == ZoneType.Active && m_currentAttackingType != AttackingType.Active)
                {
                    SetCombatState(CombatState.BackingUp);
                    return;
                }
                // If in a passive zone, but an active attacker, close the distance
                else if (m_currentAttackZone.GetZoneType() == ZoneType.Passive && m_currentAttackingType != AttackingType.Passive)
                {
                    SetCombatState(CombatState.ClosingDist);
                    return;
                }
                // If in a zone
                if (m_currentAttackZone != null)
                {
                    // If zone is not occupied
                    if (!m_currentAttackZone.IsOccupied())
                    {
                        OccupyCurrentZone();
                    }
                    else
                    {
                        // Simple code for now to randomise whether an AI can force another AI out of zone
                        // Todo: Refactor
                        int takeoverChance = Random.Range(0, 2);
                        if (takeoverChance > 0)
                        {
                            TakeOverOccupiedZone();
                        }
                        else
                        {
                            SetCombatState(CombatState.StrafingToZone);
                        }
                    }
                }
                else
                {
                    SetCombatState(CombatState.StrafingToZone);
                }
            }
        }
    }

    // Function to check if AI should start strafing to mimic more lifelike behaviour
    private void TimedBeginStrafeCheck()
    {
        if (m_attackZoneManager.AreZonesAvailable(GetZoneTypeFromAttackType()))
        {
            m_delayBeforeStrafe += Time.deltaTime;

            if ( m_delayBeforeStrafe > m_timeUntilStrafe )
            {
                SetCombatState(CombatState.StrafingToZone);
                m_delayBeforeStrafe = 0.0f;
            }
        }
    }

    // Check while strafing whether to occupy current zone
    // Based on a timer to allow variation on when the AI stops
    private void StrafeZoneCheck()
    {
        if (!m_attackZoneManager.AreZonesAvailable(GetZoneTypeFromAttackType()))
        {
            return;
        }

        m_strafeZoneTimer += Time.deltaTime;

        if (m_strafeZoneTimer >= m_strafeZoneCheckInterval)
        {
            m_strafeZoneTimer = 0.0f;

            if (m_currentAttackZone != null)               
            {
                if (m_currentAttackZone != m_occupiedAttackZone)
                {
                    if (!m_currentAttackZone.IsOccupied())
                    {
                        OccupyCurrentZone();
                        SetCombatState(CombatState.MaintainDist);
                    }
                }
            }
        }
    }

    // Checking for obstructions during strafing/radial running
    private void RadialObstructionCheck()
    {
        Vector3 dir = transform.forward;
        Vector3 castFrom = transform.position;
        castFrom.y += m_navMeshAgent.height * 0.5f;

        // If strafing, setup directions to be from sides
        if (m_combatState == CombatState.StrafingToZone)
        {
            if (m_strafeDir == StrafeDir.Left)
            {
                dir = -transform.right;
            }
            else
            {
                dir = transform.right;
            }
        }

        GameObject enemyToCheck = FindClosestEnemy().gameObject;

        if (DistanceSqrCheck(enemyToCheck, m_checkForAIDist))
        {
            Vector3 dirToCheck;

            if (m_strafeDir == StrafeDir.Right)
            {
                dirToCheck = transform.right;
            }
            else
            {
                dirToCheck = -transform.right;
            }

            Vector3 dirToEnemy = (enemyToCheck.transform.position - transform.position).normalized;
            if (Vector3.Angle(dirToCheck, dirToEnemy) < m_checkForAIAngles * 0.5f)
            {
                float currentZoneHalfDist = 0.0f;

                // Finding the distance to compare with the current strafe distance to determine whether the AI should move backwards or forwards
                if (m_currentAttackingType == AttackingType.Passive)
                {
                    currentZoneHalfDist = m_aiManager.GetActiveAttackerMaxDist() + ((m_aiManager.GetPassiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMaxDist()) * 0.5f);
                }
                else
                {
                    currentZoneHalfDist = m_aiManager.GetActiveAttackerMinDist() + ((m_aiManager.GetActiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMinDist()) * 0.5f);
                }

                if (m_strafeDist > currentZoneHalfDist)
                {
                    StartWalkAnim();
                    m_strafeDist -= m_AIAvoidanceDist;
                    m_combatState = CombatState.ClosingDist;
                }
                else
                {
                    StartWalkBackAnim();
                    m_strafeDist += m_AIAvoidanceDist;
                    m_combatState = CombatState.BackingUp;
                }
            }
        }
    }

    private EnemyAI FindClosestEnemy()
    {
        EnemyAI closestEnemy = m_aiManager.GetEnemyList()[0];
		
		if (closestEnemy == this)
		{
			closestEnemy = m_aiManager.GetEnemyList()[1];
		}
		
        foreach (EnemyAI enemy in m_aiManager.GetEnemyList())
        {
            if (enemy != this && enemy.gameObject.activeSelf && enemy.GetState() == AIState.InCombat)
            {
                if (DistanceSqrValue(enemy.gameObject) < DistanceSqrValue(closestEnemy.gameObject))
                {
                    closestEnemy = enemy;                    
                }
            }
        }

        Debug.Log("Closest AI to " + name + " is " + DistanceSqrValue(closestEnemy.gameObject));
        return closestEnemy;
    }

    // Checking if zone is available to occupy whilst radial running
    private void RadialZoneCheck()
    {
        if (m_currentAttackZone != null)
        {
            if (m_currentAttackZone != m_occupiedAttackZone)
            {
                if (!m_currentAttackZone.IsOccupied())
                {
                    m_combatState = CombatState.StrafingToZone;
                    StartStrafeAnim(m_strafeDir);
                }
            }
        }
    }

    private void OccupyCurrentZone()
    {
        // Empty previously occupied zone
        if (m_occupiedAttackZone != null)
        {
            m_occupiedAttackZone.EmptyZone();
        }

        // Occupy current zone
        m_occupiedAttackZone = m_currentAttackZone;
        m_occupiedAttackZone.SetOccupant(this);
    }

    private void TakeOverOccupiedZone()
    {
        EnemyAI currentOccupant = m_currentAttackZone.GetOccupant();

        currentOccupant.ClearOccupiedZone();
        currentOccupant.SetCombatState(CombatState.StrafingToZone);
        OccupyCurrentZone();
        //Debug.Log("AI " + name + " took zone from " + currentOccupant.name);
    }

    public void ClearOccupiedZone()
    {
        m_occupiedAttackZone.EmptyZone();
        m_occupiedAttackZone = null;
    }

    public bool IsCurrentZoneObstructed()
    {
       return m_currentAttackZone.IsAvailable();
    }

    private bool IsInAssignedZone()
    {
        return m_currentAttackZone == m_occupiedAttackZone;
    }

    // Wake up AI if player is detected in trigger zone
    private void WakeTriggerCheck()
    {
        if( m_wakeTrigger != null)
        {
            if (m_wakeTrigger.bounds.Intersects(m_playerCollider.bounds))
            {
                WakeUpAI(WakeTrigger.Standard);
            }
        }
    }

    // Wake up differently based on wake up type, i.e. hit by player vs. detected player
    public void WakeUpAI( WakeTrigger wakeTrigger )
    {
        switch (wakeTrigger)
        {
            case WakeTrigger.Attack:
            {
                SetAIState(AIState.Waking);
                StartStandUpAnim();
                break;
            }
            case WakeTrigger.Standard:
            {
                SetAIState(AIState.Waking);
                StartStandUpAnim();
                break;
            }
        }
    }

    public ZoneType GetZoneTypeFromAttackType()
    {
        if(m_currentAttackingType == AttackingType.Passive)
        {
            return ZoneType.Passive;
        }
        else if (m_currentAttackingType == AttackingType.Active)
        {
            return ZoneType.Active;
        }
        else
        {
            Debug.Log("Error: Unassigned AI trying to find ZoneType");
            return ZoneType.None;
        }
    }
    public AIState GetState()
    {
        return m_mainState;
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
        return m_strafeDist;
    }

    public float GetAIAngleCheck()
    {
        return m_checkForAIAngles;
    }

    public float GetAgentHeight()
    {
        return m_navMeshAgent.height;
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
        // Todo: Maybe remove, function has become redundant
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.InCombat);
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
        ResetAttackTimer();

        // Telling the AI manager that the attack is over and other AI can attack again
        // Very basic currently, and will be expanded upon in the future
        // Disabled for now since control of it is being handled by AI manager
        //m_aiManager.SetCanAttack(true);
    }

    private void ResetAttackTimer()
    {
        m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
        m_timeSinceLastAttack = 0.0f;
    }

    public void TakeDamage( float damageToTake )
    {
        if (m_mainState != AIState.Dead)
        {
            m_stateBeforeHit = m_mainState;

            m_health -= damageToTake;

            if (m_mainState != AIState.Sleeping)
            {
                PlayDamageAnim();
            }

            if (m_health <= 0.0f)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        m_health = 0.0f;
        SetAIState(AIState.Dead);
        m_aiManager.UnregisterAttacker(this);
    }

    public void ChangeStateFromWake()
    {
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.InCombat);

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

    public void SetAIManagerRef( AIManager aiManagerRef )
    {
        m_aiManager = aiManagerRef;
    }

    public void SetAttackZoneManagerRef( AttackZoneManager attackZoneManager )
    {
        m_attackZoneManager = attackZoneManager;
    }

    public AttackingType GetAttackingType()
    {
        return m_currentAttackingType;
    }

    public void SetAttackingType( AttackingType typeToSet )
    {
        m_currentAttackingType = typeToSet;
    }

    public void SetStrafeDist( float distance )
    {
        m_strafeDist = distance;
    }

    public StrafeDir GetStrafeDir()
    {
        return m_strafeDir;
    }

    public float GetAICheckDist()
    {
        return m_checkForAIDist;
    }

    public AttackZone GetAttackZone()
    {
        return m_currentAttackZone;
    }

    public void SetAttackZone( AttackZone zoneToSet )
    {
        m_currentAttackZone = zoneToSet;
    }

    public AttackZone GetOccupiedAttackZone()
    {
        return m_occupiedAttackZone;
    }

    public void SetOccupiedAttackZone( AttackZone zoneToSet )
    {
        m_occupiedAttackZone = zoneToSet;
    }
}
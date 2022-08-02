using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//*******************************************
// Author: Dean Pearce
// Class: EnemyAI
// Description: Base enemy AI class which handles navigation, behaviour, and animation
//*******************************************

public enum EnemyType
{
    Grunt,
    Guard
}

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
    Attacking,
    Dodging
}

public enum AttackingType
{
    Unassigned,
    Passive,
    Active
}

public enum AttackMode
{
    Primary,
    Secondary,
    Both
}

public enum StrafeDir
{
    Left,
    Right
}

public class EnemyAI : MonoBehaviour
{
    private AIManager m_aiManager;
    private EnemySoundHandler m_soundHandler;

    private NavMeshAgent m_navMeshAgent;
    private int m_spawnGroup = 0;
    [SerializeField]
    private EnemyType m_enemyType = EnemyType.Grunt;
    [SerializeField]
    [Tooltip("AI's Current State")]
    private AIState m_mainState = AIState.Idle;
    private CombatState m_combatState = CombatState.Strafing;
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
    private PlayerController m_playerController;
    private Collider m_playerCollider;

    // Combat Relevant Variables
    [Header("Combat Values")]
    private bool m_lookAtPlayer = false;
    [SerializeField]
    [Tooltip("The speed the AI will rotate when attempting to look at a target")]
    private float m_turnSpeed = 75.0f;
    // Todo: Rename and re-do description for m_rotationBuffer
    [SerializeField]
    [Tooltip("The difference from current rotation to target before the AI will lock rotation")]
    private float m_rotationBuffer = 5.0f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop")]
    private float m_playerStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on normal attack")]
    private float m_normalAttkStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on quick attack")]
    private float m_quickAttkStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on heavy attack")]
    private float m_heavyAttkStoppingDistance = 2.0f;
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
    [SerializeField]
    [Tooltip("The distance the AI will check for other obstructing AI during combat")]
    private float m_checkForAIDist = 2.1f;
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
    [Tooltip("Number of different attacks the AI will use")]
    private int m_attackNum = 3;
    [SerializeField]
    [Tooltip("The primary weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_primaryWeapon;
    private BoxCollider m_primaryWeaponCollider;
    [SerializeField]
    [Tooltip("The secondary weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_secondaryWeapon;
    private BoxCollider m_secondaryWeaponCollider;
    private AttackMode m_attackMode = AttackMode.Primary;
    private AttackingType m_currentAttackingType = AttackingType.Passive;
    private ZoneHandler m_zoneHandler = new ZoneHandler();
    private float m_zoneCheckInterval = 5.0f;
    private float m_zoneTimer = 0.0f;
    private float m_strafeCheckInterval = 2.0f;
    private float m_strafeTimer = 0.0f;
    private bool m_isStaggered = false;

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

    [Header("Animation Values")]
    [SerializeField]
    [Tooltip("Total number of sleep to wake animations")]
    private int m_sleepToWakeAnimNum = 2;
    private int[] an_sleepToWakeHashes;
    [SerializeField]
    [Tooltip("Total number of dodge animations")]
    private int m_dodgeAnimNum = 2;
    private int[] an_dodgeHashes;
    private int m_lastUsedAnimTrigger;

    [SerializeField]
    [Tooltip("The layer mask for obstacles")]
    private LayerMask m_obstacleMask;
    [SerializeField]
    [Tooltip("The layer mask for AI")]
    private LayerMask m_aiMask;

    //String to Hash stuff
    private int an_triggerNone = 0;
    private int an_walk;
    private int an_walkBack;
    private int an_strafeRight;
    private int an_strafeLeft;
    private int an_run;
    private int an_idle;
    private int an_combatIdle;
    private int an_attack;
    private int an_quickAttack;
    private int an_heavyAttack;
    //private int an_dodge0;
    //private int an_dodge1;
    //private int an_sleepToWake0;
    //private int an_sleepToWake1;
    private int an_sleep;
    private int an_death;
    private int an_takeHit;
    private int an_weaken;

    //Health Manager Component
    private CharacterDamageManager m_healthManager;

    private void Awake()
    {
        SetupStringToHashes();

        m_healthManager = GetComponent<CharacterDamageManager>();

        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();
        m_soundHandler = GetComponent<EnemySoundHandler>();

        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerController = m_player.GetComponent<PlayerController>();
        m_playerCollider = m_player.GetComponent<Collider>();
        m_primaryWeaponCollider = m_primaryWeapon.GetComponent<BoxCollider>();
        m_secondaryWeaponCollider = m_secondaryWeapon.GetComponent<BoxCollider>();

        DisableCollision();

        if (m_mainState == AIState.Sleeping)
        {
            // So if sleeping, can't get hurt
            // Todo: Review this, should they be invuln while sleeping?
            m_healthManager.SetInvulnerable( true );
        }

        SetAIState(m_mainState);


        m_lastUsedAnimTrigger = an_triggerNone;
    }

    private void Update()
    {
        if (m_lookAtPlayer)
        {
            TurnToLookAt(m_player.gameObject);
        }

        switch (m_mainState)
        {
            // Idle State
            case AIState.Idle:
            {
                if (IsPlayerVisible())
                {
                    //SetAIState(AIState.InCombat);
                }
                break;
            }
            case AIState.Sleeping:
            {
                break;
            }
            case AIState.Waking:
            {
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
            //Debug.Log("There is no patrol route attached to the AI. Please attach one.");
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
        // Update zone handler to track status of zones
        m_zoneHandler.Update();

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
                    SetupAttackingType();

                    RandomiseStrafeRange();
                }

                // Checking if they've reached the strafe range yet
                if (IsInStrafeRange() && m_currentAttackingType != AttackingType.Unassigned)
                {
                    if (!m_zoneHandler.AreZonesAvailable())
                    {
                        SetCombatState(CombatState.MaintainDist);
                        return;
                    }

                    // If the current zone exists and isn't occupied by another AI
                    if (m_zoneHandler.IsZoneAvailable())
                    {
                        // Set to maintain distance, then set this zone as the occupied zone
                        SetCombatState(CombatState.MaintainDist);

                        m_zoneHandler.OccupyCurrentZone();
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
                TimedZoneCheck();

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
                TimedZoneCheck();             

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
                m_navMeshAgent.destination = m_zoneHandler.GetReservedPos();

                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.MaintainDist);
                    m_zoneHandler.UnreserveZone();
                    m_zoneHandler.OccupyCurrentZone();
                    //Debug.Log("AI: " + name + " reached destination.");
                }
                break;
            }
            // Currently in attack animation
            case CombatState.Attacking:
            {
                // Attack hits
                if (IsAttackCollidingWithPlayer())
                {
                    // Todo: Bad place for triggering the sound, should be done directly from player
                    m_playerController.GetSoundHandler().PlayDamageSFX();

                    m_player.GetComponent<CharacterDamageManager>().TakeDamage(transform);
                    m_soundHandler.PlayNormalCollisionSFX();
                    DisableCollision();
                }
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
        ResetLastUsedAnimTrigger();

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
                //SetCombatState(CombatState.Pursuing);

                SetupAttackingType();
                RandomiseStrafeRange();
                m_zoneHandler.ReserveClosestZone();
                SetCombatState(CombatState.MovingToZone);

                ResetAttackTimer();

                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;

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
        ResetLastUsedAnimTrigger();

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
        ResetLastUsedAnimTrigger();

        switch (stateToSet)
        {
            // Pursue Player
            case CombatState.Pursuing:
            {
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
                m_zoneHandler.OccupyCurrentZone();
                StartCombatIdleAnim();
                break;
            }
            // Attack player
            case CombatState.Attacking:
            {
                Attack();
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
                m_attackMode = (AttackMode)Random.Range(0, m_attackNum);

                switch(m_attackMode)
                {
                    case AttackMode.Primary:
                    {
                        m_navMeshAgent.stoppingDistance = m_normalAttkStoppingDistance;
                        break;
                    }
                    case AttackMode.Both:
                    {
                        m_navMeshAgent.stoppingDistance = m_quickAttkStoppingDistance;
                        break;
                    }
                    case AttackMode.Secondary:
                    {
                        m_navMeshAgent.stoppingDistance = m_heavyAttkStoppingDistance;
                        break;
                    }
                }

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

    //********
    // Function:SetupStringToHashes
    // Author: Charlie Taylor
    // Description: Sets up variables for string to int hashes used by the animation controller
    // Parameters: None
    // Returns: None
    //********
    private void SetupStringToHashes()
    {
        an_dodgeHashes = new int[m_dodgeAnimNum];
        an_sleepToWakeHashes = new int[m_sleepToWakeAnimNum];

        for(int i = 0; i < an_dodgeHashes.Length; i++)
        {
            an_dodgeHashes[i] = Animator.StringToHash("Dodge" + i);
        }

        for (int i = 0; i < an_dodgeHashes.Length; i++)
        {
            an_sleepToWakeHashes[i] = Animator.StringToHash("SleepToWake" + i);
        }

        //an_triggerNone = Animator.StringToHash( "None" );
        an_walk = Animator.StringToHash("Walk");
        an_walkBack = Animator.StringToHash("WalkBack");
        an_strafeRight = Animator.StringToHash("StrafeRight");
        an_strafeLeft = Animator.StringToHash("StrafeLeft");
        an_run = Animator.StringToHash("Run");
        an_idle = Animator.StringToHash("Idle");
        an_combatIdle = Animator.StringToHash("CombatIdle");
        an_attack = Animator.StringToHash("Attack");
        an_quickAttack = Animator.StringToHash("QuickAttack");
        an_heavyAttack = Animator.StringToHash("HeavyAttack");
        //an_dodge0 = Animator.StringToHash("Dodge0");
        //an_dodge1 = Animator.StringToHash("Dodge1");
        //an_sleepToWake0 = Animator.StringToHash("SleepToWake0");
        //an_sleepToWake1 = Animator.StringToHash("SleepToWake1");
        an_sleep = Animator.StringToHash("Sleep");
        an_death = Animator.StringToHash("Death");
        an_takeHit = Animator.StringToHash("TakeHit");
        an_weaken = Animator.StringToHash("Weaken");
    }

    private void SetupAttackingType()
    {
        // If there's space for active attackers, become active
        if (m_aiManager.ActiveSlotsOpen())
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
    }

    private void SetupPatrolRoutes()
    {
        if (m_patrolRoutePoints.Count > 0)
        {
            m_patrolRoutePoints.Clear();
        }

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

    public void SetupZoneHandler( ref AttackZoneManager attackZoneManager )
    {
        EnemyAI thisEnemy = this;
        m_zoneHandler.Init(ref thisEnemy, ref attackZoneManager);
    }

    public void StopNavMesh()
    {
        m_navMeshAgent.isStopped = true;
    }

    private void TurnToLookAt(GameObject targetObj)
    {
        // Getting dir from enemy to player
        Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
        float targetAngle = Vector3.SignedAngle(dirToPlayer, Vector3.forward, Vector3.down);
        float angleFrom = Vector3.SignedAngle(dirToPlayer, transform.forward, Vector3.down);

        Vector3 currentEulerAngles = transform.eulerAngles;

        // Wrapping angle back to 360
        if (targetAngle < 0.0f)
        {
            targetAngle = 360.0f - targetAngle * -1.0f;
        }

        // Todo: Redo this diff check, difference should never be more than 180
        float angleDiff = currentEulerAngles.y - targetAngle;

        if (angleDiff > m_rotationBuffer)
        {
            // Checking whether it's quicker to rotate clockwise or counter-clockwise
            if (angleFrom > 0)
            {
                currentEulerAngles.y += m_turnSpeed * Time.deltaTime;
            }
            else
            {
                currentEulerAngles.y -= m_turnSpeed * Time.deltaTime;
            }

            transform.eulerAngles = currentEulerAngles;
        }
        else
        {
            transform.LookAt(new Vector3(targetObj.transform.position.x, transform.position.y, targetObj.transform.position.z));
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
    }

    private void BackUp()
    {
        Vector3 dir = (transform.position - m_player.transform.position).normalized;
        m_navMeshAgent.SetDestination(transform.position + (dir * 2.0f));
    }

    private void Attack()
    {
        switch (m_attackMode)
        {
            case AttackMode.Primary:
            {
                StartAttackAnim();
                break;
            }
            case AttackMode.Both:
            {
                StartQuickAttackAnim();
                break;
            }
            case AttackMode.Secondary:
            {
                StartHeavyAttackAnim();
                break;
            }
        }
    }

    private void EndAttack()
    {
        SetCombatState(CombatState.BackingUp);
        ResetAttackTimer();

        m_navMeshAgent.speed = m_runSpeed;
        m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
    }

    private void ResetAttackTimer()
    {
        m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
        m_timeSinceLastAttack = 0.0f;
    }

    private void RecoverFromHit()
    {
        SetCombatState(CombatState.Pursuing);
        SetStaggered(false);
    }

    /*
    public void TakeDamage( float damageToTake )
    {
        if (m_mainState != AIState.Dead)
        {
            m_health -= damageToTake;

            if (m_mainState != AIState.Sleeping)
            {
                ResetLastUsedAnimTrigger();
                //PlayDamageAnim();
            }

            if (m_health <= 0.0f)
            {
                Die();
            }
        }
    }*/

    /*
    private void Die()
    {
        m_health = 0.0f;
        SetAIState(AIState.Dead);
        m_aiManager.UnregisterAttacker(this);
    }
    */

    public void UnregisterAttacker()
    {
        m_aiManager.UnregisterAttacker(this);
    }

    public void ChangeStateFromWake()
    {
        SetAIState(AIState.InCombat);

        // Had to put this setter here to force path recalculation, otherwise AI would attack immediately.
        m_navMeshAgent.SetDestination(m_player.transform.position);
        m_lookAtPlayer = false;

        m_healthManager.SetInvulnerable(false);
    }

    public void ResetLastUsedAnimTrigger()
    {
        if (m_lastUsedAnimTrigger != an_triggerNone)
        {
            m_animController.ResetTrigger(m_lastUsedAnimTrigger);
        }
    }

    private void ResetAllAnimTriggers()
    {
        m_animController.ResetTrigger(an_walk);
        m_animController.ResetTrigger(an_idle);
        m_animController.ResetTrigger(an_attack);
        m_animController.ResetTrigger(an_quickAttack);
        m_animController.ResetTrigger(an_heavyAttack);
        m_animController.ResetTrigger(an_run);
        //m_animController.ResetTrigger(an_sleepToWake0);
        //m_animController.ResetTrigger(an_sleepToWake1);
        m_animController.ResetTrigger(an_sleep);
        m_animController.ResetTrigger(an_takeHit);
        m_animController.ResetTrigger(an_strafeLeft);
        m_animController.ResetTrigger(an_strafeRight);
        m_animController.ResetTrigger(an_combatIdle);
        m_animController.ResetTrigger(an_walkBack);
        m_animController.ResetTrigger(an_death);
        m_animController.ResetTrigger(an_weaken);
        //m_animController.ResetTrigger(an_dodge0);
        //m_animController.ResetTrigger(an_dodge1);

        foreach(int trigger in an_dodgeHashes)
        {
            m_animController.ResetTrigger(trigger);
        }

        foreach (int trigger in an_sleepToWakeHashes)
        {
            m_animController.ResetTrigger(trigger);
        }
    }

    public void ResetToSpawn()
    {
        m_lastUsedAnimTrigger = an_triggerNone;
        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();
        DisableCollision();

        // Todo: Health Manager reset to go here
    }

    public void WakeUpAI()
    {
        SetAIState(AIState.Waking);
        StartSleepToWakeAnim();
    }

    public void LookAtPlayerOnWake()
    {
        m_lookAtPlayer = true;
    }

    public void DisableCollision()
    {
        m_primaryWeaponCollider.enabled = false;
        m_secondaryWeaponCollider.enabled = false;
    }

    private void EnableCollision()
    {
        switch (m_attackMode)
        {
            case AttackMode.Primary:
            {
                m_primaryWeaponCollider.enabled = true;

                break;
            }
            case AttackMode.Secondary:
            {
                m_secondaryWeaponCollider.enabled = true;

                break;
            }
            case AttackMode.Both:
            {
                m_primaryWeaponCollider.enabled = true;
                m_secondaryWeaponCollider.enabled = true;

                break;
            }
        }
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
            if (m_currentAttackingType == AttackingType.Passive)
            {
                m_aiManager.MakeUnasssignedAttacker(this);
                m_currentAttackingType = AttackingType.Unassigned;
            }
            SetCombatState(CombatState.Pursuing);
        }
        // Player moved closer than strafe range
        // Empty zone, then back up

        //** Old if condition, might not work the way originally intended, leaving commented incase needed again
        // Using minStrafeRange - (minStrafeRange * 0.25f) to act as a buffer for preventing the AI backing up prematurely
        // Todo: Could use a rework for the buffer logic, perhaps a member variable?
        //if (DistanceSqrCheck(m_player, minStrafeRange - (minStrafeRange * 0.25f)) && m_combatState != CombatState.BackingUp)


        if (DistanceSqrCheck(m_player, minStrafeRange) && m_combatState != CombatState.BackingUp)
        {
            SetCombatState(CombatState.BackingUp);
        }
    }

    private void AttackCheck()
    {
        // Checking if it's been long enough to attack, if the current AI is an active attacker, and checking with the AI manager if attacking is allowed currently
        if (m_timeSinceLastAttack >= m_attackTimer && m_aiManager.CanAttack() && m_attackEnabled && m_currentAttackingType == AttackingType.Active)
        {
            SetCombatState(CombatState.MovingToAttack);
        }
    }

    private void StrafeOrMaintain()
    {
        // Decide whether to strafe or maintain distance based on whether the zone is the currently occupied zone
        if (m_zoneHandler.IsInValidZone() || !m_zoneHandler.AreZonesAvailable())
        {
            SetCombatState(CombatState.MaintainDist);
        }
        else
        {
            m_combatState = CombatState.RadialRunToZone;
            ResetLastUsedAnimTrigger();
            StartRunAnim();
        }
    }

    private void RandomiseStrafeRange()
    {
        // Randomise the range for the AI to maintain based on attacker type
        if (m_currentAttackingType == AttackingType.Passive)
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist());
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

    public bool IsAttackCollidingWithPlayer()
    {
        bool isColliding = false;

        if (m_primaryWeaponCollider.bounds.Intersects(m_playerCollider.bounds) && m_primaryWeaponCollider.enabled ||
            m_secondaryWeaponCollider.bounds.Intersects(m_playerCollider.bounds) && m_secondaryWeaponCollider.enabled)
        {
            isColliding = true;
        }

        return isColliding;
    }

    private void TimedZoneCheck()
    {
        if (!m_zoneHandler.AreZonesAvailable())
        {
            return;
        }

        m_zoneTimer += Time.deltaTime;

        if (m_zoneTimer >= m_zoneCheckInterval)
        {
            m_zoneTimer = 0.0f;

            if (!m_zoneHandler.IsInMatchingZone())
            {
                // If in the active zone, but not an active attacker, back up
                if (m_currentAttackingType != AttackingType.Active)
                {
                    SetCombatState(CombatState.BackingUp);
                    return;
                }
                // If in a passive zone, but an active attacker, close the distance
                else if (m_currentAttackingType == AttackingType.Active)
                {
                    SetCombatState(CombatState.ClosingDist);
                    return;
                }
            }
            // If zone is not occupied
            if (m_zoneHandler.IsZoneAvailable())
            {
                m_zoneHandler.OccupyCurrentZone();
            }
            else if (m_zoneHandler.GetCurrentAttackZone().IsOccupied())
            {
                // Simple code for now to randomise whether an AI can force another AI out of zone
                // Todo: Refactor
                int takeoverChance = Random.Range(0, 2);
                if (takeoverChance > 0)
                {
                    m_zoneHandler.TakeOverOccupiedZone();
                }
                else
                {
                    SetCombatState(CombatState.StrafingToZone);
                }
            }
            else
            {
                SetCombatState(CombatState.StrafingToZone);
            }
        }
    }

    // Function to check if AI should start strafing to mimic more lifelike behaviour
    private void TimedBeginStrafeCheck()
    {
        if (m_zoneHandler.AreZonesAvailable())
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
        if (!m_zoneHandler.AreZonesAvailable())
        {
            return;
        }

        m_strafeTimer += Time.deltaTime;

        if (m_strafeTimer >= m_strafeCheckInterval)
        {
            m_strafeTimer = 0.0f;

            if (m_zoneHandler.IsZoneAvailable())               
            {
                m_zoneHandler.OccupyCurrentZone();
                SetCombatState(CombatState.MaintainDist);
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

        if( enemyToCheck != this )
        {
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
                        ResetLastUsedAnimTrigger();
                        StartWalkAnim();
                        m_strafeDist -= m_AIAvoidanceDist;
                        m_combatState = CombatState.ClosingDist;
                    }
                    else
                    {
                        ResetLastUsedAnimTrigger();
                        StartWalkBackAnim();
                        m_strafeDist += m_AIAvoidanceDist;
                        m_combatState = CombatState.BackingUp;
                    }
                }
            }
        }
    }

    // Checking if zone is available to occupy whilst radial running
    private void RadialZoneCheck()
    {
        if (m_zoneHandler.IsZoneAvailable())
        {
            m_combatState = CombatState.StrafingToZone;
            ResetLastUsedAnimTrigger();
            StartStrafeAnim(m_strafeDir);
        }
    }

    private EnemyAI FindClosestEnemy()
    {
        EnemyAI closestEnemy = this;

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

        return closestEnemy;
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
    private bool DistanceSqrCheck( GameObject targetToCheck, float distanceToCheck )
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

    private void StartWalkAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger(an_walk);
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = an_walk;
    }

    private void StartStrafeAnim( StrafeDir dirToStrafe )
    {
        int animTrigger = an_triggerNone;

        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.speed = m_strafeSpeed;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;

        switch (dirToStrafe)
        {
            case StrafeDir.Left:
            {
                animTrigger = an_strafeLeft;

                m_animController.SetTrigger(animTrigger);
                break;
            }
            case StrafeDir.Right:
            {
                animTrigger = an_strafeRight;

                m_animController.SetTrigger(animTrigger);
                break;
            }
        }

        m_lastUsedAnimTrigger = animTrigger;
    }

    private void StartWalkBackAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger(an_walkBack);
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = an_walkBack;
    }

    private void StartRunAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger(an_run);
        m_navMeshAgent.speed = m_runSpeed;
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = an_run;
    }

    private void StartIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_idle);
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = an_idle;
    }

    private void StartCombatIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_combatIdle);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = an_combatIdle;
    }

    private void StartAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_attack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = an_attack;
    }

    private void StartQuickAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_quickAttack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = an_quickAttack;
    }

    private void StartHeavyAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_heavyAttack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = an_heavyAttack;
    }

    private void StartDodgeAnim()
    {
        int animNum = Random.Range(0, m_dodgeAnimNum);
        int animTrigger = an_dodgeHashes[animNum];

        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(animTrigger);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_lastUsedAnimTrigger = animTrigger;
    }

    private void StartSleepToWakeAnim()
    {
        int animNum = Random.Range(0, m_sleepToWakeAnimNum);
        int animTrigger = an_sleepToWakeHashes[animNum];

        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(animTrigger);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = animTrigger;
    }

    private void SetToPlayDeadAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(an_sleep);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = an_sleep;
    }

    private void StartDeathAnim()
    {
        m_navMeshAgent.isStopped = true;
        //m_animController.SetTrigger(an_death);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
        m_lastUsedAnimTrigger = an_death;
    }

    /*
    public void StartDamageAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger(animTrigger);
        DisableCollision();
        m_lastUsedAnimTrigger = animTrigger;
    }
    */

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

    public void LockAttack()
    {
        m_navMeshAgent.isStopped = true;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
    }

    public void UnlockAttack()
    {
        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    public void PlayDamageSFX()
    {
        // Todo: Placeholder Function, need to flesh out the logic fully
        m_soundHandler.PlayDamageSFX();
    }

    public EnemyType GetEnemyType()
    {
        return m_enemyType;
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
            //Debug.Log("Error: Unassigned AI trying to find ZoneType");
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

    public void SetPatrolRoute(GameObject patrolRoute)
    {
        m_patrolRoute = patrolRoute;
        m_patrolRoutePoints.Clear();

        SetupPatrolRoutes();
    }

    public float GetViewRadius()
    {
        return m_viewRadius;
    }

    public float GetViewAngle()
    {
        return m_viewAngle;
    }

    public float GetEulerAngles()
    {
        return transform.eulerAngles.y;
    }

    /*
    public float GetHealth()
    {
        return m_health;
    }
    */
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

    public EnemySoundHandler GetSoundHandler()
    {
        return m_soundHandler;
    }

    public CharacterDamageManager GetHealthManager()
	{
        return m_healthManager;
	}

    public void SetLastUsedAnimTrigger( int trigger )
	{
        m_lastUsedAnimTrigger = trigger;
    }

    public void SetAIManagerRef( AIManager aiManagerRef )
    {
        m_aiManager = aiManagerRef;
    }

    public void SetStaggered(bool isStaggered)
    {
        m_isStaggered = isStaggered;
    }

    public bool IsStaggered()
    {
        return m_isStaggered;
    }

    public AttackingType GetAttackingType()
    {
        return m_currentAttackingType;
    }

    public AttackMode GetAttackMode()
    {
        return m_attackMode;
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

    public ZoneHandler GetZoneHandler()
    {
        return m_zoneHandler;
    }

    public void SetSpawnGroup(int groupToSet)
    {
        m_spawnGroup = groupToSet;
    }

    public int GetSpawnGroup()
    {
        return m_spawnGroup;
    }
}
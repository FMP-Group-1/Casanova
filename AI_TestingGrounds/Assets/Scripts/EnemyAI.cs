using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum AIState
{
    Idle,
    Sleeping,
    Pursuing,
    Patrolling,
    ReturningToPatrol,
    Attacking
}

enum PatrolState
{
    Patrol,
    ReversePatrol,
    Waiting
}

// Specific to Zombie Placeholder
// May need tweaking once actual enemies are implemented
enum CollisionSide
{
    Left,
    Right
}

// Enemy AI Script, will likely be reworked to use inheritance once base functionality is polished
public class EnemyAI : MonoBehaviour
{
    // For Testing nav movement, will need to switch to a form of player tracking when ready
    [SerializeField]
    private Transform m_movePosTarget;

    private NavMeshAgent m_navMeshAgent;
    private Animator m_animController;
    private AIState m_state = AIState.Idle;
    [SerializeField]
    private float m_walkSpeed = 2.0f;
    [SerializeField]
    private float m_runSpeed = 5.0f;

    // Patrol Relevant Variables
    [SerializeField]
    private GameObject m_patrolRoute;
    private PatrolState m_patrolState = PatrolState.Waiting;
    private PatrolState m_lastPatrolState;
    private List<Transform> m_patrolRoutePoints = new List<Transform>();
    private Transform m_nextPatrolPoint;
    private Vector3 m_lastPointOnPatrol;
    private float m_patrolTimer = 0.0f;
    private float m_patrolWaitTime = 2.5f;
    private int m_patrolDestinationIndex = 1;
    [SerializeField]
    private float m_patrolStoppingDistance = 0.5f;
    [SerializeField]
    private float m_playerStoppingDistance = 2.0f;

    // Player/Detection Relevant Variables
    private GameObject m_player;
    private CapsuleCollider m_playerCollider;

    // Vision Detection Relevant Variables
    [SerializeField]
    private float m_viewRadius = 15.0f;    
    [SerializeField]
    [Range(0.0f,360.0f)]
    private float m_viewAngle = 45.0f;

    [SerializeField]
    private LayerMask obstacleMask;

    private BoxCollider m_leftHandCollider;
    private BoxCollider m_rightHandCollider;

    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();

        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerCollider = m_player.GetComponent<CapsuleCollider>();
        m_leftHandCollider = GameObject.Find("Base HumanLArmPalm").GetComponent<BoxCollider>();
        m_rightHandCollider = GameObject.Find("Base HumanRArmPalm").GetComponent<BoxCollider>();

        DisableCollision();
    }
    private void Update()
    {
        m_player.GetComponent<Player>().SetHitVisual( IsAttackCollidingWithPlayer() );

        TestingInputs();

        switch ( m_state )
        {
            // Idle State
            case AIState.Idle:
            {
                if ( IsPlayerVisible() )
                {
                    //SetAIState(AIState.Pursuing);
                }
                break;
            }
            case AIState.Sleeping:
            {
                // Collision Detection/Hit Detection to go here
                // Left Click test just to make sure enemy can detect from this state
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Wake Up Enemy");
                    SetAIState(AIState.Idle);
                }
                break;
            }
            // Chase after target/player
            case AIState.Pursuing:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Very basic detection for reaching destination, will need to be expanded upon
                // i.e. in case of path being blocked
                // Logic from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
                if ( HasReachedDestination() )
                {
                    SetAIState(AIState.Attacking);
                    //Debug.Log("Destination Reached");
                }
                break;
            }
            // Patrol Logic
            case AIState.Patrolling:
            {
                if (IsPlayerVisible())
                {
                    //SetAIState(AIState.Pursuing);
                }
                AIPatrol();
                break;
            }
            case AIState.ReturningToPatrol:
            {
                if ( HasReachedDestination() )
                {
                    m_state = AIState.Patrolling;
                }
                break;
            }
            // Attack
            case AIState.Attacking:
            {
                break;
            }
        }
    }

    private void SetAIState( AIState stateToSet )
    {
        // If changing FROM patrol state, store the last position in the patrol route
        if ( m_state == AIState.Patrolling )
        {
            m_lastPointOnPatrol = gameObject.transform.position;
        }

        m_state = stateToSet;
        ResetAnimTriggers();

        switch ( stateToSet )
        {
            case AIState.Idle:
            {
                AIStandStill();
                break;
            }
            case AIState.Sleeping:
            {
                break;
            }
            case AIState.Pursuing:
            {
                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
                m_navMeshAgent.autoBraking = true;
                AIStartRun();
                break;
            }
            case AIState.Patrolling:
            {
                if ( m_patrolState == PatrolState.Patrol || m_patrolState == PatrolState.ReversePatrol )
                {
                    AIStartWalk();
                }
                else if ( m_patrolState == PatrolState.Waiting )
                {
                    AIStandStill();
                }
                break;
            }
            case AIState.ReturningToPatrol:
            {
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
                m_navMeshAgent.autoBraking = false;
                AIStartWalk();
                break;
            }
            case AIState.Attacking:
            {
                AIStartAttack();
                break;
            }
        }
    }

    private void AIPatrol()
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
                        m_patrolState = PatrolState.Waiting;
                        ResetAnimTriggers();
                        AIStandStill();
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
                        m_patrolState = PatrolState.Waiting;
                        ResetAnimTriggers();
                        AIStandStill();
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
                        m_patrolState = PatrolState.ReversePatrol;
                    }
                    else if (m_patrolDestinationIndex <= 0)
                    {
                        m_patrolState = PatrolState.Patrol;
                    }

                    m_patrolTimer = 0.0f;
                    ResetAnimTriggers();
                    AIStartWalk();
                }
                break;
            }
        }
    }

    private bool HasReachedDestination()
    {
        bool destinationReached = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
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

    private void DisableCollision()
    {
        m_leftHandCollider.enabled = false;
        m_rightHandCollider.enabled = false;
    }

    private void EnableCollision( CollisionSide sideToCollide )
    {
        if ( sideToCollide == CollisionSide.Left )
        {
            m_leftHandCollider.enabled = true;
        }
        else if ( sideToCollide == CollisionSide.Right )
        {
            m_rightHandCollider.enabled = true;
        }
    }

    public bool IsAttackCollidingWithPlayer()
    {
        bool isColliding = false;

        // If using this method for actual collision, needs a collider.enabled check
        // But for demonstrating the collision, this is not present currently

        if (m_leftHandCollider.bounds.Intersects(m_playerCollider.bounds))
        {
            isColliding = true;
        }
        if (m_rightHandCollider.bounds.Intersects(m_playerCollider.bounds))
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

        // Checking if player is in range
        if (Vector3.Distance(transform.position, m_player.transform.position) <= m_viewRadius)
        {
            // Once player is in range, getting the direction to the player and checking if it's within the AI's FOV
            Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < m_viewAngle * 0.5f )
            {
                // Once player is in range and in FOV, using Raycast to check if any obstacles are in the way
                float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);
                if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    playerIsVisible = true;
                }
            }
        }

        return playerIsVisible;
    }

    public AIState GetState()
    {
        return m_state;
    }

    public float GetViewRadius()
    {
        return m_viewRadius;
    }

    public float GetViewAngle()
    {
        return m_viewAngle;
    }

    private void AIStartWalk()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Walk");
        m_navMeshAgent.speed = m_walkSpeed;
    }

    private void AIStartRun()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Run");
        m_navMeshAgent.speed = m_runSpeed;
    }

    private void AIStandStill()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Idle");
    }

    private void AIStartAttack()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Attack");
    }

    private void EndAttack()
    {
        SetAIState(AIState.ReturningToPatrol);
        m_navMeshAgent.destination = m_lastPointOnPatrol;
    }

    private void ResetAnimTriggers()
    {
        m_animController.ResetTrigger("Walk");
        m_animController.ResetTrigger("Idle");
        m_animController.ResetTrigger("Attack");
        m_animController.ResetTrigger("Run");
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
            AIStartWalk();
            if (m_patrolRoute != null)
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
            }
            //Debug.Log("Going to Next Destination");
        }

        // Start Pursuing Test Input
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetAIState(AIState.Pursuing);
            ResetAnimTriggers();
            AIStartRun();
            m_navMeshAgent.destination = m_movePosTarget.position;
        }

        // Start Sleeping Test Input
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SetAIState(AIState.Sleeping);
        //}
    }
}
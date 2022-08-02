using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*******************************************
// Author: Dean Pearce
// Class: AIManager
// Description: Manages the EnemyAI objects in the game
//*******************************************

public class AIManager : MonoBehaviour
{
    private GameObject m_player;

    private List<EnemyAI> m_enemyList = new List<EnemyAI>();
    private List<EnemyAI> m_activeAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_passiveAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_unassignedAttackers = new List<EnemyAI>();
    private AttackZoneManager m_attackZoneManager;

    private bool m_canAttack = true;
    [SerializeField]
    private bool m_useSpawnManager = true;

    [SerializeField]
    [Range(0, 30)]
    private int m_attackZonesNum = 10;
    [SerializeField]
    private float m_activeAttackerMinDist = 3.0f;
    [SerializeField]
    private float m_activeAttackerMaxDist = 5.0f;
    [SerializeField]
    private float m_passiveAttackerMaxDist = 10.0f;

    [SerializeField]
    private float m_zoneDistanceBuffer = 2.0f;

    [SerializeField]
    private int m_maxActiveAttackers = 3;
    [SerializeField]
    private int m_maxSimultaneousAttacks = 2;

    [SerializeField]
    private GameObject m_obsCheckDebug;

    //Input System
    private DeanControls m_inputs;
    [SerializeField]
    private bool m_debugInputsActive = false;

    private void Awake()
    {
        // Create the control input
        m_inputs = new DeanControls();
        m_attackZoneManager = new AttackZoneManager(this);
    }

    private void OnEnable()
    {
        m_inputs.Enable();
    }

    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");

        EventManager.WakeEnemiesEvent += WakeGroup;
        EventManager.AlertEnemiesEvent += AlertGroup;

        if (!m_useSpawnManager)
        {
            RegisterEnemies();
        }
    }

    void Update()
    {
        // Function for reading the test inputs
        TestingInputs();

        m_attackZoneManager.Update();
        ActiveAttackerCount();

        // Check to make sure too many enemies can't attack at once
        m_canAttack = TotalEnemiesAttacking() < m_maxSimultaneousAttacks;
    }

    public void RegisterEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();

            if (enemyScript != null)
            {
                // Adding the enemy into the list
                m_enemyList.Add(enemyScript);

                // Giving the enemy a reference to the managers
                enemyScript.SetAIManagerRef(this);
                enemyScript.SetupZoneHandler(ref m_attackZoneManager);
            }
            else
            {
                // Notifying user that an enemy has failed to register with the manager
                Debug.Log("AIManager: Failed to add EnemyAI script of Enemy: " + enemy.name);
            }
        }

        //Debug.Log("AIManager: Enemies in list: " + m_enemyList.Count);
    }

    // Register an enemy as an attacker
    public void RegisterAttacker(EnemyAI enemyToRegister)
    {
        if (!m_unassignedAttackers.Contains(enemyToRegister))
        {
            m_unassignedAttackers.Add(enemyToRegister);
            enemyToRegister.SetAttackingType(AttackingType.Unassigned);
        }

        // Check to make sure enemy isn't already in list
        //if (!m_activeAttackers.Contains(enemyToRegister) && !m_passiveAttackers.Contains(enemyToRegister))
        //{
        //    // If active attackers isn't at max, add to active attackers, otherwise add to the passive attackers
        //    if (m_activeAttackers.Count < m_maxActiveAttackers)
        //    {
        //        m_activeAttackers.Add(enemyToRegister);
        //        enemyToRegister.SetAttackingType(AttackingType.Active);
        //    }
        //    else
        //    {
        //        m_passiveAttackers.Add(enemyToRegister);
        //        enemyToRegister.SetAttackingType(AttackingType.Passive);
        //    }
        //}
    }

        // Unregister enemy from attacker lists
    public void UnregisterAttacker(EnemyAI enemyToUnregister)
    {
        if (m_activeAttackers.Contains(enemyToUnregister))
        {
            m_activeAttackers.Remove(enemyToUnregister);
        }
        if (m_passiveAttackers.Contains(enemyToUnregister))
        {
            m_passiveAttackers.Remove(enemyToUnregister);
        }
        if (m_unassignedAttackers.Contains(enemyToUnregister))
        {
            m_unassignedAttackers.Remove(enemyToUnregister);
        }
    }

    // Adds specified enemy to active attacker list, and makes sure they're removed from passive list
    public void MakeActiveAttacker(EnemyAI enemy)
    {
        if (m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Remove(enemy);
        }
        if (m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Remove(enemy);
        }
        if (!m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Add(enemy);
            enemy.SetAttackingType(AttackingType.Active);
        }
    }

    // Adds specified enemy to passive attacker list, and makes sure they're removed from active list
    public void MakePassiveAttacker(EnemyAI enemy)
    {
        if (m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Remove(enemy);
        }
        if (m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Remove(enemy);
        }
        if (!m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Add(enemy);
            enemy.SetAttackingType(AttackingType.Passive);
        }
    }

    public void MakeUnasssignedAttacker( EnemyAI enemy )
    {
        if (m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Remove(enemy);
        }
        if (m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Remove(enemy);
        }
        if (!m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Add(enemy);
            enemy.SetAttackingType(AttackingType.Unassigned);
        }
    }

    // Function for ensuring the active attacker count is always correct
    private void ActiveAttackerCount()
    {
        if (m_activeAttackers.Count > m_maxActiveAttackers)
        {
            MakePassiveAttacker(FindFurthestActiveAttacker());
        }
        else if (m_activeAttackers.Count < m_maxActiveAttackers && m_passiveAttackers.Count > 0)
        {
            MakeActiveAttacker(FindClosestPassiveAttacker());
        }
    }

    private void WakeGroup(int groupToWake)
    {
        foreach(EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf && enemy.GetSpawnGroup() == groupToWake)
            {
                enemy.WakeUpAI();
            }
        }
    }

    private void AlertGroup(int groupToAlert)
    {
        foreach (EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf && enemy.GetSpawnGroup() == groupToAlert)
            {
                enemy.SetAIState(AIState.InCombat);
            }
        }
    }

    // Function for finding the furthest active attacker from the player
    private EnemyAI FindFurthestActiveAttacker()
    {
        // Setting the first index as the default
        EnemyAI furthestEnemy = m_activeAttackers[0];

        for (int i = 0; i < m_activeAttackers.Count; i++)
        {
            // Looping through the list to compare distances
            if (GetSqrDistance(m_activeAttackers[i].gameObject, m_player) > GetSqrDistance(furthestEnemy.gameObject, m_player))
            {
                furthestEnemy = m_activeAttackers[i];
            }
        }

        return furthestEnemy;
    }

    // Function for finding the closest passive attacker to the player
    private EnemyAI FindClosestPassiveAttacker()
    {
        // Setting the first index as the default
        EnemyAI closestEnemy = m_passiveAttackers[0];

        for (int i = 0; i < m_passiveAttackers.Count; i++)
        {
            // Looping through the list to compare distances
            if (GetSqrDistance(m_passiveAttackers[i].gameObject, m_player) < GetSqrDistance(closestEnemy.gameObject, m_player))
            {
                closestEnemy = m_passiveAttackers[i];
            }
        }

        return closestEnemy;
    }

    public bool ActiveSlotsOpen()
    {
        return m_activeAttackers.Count < m_maxActiveAttackers;
    }

    // Function for getting the square distance for more optimal comparison checks
    private float GetSqrDistance( GameObject firstTarget, GameObject secondTarget )
    {
        return (firstTarget.transform.position - secondTarget.transform.position).sqrMagnitude;
    }

    // Function for passive attacker to call when they've gotten too close to the player
    // Makes the passive attacker an active attacker and vice versa
    public void SwapPassiveWithActive( EnemyAI enemyToSwap )
    {
        EnemyAI furthestActive = FindFurthestActiveAttacker();
        MakeActiveAttacker(enemyToSwap);
        MakePassiveAttacker(furthestActive);

        if (furthestActive.GetCombatState() != CombatState.Attacking)
        {
            furthestActive.SetCombatState(CombatState.BackingUp);
        }
    }

    public int TotalEnemiesAttacking()
    {
        int total = 0;

        foreach (EnemyAI enemy in m_activeAttackers)
        {
            if (enemy.GetCombatState() == CombatState.Attacking || enemy.GetCombatState() == CombatState.MovingToAttack)
            {
                total++;
            }
        }

        return total;
    }

    public int RemainingEnemiesInGroup(int groupNum)
    {
        int enemiesRemaining = 0;

        for (int i = 0; i < m_enemyList.Count; i++)
        {
            if (m_enemyList[i].GetSpawnGroup() == groupNum && m_enemyList[i].gameObject.activeSelf)
            {
                enemiesRemaining++;
            }
        }

        return enemiesRemaining;
    }

    public AttackZoneManager GetAttackZoneManager()
    {
        return m_attackZoneManager;
    }

    public int GetAttackZonesNum()
    {
        return m_attackZonesNum;
    }

    public float GetActiveAttackerMinDist()
    {
        return m_activeAttackerMinDist;
    }

    public float GetActiveAttackerMaxDist()
    {
        return m_activeAttackerMaxDist;
    }

    public float GetPassiveAttackerMaxDist()
    {
        return m_passiveAttackerMaxDist;
    }

    public float GetZoneDistanceBuffer()
    {
        return m_zoneDistanceBuffer;
    }

    public bool CanAttack()
    {
        return m_canAttack;
    }

    public void SetCanAttack(bool canAttack)
    {
        m_canAttack = canAttack;
    }

    public GameObject GetObsCheckDebug()
    {
        return m_obsCheckDebug;
    }

    public List<EnemyAI> GetEnemyList()
    {
        return m_enemyList;
    }

    // Function for reading inputs for purposes of debugging
    private void TestingInputs()
    {
        if (m_debugInputsActive)
        {
            // Start Patrolling Test Input
            if (m_inputs.Debug.AI_Move.triggered)
            {
                foreach (EnemyAI enemy in m_enemyList)
                {
                    enemy.SetAIState(AIState.Patrolling);
                }
            }

            // Start Pursuing Test Input
            if (m_inputs.Debug.AI_Combat.triggered)
            {
                foreach (EnemyAI enemy in m_enemyList)
                {
                    if (enemy.GetState() != AIState.Dead)
                    {
                        enemy.SetAIState(AIState.InCombat);
                        m_canAttack = true;
                    }
                }
            }
        }
    }
}

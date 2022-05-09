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
    private AttackZoneManager m_attackZoneManager;

    private bool m_canAttack = true;

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
    private GameObject m_obsCheckDebug;
    
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_attackZoneManager = new AttackZoneManager(this);

        RegisterEnemies();
    }

    void Update()
    {
        m_attackZoneManager.Update();
        ActiveAttackerCount();
    }

    private void RegisterEnemies()
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
                enemyScript.SetAttackZoneManagerRef(m_attackZoneManager);
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
        // Check to make sure enemy isn't already in list
        if (!m_activeAttackers.Contains(enemyToRegister) && !m_passiveAttackers.Contains(enemyToRegister))
        {
            // If active attackers isn't at max, add to active attackers, otherwise add to the passive attackers
            if (m_activeAttackers.Count < m_maxActiveAttackers)
            {
                m_activeAttackers.Add(enemyToRegister);
                enemyToRegister.SetAttackingType(AttackingType.Active);
            }
            else
            {
                m_passiveAttackers.Add(enemyToRegister);
                enemyToRegister.SetAttackingType(AttackingType.Passive);
            }
        }
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
    }

    // Adds specified enemy to active attacker list, and makes sure they're removed from passive list
    public void MakeActiveAttacker(EnemyAI enemy)
    {
        if (m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Remove(enemy);
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
        if (!m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Add(enemy);
            enemy.SetAttackingType(AttackingType.Passive);
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

    // Function for finding the furthest active attacker from the player
    private EnemyAI FindFurthestActiveAttacker()
    {
        // Setting the first index as the default
        EnemyAI furthestEnemy = m_activeAttackers[0];

        for (int i = 0; i < m_activeAttackers.Count; i++)
        {
            // Looping through the list to compare distances
            if (Vector3.Distance(m_activeAttackers[i].gameObject.transform.position, m_player.transform.position) > Vector3.Distance(furthestEnemy.transform.position, m_player.transform.position))
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
            if (Vector3.Distance(m_passiveAttackers[i].gameObject.transform.position, m_player.transform.position) < Vector3.Distance(closestEnemy.transform.position, m_player.transform.position))
            {
                closestEnemy = m_passiveAttackers[i];
            }
        }

        return closestEnemy;
    }

    // Function for passive attacker to call when they've gotten to close to the player
    public void SwapPassiveWithActive( EnemyAI enemyToSwap )
    {
        // Todo: Add logic to interrupt MovingToAttack state
        MakeActiveAttacker(enemyToSwap);
        MakePassiveAttacker(FindFurthestActiveAttacker());
    }

    // Check if any of the active attackers are currently attacking
    public bool AreEnemiesAttacking()
    {
        foreach (EnemyAI enemy in m_activeAttackers)
        {
            if (enemy.GetCombatState() == CombatState.Attacking)
            {
                return true;
            }
        }

        return false;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private List<EnemyAI> m_enemyList = new List<EnemyAI>();
    private List<EnemyAI> m_activeAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_passiveAttackers = new List<EnemyAI>();

    [SerializeField]
    private int m_maxActiveAttackers = 3;
    
    void Start()
    {
        RegisterEnemies();
    }

    void Update()
    {

    }

    private void RegisterEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();

            if (enemyScript != null)
            {
                m_enemyList.Add(enemyScript);
                enemyScript.SetAIManagerRef(this);
            }
            else
            {
                Debug.Log("AIManager: Failed to add EnemyAI script of Enemy: " + enemy.name);
            }
        }

        Debug.Log("AIManager: Enemies in list: " + m_enemyList.Count);
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
            }
            else
            {
                m_passiveAttackers.Add(enemyToRegister);
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
        }
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
}

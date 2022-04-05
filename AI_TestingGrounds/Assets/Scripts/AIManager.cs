using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private GameObject m_player;

    private List<EnemyAI> m_enemyList = new List<EnemyAI>();
    private List<EnemyAI> m_activeAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_passiveAttackers = new List<EnemyAI>();
    private List<AttackZone> m_activeAttackZones = new List<AttackZone>();
    private List<AttackZone> m_passiveAttackZones = new List<AttackZone>();

    [SerializeField]
    [Min(0)]
    private int m_attackZonesNum = 10;
    [SerializeField]
    private float m_activeAttackerMinDist = 3.0f;
    [SerializeField]
    private float m_activeAttackerMaxDist = 5.0f;
    [SerializeField]
    private float m_passiveAttackerMaxDist = 10.0f;

    [SerializeField]
    private int m_maxActiveAttackers = 3;
    
    void Start()
    {
        RegisterEnemies();
        SetupAttackZones();

        m_player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        Debug.Log("AIManager: Current Zone: " + FindAttackZone(m_enemyList[0]).GetZoneType());
    }

    private void SetupAttackZones()
    {
        for ( int i = 0; i < m_attackZonesNum; i++)
        {
            m_activeAttackZones.Add(new AttackZone(false, ZoneType.Active));
            m_passiveAttackZones.Add(new AttackZone(false, ZoneType.Passive));
        }
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
                enemyToRegister.SetAttackingType(AttackingType.Passive);
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

    //public float FindAttackSection(EnemyAI enemyToCheck)
    //{
    //    // Messy equation, needs refactoring, but basic logic works
    //    Vector3 enemyPos = enemyToCheck.gameObject.transform.position;

    //    Vector3 dirFromPlayer = (enemyPos - m_player.transform.position).normalized;
    //    float angle = Vector3.SignedAngle(enemyPos, dirFromPlayer, Vector3.up);

    //    if (angle < 0.0f)
    //    {
    //        angle = 360.0f - angle * -1.0f;
    //    }

    //    float sectionAngle = 360.0f / m_attackZonesNum;

    //    return (int)(angle / sectionAngle);
    //}

    public AttackZone FindAttackZone( EnemyAI enemyToCheck )
    {
        // Messy equation, needs refactoring, but basic logic works
        Vector3 enemyPos = enemyToCheck.gameObject.transform.position;

        Vector3 dirFromPlayer = (enemyPos - m_player.transform.position).normalized;
        float angle = Vector3.SignedAngle(enemyPos, dirFromPlayer, Vector3.up);

        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        float sectionAngle = 360.0f / m_attackZonesNum;

        float dist = Vector3.Distance(enemyToCheck.transform.position, m_player.transform.position);

        if (dist > m_activeAttackerMinDist && dist < m_passiveAttackerMaxDist )
        {
            if (dist < m_activeAttackerMaxDist)
            {
                return m_activeAttackZones[(int)(angle / sectionAngle)];
            }
            else
            {
                return m_passiveAttackZones[(int)(angle / sectionAngle)];
            }
        }

        // Todo: Refactor this function to not use null returns
        return null;
    }

    public Vector3 RandomiseAttackPosForEnemy(EnemyAI enemy)
    {
        float anglePerZone = 360.0f / m_attackZonesNum;
        float dist = m_activeAttackerMaxDist;

        int attackZone = Random.Range(0, m_attackZonesNum);
        float randomAngle = Random.Range(anglePerZone * attackZone, anglePerZone * (attackZone + 1));

        Vector3 dirToAttackZone = DirFromAngle(randomAngle, true, m_player);

        if (enemy.GetAttackingType() == AttackingType.Active)
        {
            dist = Random.Range(m_activeAttackerMinDist, m_activeAttackerMaxDist);
        }
        else if (enemy.GetAttackingType() == AttackingType.Passive)
        {
            dist = Random.Range(m_activeAttackerMaxDist, m_passiveAttackerMaxDist);
        }

        //enemy.SetStrafeDist(dist);

        return m_player.transform.position + (dirToAttackZone * dist);
    }

    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject gameObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += gameObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

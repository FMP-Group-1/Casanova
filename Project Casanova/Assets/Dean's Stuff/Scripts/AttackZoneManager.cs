using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackZoneManager
{
    private AIManager m_aiManager;
    private GameObject m_player;
    private List<AttackZone> m_activeAttackZones = new List<AttackZone>();
    private List<AttackZone> m_passiveAttackZones = new List<AttackZone>();
    private int m_attackZonesNum;

    private float m_sectionHalfAngle;
    private float m_anglePerSection;

    private int m_currentZoneNumToCheck = 0;

    private GameObject m_obsCheckDebug;
    private List<GameObject> m_obsCheckChildArray = new List<GameObject>();

    public AttackZoneManager(AIManager aiManager)
    {
        m_aiManager = aiManager;
        m_player = GameObject.FindGameObjectWithTag("Player");

        // Could possibly refactor this to use it's own variable, but for now it's set by the AIManager because it can be set in inspector
        m_attackZonesNum = m_aiManager.GetAttackZonesNum();

        SetupAttackZones();

        // Setting the debug object from the AI as it's set in inspector
        // Only using temporarily to get base logic working
        m_obsCheckDebug = m_aiManager.GetObsCheckDebug();

        // Adding the debug objects to the list
        for (int i = 0; i < m_obsCheckDebug.transform.childCount; i++)
        {
            m_obsCheckChildArray.Add(m_obsCheckDebug.transform.GetChild(i).gameObject);
        }
    }

    public void Update()
    {
        // Using this update function to sequentially check for obstruction in zones

        m_activeAttackZones[m_currentZoneNumToCheck].CheckForObstruction();
        m_passiveAttackZones[m_currentZoneNumToCheck].CheckForObstruction();

        m_currentZoneNumToCheck++;
        if (m_currentZoneNumToCheck >= m_aiManager.GetAttackZonesNum())
        {
            m_currentZoneNumToCheck = 0;
        }
    }
    private void SetupAttackZones()
    {
        // Setting up attack zone objects, and giving them their initial data
        m_anglePerSection = 360.0f / m_attackZonesNum;
        m_sectionHalfAngle = m_anglePerSection * 0.5f;

        for (int i = 0; i < m_attackZonesNum; i++)
        {
            m_activeAttackZones.Add(new AttackZone(false, ZoneType.Active, i));
            m_activeAttackZones[i].SetBounds(m_anglePerSection * i - m_sectionHalfAngle, m_anglePerSection * (i + 1) - m_sectionHalfAngle, m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist(), m_anglePerSection);
            m_passiveAttackZones.Add(new AttackZone(false, ZoneType.Passive, i));
            m_passiveAttackZones[i].SetBounds(m_anglePerSection * i - m_sectionHalfAngle, m_anglePerSection * (i + 1) - m_sectionHalfAngle, m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist(), m_anglePerSection);
        }
    }

    // Function for finding the attack zone that the given enemy is in
    public AttackZone FindAttackZone( EnemyAI enemyToCheck )
    {
        // Messy equation, needs refactoring, but basic logic works
        Vector3 enemyPos = enemyToCheck.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Todo: In BIG need of refactor

        //Debug.Log("Zone Angle: " + angle);

        angle += m_sectionHalfAngle;

        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        float sectionAngle = 360.0f / m_attackZonesNum;

        float dist = Vector3.Distance(enemyToCheck.transform.position, m_player.transform.position);

        if (dist > m_aiManager.GetActiveAttackerMinDist() && dist < m_aiManager.GetPassiveAttackerMaxDist())
        {
            if (dist < m_aiManager.GetActiveAttackerMaxDist())
            {
                return m_activeAttackZones[(int)(angle / sectionAngle)];
            }
            else
            {
                return m_passiveAttackZones[(int)(angle / sectionAngle)];
            }
        }

        // Todo: Refactor this function to not use null returns (maybe, kind of useful atm)
        return null;
    }

    // Function for randomising a position for a given enemy to travel to, not currently being made use of, as enemies have a different logic setup right now
    public Vector3 RandomiseAttackPosForEnemy( EnemyAI enemy )
    {
        float anglePerZone = 360.0f / m_attackZonesNum;
        float dist = m_aiManager.GetActiveAttackerMaxDist();

        int attackZone = Random.Range(0, m_attackZonesNum);
        float randomAngle = Random.Range((anglePerZone * attackZone) + m_aiManager.GetZoneDistanceBuffer(), (anglePerZone * (attackZone + 1)) - m_aiManager.GetZoneDistanceBuffer());

        Vector3 dirToAttackZone = DirFromAngle(randomAngle - m_sectionHalfAngle, true, m_player);

        if (enemy.GetAttackingType() == AttackingType.Active)
        {
            dist = Random.Range(m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist());
        }
        else if (enemy.GetAttackingType() == AttackingType.Passive)
        {
            dist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist());
        }

        return m_player.transform.position + (dirToAttackZone * dist);
    }

    // Overload of above function to get pos for a specific zone
    public Vector3 RandomiseAttackPosForEnemy( EnemyAI enemy, int zoneToUse )
    {
        float anglePerZone = 360.0f / m_attackZonesNum;
        float dist = m_aiManager.GetActiveAttackerMaxDist();

        float randomAngle = Random.Range((anglePerZone * zoneToUse) + m_aiManager.GetZoneDistanceBuffer(), (anglePerZone * (zoneToUse + 1)) - m_aiManager.GetZoneDistanceBuffer());

        Vector3 dirToAttackZone = DirFromAngle(randomAngle - m_sectionHalfAngle, true, m_player);

        if (enemy.GetAttackingType() == AttackingType.Active)
        {
            dist = Random.Range(m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist());
        }
        else if (enemy.GetAttackingType() == AttackingType.Passive)
        {
            dist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist());
        }

        return m_player.transform.position + (dirToAttackZone * dist);
    }

    // Getting position for an enemy by a specified zone and distance
    public Vector3 GetAttackPosByZoneAndDist( EnemyAI enemy, int zoneToUse, float dist )
    {
        float anglePerZone = 360.0f / m_attackZonesNum;

        float randomAngle = Random.Range((anglePerZone * zoneToUse) + m_aiManager.GetZoneDistanceBuffer(), (anglePerZone * (zoneToUse + 1)) - m_aiManager.GetZoneDistanceBuffer());

        Vector3 dirToAttackZone = DirFromAngle(randomAngle - m_sectionHalfAngle, true, m_player);

        return m_player.transform.position + (dirToAttackZone * dist);
    }

    // Return the number of the zone the specified enemy is in
    public int GetZoneNumByAngle( EnemyAI enemy )
    {
        Vector3 enemyPos = enemy.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Todo: In BIG need of refactor

        //Debug.Log("Zone Angle: " + angle);

        angle += m_sectionHalfAngle;

        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        float sectionAngle = 360.0f / m_attackZonesNum;

        return (int)(angle / sectionAngle);
    }

    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject gameObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += gameObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public AttackZone GetAttackZoneByNum( int num, ZoneType zoneType )
    {
        AttackZone zoneToReturn = null;

        switch (zoneType)
        {
            case ZoneType.Passive:
            {
                zoneToReturn = m_passiveAttackZones[num];
                break;
            }
            case ZoneType.Active:
            {
                zoneToReturn = m_activeAttackZones[num];
                break;
            }
        }

        return zoneToReturn;
    }

    public List<AttackZone> GetPassiveAttackZones()
    {
        return m_passiveAttackZones;
    }

    public List<AttackZone> GetActiveAttackZones()
    {
        return m_activeAttackZones;
    }

    public float GetAnglePerSection()
    {
        return m_anglePerSection;
    }
}

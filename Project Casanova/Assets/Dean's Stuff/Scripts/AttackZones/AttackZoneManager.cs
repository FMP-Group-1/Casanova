using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*******************************************
// Author: Dean Pearce
// Class: AttackZoneManager
// Description: Class for managing the attack zone objects which hold information about the attack zones
// around the player
//*******************************************
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
        if ( m_aiManager.GetObsCheckDebug() != null)
        {
            m_obsCheckDebug = m_aiManager.GetObsCheckDebug();

            // Adding the debug objects to the list
            for (int i = 0; i < m_obsCheckDebug.transform.childCount; i++)
            {
                m_obsCheckChildArray.Add(m_obsCheckDebug.transform.GetChild(i).gameObject);
            }
        }
    }

    public void Update()
    {
        ObsCheckUpdate();
    }

    private void ObsCheckUpdate()
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
        AttackZone returnZone = null;

        // Getting relevant positions
        Vector3 enemyPos = enemyToCheck.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        // Zeroing y co-ords to prevent affecting angle output
        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        // Getting dir between enemy and player
        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Adding half angle to account for the offset
        angle += m_sectionHalfAngle;

        // Wrapping angle back to 360
        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        // If within the zone bounds
        if (DistanceSqrCheck(enemyToCheck.gameObject, m_player, m_aiManager.GetPassiveAttackerMaxDist()))
        {
            // Within active zone bounds
            if (DistanceSqrCheck(enemyToCheck.gameObject, m_player, m_aiManager.GetActiveAttackerMaxDist()))
            {
                returnZone = m_activeAttackZones[(int)(angle / m_anglePerSection)];
            }
            // Within passive zone bounds
            else
            {
                returnZone = m_passiveAttackZones[(int)(angle / m_anglePerSection)];
            }
        }

        return returnZone;
    }

    // Getting position for an enemy by a specified zone and distance
    public Vector3 GetSpecifiedPos( float angle, float dist )
    {
        Vector3 dirToPos = DirFromAngle(angle - m_sectionHalfAngle, true, m_player);

        return m_player.transform.position + (dirToPos * dist);
    }

    // Return the number of the zone the specified enemy is in
    public int GetZoneNumByAngle( EnemyAI enemy )
    {
        // Getting positions
        Vector3 enemyPos = enemy.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        // Zero y to prevent affecting angles
        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        // Getting dir from player to enemy
        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Add half angle to account for offset
        angle += m_sectionHalfAngle;

        // If angle less than 0, wrap back around
        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        return (int)(angle / m_anglePerSection);
    }

    // Function for optimally checking two targets are within a given distance of each other
    private bool DistanceSqrCheck( GameObject firstTarget, GameObject secondTarget, float distanceToCheck )
    {
        bool isInRange = false;

        // Getting the distance between this and the target
        Vector3 distance = firstTarget.transform.position - secondTarget.transform.position;

        // Checking if sqrMagnitude is less than the distance squared
        if (distance.sqrMagnitude <= distanceToCheck * distanceToCheck)
        {
            isInRange = true;
        }

        return isInRange;
    }

    public bool AreZonesAvailable(ZoneType typeToCheck)
    {
        if( typeToCheck == ZoneType.Active)
        {
            foreach (AttackZone zone in m_activeAttackZones)
            {
                if (zone.IsAvailable())
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (AttackZone zone in m_passiveAttackZones)
            {
                if (zone.IsAvailable())
                {
                    return true;
                }
            }
        }

        return false;
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
        if (zoneType == ZoneType.Passive)
        {
            return m_passiveAttackZones[num];
        }
        else
        {
            return m_activeAttackZones[num];
        }
    }

    public List<AttackZone> GetPassiveAttackZones()
    {
        return m_passiveAttackZones;
    }

    public List<AttackZone> GetActiveAttackZones()
    {
        return m_activeAttackZones;
    }

    public int GetTotalZonesNum()
    {
        return m_attackZonesNum;
    }

    public float GetAnglePerSection()
    {
        return m_anglePerSection;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZoneType
{
    Passive,
    Active
}

public class AttackZone
{
    private bool m_isOccupied = false;
    private bool m_isObstructed = false;
    private ZoneType m_zoneType = ZoneType.Active;
    private EnemyAI m_occupant;
    private GameObject m_player;
    private int m_zoneNum;
    private float m_navMeshCheckDist = 1.0f;
    private float m_zoneAngleSize;
    private float m_zoneAngleStart;
    private float m_zoneAngleEnd;
    private float m_zoneDistStart;
    private float m_zoneDistEnd;

    public AttackZone(bool isOccupied, ZoneType zoneType, int zoneNum)
    {
        m_isOccupied = isOccupied;
        m_zoneType = zoneType;
        m_zoneNum = zoneNum;
        m_player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SetBounds( float angleStart, float angleEnd, float startDist, float endDist, float angleSize)
    {
        m_zoneAngleStart = angleStart;
        m_zoneAngleEnd = angleEnd;
        m_zoneDistStart = startDist;
        m_zoneDistEnd = endDist;
        m_zoneAngleSize = angleSize;
    }

    public bool IsOccupied()
    {
        return m_isOccupied;
    }

    public bool IsObstructed()
    {
        return m_isObstructed;
    }

    public bool IsAvailable()
    {
        return !m_isOccupied && !m_isObstructed;
    }

    public void SetOccupied(bool isOccupied)
    {
        m_isOccupied = isOccupied;
    }

    public int GetZoneNum()
    {
        return m_zoneNum;
    }

    public void SetZoneNum(int zoneNum)
    {
        m_zoneNum = zoneNum;
    }

    public ZoneType GetZoneType()
    {
        return m_zoneType;
    }

    public void SetZoneType(ZoneType typeToSet)
    {
        m_zoneType = typeToSet;
    }

    public EnemyAI GetOccupant()
    {
        return m_occupant;
    }

    public void SetOccupant( EnemyAI occupantToSet )
    {
        m_occupant = occupantToSet;
        m_isOccupied = true;
    }

    public void EmptyZone()
    {
        m_occupant = null;
        m_isOccupied = false;
    }

    public float GetAngleStart()
    {
        return m_zoneAngleStart;
    }

    public float GetAngleEnd()
    {
        return m_zoneAngleEnd;
    }

    public float GetStartDist()
    {
        return m_zoneDistStart;
    }

    public float GetEndDist()
    {
        return m_zoneDistEnd;
    }

    public void CheckForObstruction()
    {
        // Todo: Awful function, needs rework, just putting in as basic logic
        Vector3[] pointArray = new Vector3[5];

        pointArray[0] = DirFromAngle(m_zoneAngleStart, true, m_player) * m_zoneDistStart;
        pointArray[1] = DirFromAngle(m_zoneAngleStart, true, m_player) * m_zoneDistEnd;
        pointArray[2] = DirFromAngle(m_zoneAngleEnd, true, m_player) * m_zoneDistStart;
        pointArray[3] = DirFromAngle(m_zoneAngleEnd, true, m_player) * m_zoneDistEnd;
        pointArray[4] = DirFromAngle(m_zoneDistEnd - (m_zoneAngleSize * 0.5f), true, m_player) * (m_zoneDistStart + ((m_zoneDistEnd - m_zoneDistStart) * 0.5f));

        for (int i = 0; i < pointArray.Length; i++)
        {
            if (!NavMesh.SamplePosition(pointArray[i], out NavMeshHit hit, m_navMeshCheckDist, NavMesh.AllAreas))
            {
                // If point is not valid on navmesh          
                Debug.Log("Zone: " + m_zoneType + " " + m_zoneNum + " NavMesh Invalid");
            }
        }
    }

    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject dirFromObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += dirFromObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

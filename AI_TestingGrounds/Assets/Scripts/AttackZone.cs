using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private int m_zoneNum;

    public AttackZone(bool isOccupied, ZoneType zoneType, int zoneNum)
    {
        m_isOccupied = isOccupied;
        m_zoneType = zoneType;
        m_zoneNum = zoneNum;
    }

    public void ObstructionCheck()
    {

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
}

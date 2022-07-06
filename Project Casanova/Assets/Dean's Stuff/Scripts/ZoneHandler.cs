using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneHandler
{
    private EnemyAI m_parentAI;
    private AttackZoneManager m_attackZoneManager;
    private AttackZone m_currentAttackZone;
    private AttackZone m_occupiedAttackZone;

    public void Init(ref EnemyAI enemyAI, ref AttackZoneManager attackZoneManager)
    {
        m_parentAI = enemyAI;
        m_attackZoneManager = attackZoneManager;
    }

    public void Update()
    {
        m_currentAttackZone = m_attackZoneManager.FindAttackZone(m_parentAI);

        OccupiedZoneCheck();
    }

    private void OccupiedZoneCheck()
    {
        if (m_occupiedAttackZone != null && !IsInOccupiedZone())
        {
            ClearOccupiedZone();
        }
    }
    public void ClearOccupiedZone()
    {
        m_occupiedAttackZone.EmptyZone();
        m_occupiedAttackZone = null;
    }

    public void OccupyCurrentZone()
    {
        m_occupiedAttackZone = m_currentAttackZone;
        m_occupiedAttackZone.SetOccupant(m_parentAI);
    }

    public void TakeOverOccupiedZone()
    {
        EnemyAI currentOccupant = m_currentAttackZone.GetOccupant();

        currentOccupant.GetZoneHandler().ClearOccupiedZone();
        currentOccupant.SetCombatState(CombatState.StrafingToZone);
        OccupyCurrentZone();
    }

    private bool AdjacentZoneIsAvailable()
    {
        bool zoneAvailable = false;
        int nextZoneNum;

        // Getting num of the next zone based on strafe dir
        if (m_parentAI.GetStrafeDir() == StrafeDir.Left)
        {
            nextZoneNum = m_currentAttackZone.GetZoneNum() + 1 % m_attackZoneManager.GetTotalZonesNum();
        }
        else
        {
            nextZoneNum = m_currentAttackZone.GetZoneNum() - 1;

            if (nextZoneNum < 0)
            {
                nextZoneNum = m_attackZoneManager.GetTotalZonesNum() - 1;
            }
        }

        // Checking if target zone is available
        if (m_parentAI.GetAttackingType() == AttackingType.Passive)
        {
            if (!m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Passive).IsObstructed() &&
                !m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Passive).IsOccupied())
            {
                zoneAvailable = true;
            }

        }
        else
        {
            if (!m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Active).IsObstructed() &&
                !m_attackZoneManager.GetAttackZoneByNum(nextZoneNum, ZoneType.Active).IsOccupied())
            {
                zoneAvailable = true;
            }

        }

        return zoneAvailable;
    }

    public bool IsInOccupiedZone()
    {
        if (IsInValidZone())
        {
            return m_occupiedAttackZone == m_currentAttackZone;
        }
        else
        {
            return false;
        }
    }

    public bool IsZoneAvailable()
    {
        if (IsInValidZone())
        {
            return m_currentAttackZone.IsAvailable();
        }
        else
        {
            return false;
        }
    }

    public bool IsInValidZone()
    {
        return m_currentAttackZone != null;
    }

    public bool IsInMatchingZone()
    {
        if (IsInValidZone())
        {
            return m_currentAttackZone.GetZoneType() == m_parentAI.GetZoneTypeFromAttackType();
        }
        else
        {
            return false;
        }
    }

    public bool AreZonesAvailable()
    {
        return m_attackZoneManager.AreZonesAvailable(m_parentAI.GetZoneTypeFromAttackType());
    }

    public ref AttackZone GetCurrentAttackZone()
    {
        return ref m_currentAttackZone;
    }

    public void SetCurrentAttackZone( ref AttackZone zoneToSet )
    {
        m_currentAttackZone = zoneToSet;
    }

    public ref AttackZone GetOccupiedAttackZone()
    {
        return ref m_occupiedAttackZone;
    }

    public void SetOccupiedAttackZone( ref AttackZone zoneToSet )
    {
        m_occupiedAttackZone = zoneToSet;
    }
}

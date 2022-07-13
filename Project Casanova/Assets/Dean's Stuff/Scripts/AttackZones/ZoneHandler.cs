using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneHandler
{
    private EnemyAI m_parentAI;
    private AttackZoneManager m_attackZoneManager;
    private AttackZone m_currentAttackZone;
    private AttackZone m_occupiedAttackZone;
    private AttackZone m_reservedZone;
    private bool m_reserveZone = false;
    private Vector3 m_reservedPos;
    private float m_targetAngle;
    private int m_closestZoneNum = 0;

    public void Init(ref EnemyAI enemyAI, ref AttackZoneManager attackZoneManager)
    {
        m_parentAI = enemyAI;
        m_attackZoneManager = attackZoneManager;
    }

    public void Update()
    {
        m_currentAttackZone = m_attackZoneManager.FindAttackZone(m_parentAI);

        OccupiedZoneCheck();

        if (m_reserveZone)
        {
            UpdateReservedPos();
        }
    }

    private void UpdateReservedPos()
    {
        m_reservedPos = m_attackZoneManager.GetSpecifiedPos(m_targetAngle, m_parentAI.GetStrafeDist());
    }

    private void OccupiedZoneCheck()
    {
        if (!m_reserveZone && m_occupiedAttackZone != null && !IsInOccupiedZone())
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

    private AttackZone FindClosestZone()
    {
        m_closestZoneNum = m_attackZoneManager.GetZoneNumByAngle(m_parentAI);

        if (m_parentAI.GetAttackingType() == AttackingType.Passive)
        {
            return m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Passive);
        }
        else
        {
            return m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Active);
        }
    }

    private AttackZone FindClosestAvailableZone()
    {
        m_closestZoneNum = m_attackZoneManager.GetZoneNumByAngle(m_parentAI);

        int dirToCheckFirst = Random.Range(0, 2);
        int zoneNumOffset = 1;

        AttackZone zoneToReturn = null;

        // Passive Attacker
        if (m_parentAI.GetAttackingType() == AttackingType.Passive)
        {
            // Closest Zone
            zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Passive);

            // While loop to loop through zones to find the next available zone
            while (!zoneToReturn.IsAvailable())
            {
                int leftNum = m_closestZoneNum + zoneNumOffset % m_attackZoneManager.GetTotalZonesNum();
                int rightNum = m_closestZoneNum - zoneNumOffset;

                if (rightNum < 0)
                {
                    rightNum = m_attackZoneManager.GetTotalZonesNum() - zoneNumOffset;
                }

                // Randomising which direction to check so AI don't bias a direction
                if (dirToCheckFirst == 0)
                {
                    zoneToReturn =  m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Passive);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Passive);
                    }
                }
                else
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Passive);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Passive);
                    }
                }

                zoneNumOffset++;
            }
        }

        // Active Attacker
        else
        {
            zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Active);

            while (!zoneToReturn.IsAvailable())
            {
                int leftNum = m_closestZoneNum + zoneNumOffset % m_attackZoneManager.GetTotalZonesNum();
                int rightNum = m_closestZoneNum - zoneNumOffset;

                if (rightNum < 0)
                {
                    rightNum = m_attackZoneManager.GetTotalZonesNum() - zoneNumOffset;
                }

                if (dirToCheckFirst == 0)
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Active);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Active);
                    }
                }
                else
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Active);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Active);
                    }
                }

                zoneNumOffset++;
            }
        }

        return zoneToReturn;
    }

    public void ReserveZone(AttackZone zoneToReserve)
    {
        m_reserveZone = true;
        m_reservedZone = zoneToReserve;
        m_occupiedAttackZone = m_reservedZone;
        m_targetAngle = Random.Range(m_reservedZone.GetAngleStart(), m_reservedZone.GetAngleEnd());
        m_reservedZone.SetOccupant(m_parentAI);
    }

    public void ReserveClosestZone()
    {
        ReserveZone(FindClosestAvailableZone());
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

    public Vector3 GetReservedPos()
    {
        return m_reservedPos;
    }

    public void UnreserveZone()
    {
        m_reservedZone.EmptyZone();
        m_reservedZone = null;
        m_reserveZone = false;
    }

    public void SetReserveFlag( bool shouldReserve )
    {
        m_reserveZone = shouldReserve;
    }

    public bool GetReserveFlag()
    {
        return m_reserveZone;
    }
}

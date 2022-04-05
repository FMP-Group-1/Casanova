public enum ZoneType
{
    Passive,
    Active
}

public class AttackZone
{
    private bool m_isOccupied = false;
    private ZoneType m_zoneType = ZoneType.Active;

    public AttackZone(bool isOccupied, ZoneType zoneType)
    {
        m_isOccupied = isOccupied;
        m_zoneType = zoneType;
    }

    public bool IsOccupied()
    {
        return m_isOccupied;
    }

    public void SetOccupied(bool isOccupied)
    {
        m_isOccupied = isOccupied;
    }

    public ZoneType GetZoneType()
    {
        return m_zoneType;
    }

    public void SetZoneType(ZoneType typeToSet)
    {
        m_zoneType = typeToSet;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField]
    GateMover m_cellExit;
    [SerializeField]
    GateMover m_armoryExit;
    [SerializeField]
    GateMover m_guardRoomEntrance;
    [SerializeField]
    GateMover m_guardRoomExit;
    [SerializeField]
    GateMover m_arenaExitGate;


    //I know this is awful, but it just needed to be made to work and time was not my friend
    public void OpenCellHallExitGate()
    {
        m_cellExit.OpenGate();
    }
    public void OpenArmoryExitGate()
    {
        m_armoryExit.OpenGate();
        m_guardRoomEntrance.OpenGate();
    }
    public void OpenGuardRoomExitGate()
	{
        m_guardRoomExit.OpenGate();

    }
    public void OpenArenaExitGate()
    {
        m_arenaExitGate.OpenGate();
    }
}

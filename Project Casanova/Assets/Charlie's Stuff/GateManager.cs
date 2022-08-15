using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField]
    GateMover m_cellsExit;
    [SerializeField]
    GateMover m_armoryExit;
    [SerializeField]
    GateMover m_guardRoomEntrance;
    [SerializeField]
    GateMover m_guardRoomExit;
    [SerializeField]
    GateMover m_arenaExitGate;

    public void ResetGate(Room roomExit)
	{
        switch ( roomExit )
        {
            case Room.Hall:
                m_cellsExit.ResetGate();
                break;
            case Room.Armory1:
                //We respawn inside the armoury, trapped
                break;
            case Room.Armory2:
                m_armoryExit.ResetGate();
                m_guardRoomEntrance.ResetGate();
                break;
            case Room.GuardRoom:
                m_guardRoomExit.ResetGate();
                break;
        }
	}
    //I know this is awful, but it just needed to be made to work and time was not my friend
    public void OpenCellHallExitGate()
    {
        m_cellsExit.OpenGate();
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

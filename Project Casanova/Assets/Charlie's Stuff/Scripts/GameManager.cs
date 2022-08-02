using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Room
{
    Cell,
    Hall,
    Armory,
    GuardRoom,
    Arena
}

public class GameManager : MonoBehaviour
{
    private Room m_currentRoom;
    private UIManager m_uiManager;
    private RespawnManager m_respawnManager;
    private GateManager m_gateManager;

    void Start()
    {
        //Very Begining of Game
        m_currentRoom = Room.Cell;
        m_uiManager = GetComponent<UIManager>();
        m_respawnManager = GetComponent<RespawnManager>();
        m_gateManager = GetComponent<GateManager>();

        m_respawnManager.SetRespawnPoint( Room.Cell );
        m_uiManager.BeginScene();

        EventManager.StartSpawnEnemiesEvent( 0 );
        //EventManager.StartWakeEnemiesEvent( 0 );
        EventManager.StartSpawnEnemiesEvent( 1 );
    }

	private void Update()
	{
		
	}

    public void CompleteRoom( Room room )
	{
        switch( room )
        {
            case Room.Hall:
                m_gateManager.OpenCellHallExitGate();
                m_respawnManager.SetRespawnPoint( Room.Hall );
                break;
            case Room.Armory:
                m_gateManager.OpenArmoryExitGate();
                m_respawnManager.SetRespawnPoint( Room.Armory );
                break;
            case Room.GuardRoom:
                m_gateManager.OpenGuardRoomExitGate();
                m_respawnManager.SetRespawnPoint( Room.GuardRoom );
                break;
            case Room.Arena:
                //Entering Arena sets Arena respawn point
                //no respawn, you can't die now
                m_gateManager.OpenArenaExitGate();
                break;
        }
	}

}

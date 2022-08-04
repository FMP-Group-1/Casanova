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

    private AIManager m_aiManager;

    private bool m_roomComplete;

    void Start()
    {
        //Very Begining of Game
        m_currentRoom = Room.Cell;
        m_uiManager = GetComponent<UIManager>();
        m_respawnManager = GetComponent<RespawnManager>();
        m_gateManager = GetComponent<GateManager>();
        m_aiManager = GetComponent<AIManager>();

        m_respawnManager.SetRespawnPoint( Room.Cell );
        m_uiManager.BeginScene();

        EventManager.StartSpawnEnemiesEvent( 0 );
        EventManager.StartSpawnEnemiesEvent( 1 );
    }

	private void Update()
	{
        if( !m_roomComplete )
		{

            switch( m_currentRoom )
		    {
                case Room.Cell:

                    break;

                case Room.Hall:

                    //If you're in the hall, check if this group is a1l dead
                    if( m_aiManager.RemainingEnemiesInGroup( 0 ) <= 0 )
                    {
                        CompleteRoom( Room.Hall );
                    }
                    break;
                case Room.Armory:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 1 ) <= 0 )
                    {
                        CompleteRoom( Room.Armory );
                    }
                    break;
                case Room.GuardRoom:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 2 ) <= 0 )
                    {
                        CompleteRoom( Room.GuardRoom );
                    }
                    break;
                case Room.Arena:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 3 ) <= 0 )
                    {
                        CompleteRoom( Room.Arena );
                    }
                    break;
            }
        }

    }

    public void Die()
	{
        m_uiManager.DisplayDeathUI();
        StartCoroutine( BeginRespawn() );

    }
    private IEnumerator BeginRespawn()
    {
        yield return new WaitForSeconds( 4.0f );

        //Both these need same value. Value will fade out for that time, and delay the ACTUAL stuff until then
        float fadeTimeAndDelay = 4.0f;
        m_uiManager.Respawn( fadeTimeAndDelay );
        StartCoroutine( m_respawnManager.Respawn( fadeTimeAndDelay ) );
    }

    public void ActuallyRespawn()
    {
        m_aiManager.DeactivateActiveEnemies();

        Room respawnPoint = m_respawnManager.GetRespawnPoint();
        switch ( respawnPoint )
        {
            case Room.Cell:
                EventManager.StartSpawnEnemiesEvent( 0 );
                break;
            case Room.Hall:
                EventManager.StartSpawnEnemiesEvent( 1 );
                break;
            case Room.Armory:

                EventManager.StartSpawnEnemiesEvent( 2 );
                break;
            case Room.GuardRoom:

                EventManager.StartSpawnEnemiesEvent( 3 );
                break;
            case Room.Arena:

                EventManager.StartSpawnEnemiesEvent( 3 );
                break;
        }

        m_gateManager.ResetGate( respawnPoint );
        //This sets this room as active room
        //CompleteRoom( m_respawnManager.GetRespawnPoint() );
    }

    public void EnterRoom( Room room )
	{
        m_roomComplete = false;
        m_currentRoom = room;
        Debug.Log( m_currentRoom );
	}

    public void CompleteRoom( Room room )
    {
        m_roomComplete = true;
        switch( room )
        {
            case Room.Cell:
                break;
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

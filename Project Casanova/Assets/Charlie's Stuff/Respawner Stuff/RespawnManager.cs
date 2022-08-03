using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RespawnPoint
{
    Cell,
    Hall,
    Armory,
    GuardRoom,
    Arena
}
public class RespawnManager : MonoBehaviour
{
    [SerializeField]
    private Transform[] respawnPoints;

    private Room currentRespawnPoint;

    private GameObject m_player;

    private GameManager m_gameManager;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag( "Player" );
        m_gameManager = gameObject.GetComponent<GameManager>();
    }

    public void SetRespawnPoint( Room newRespawnPoint )
    {
        currentRespawnPoint = newRespawnPoint;
        switch( newRespawnPoint )
        {
            case Room.Cell:
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log( "Current Respawn Point: " + currentRespawnPoint.ToString() );
    }












    public IEnumerator Respawn( float delay )
    {
        yield return new WaitForSeconds( delay );
        //Honestly cannot think of a beter way without a major overhaul that i DO NOT have time for (Made a note in the parent to keep it on order)
        int enumIntValue = (int)currentRespawnPoint;

        m_player.GetComponent<PlayerDamageManager>().Respawn( respawnPoints[ enumIntValue ].transform );

        m_gameManager.CompleteRoom( currentRespawnPoint );
        //Bring player back to life shit

        switch ( currentRespawnPoint )
        {
            case Room.Cell:

                EventManager.StartSpawnEnemiesEvent( 0 );
                EventManager.StartSpawnEnemiesEvent( 1 );

                break;
            case Room.Hall:

                //Respwan the trigger box
                EventManager.StartSpawnEnemiesEvent( 1 );
                EventManager.StartSpawnEnemiesEvent( 2 );

                break;
            case Room.Armory:

                //Respwan the trigger box
                EventManager.StartSpawnEnemiesEvent( 2 );
                EventManager.StartSpawnEnemiesEvent( 3 );

                break;
            case Room.GuardRoom:

                //Respwan the trigger box
                EventManager.StartSpawnEnemiesEvent( 3 );

                break;
            case Room.Arena:

                //Respwan the trigger box
                EventManager.StartSpawnEnemiesEvent( 3 );

                break;
        }
    }
}


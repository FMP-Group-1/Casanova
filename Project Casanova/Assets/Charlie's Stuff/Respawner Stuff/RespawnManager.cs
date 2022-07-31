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

    [HideInInspector]
    public static RespawnPoint currentRespawnPoint = RespawnPoint.Cell;

    private GameObject m_player;
    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag( "Player" );
    }


    public void SetRespawnPoint( RespawnPoint newRespawnPoint )
    {
        currentRespawnPoint = newRespawnPoint;
        switch( newRespawnPoint )
        {
            case RespawnPoint.Cell:
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log( "Current Respawn Point: " + currentRespawnPoint.ToString() );
    }












    public void Respawn()
    {
        int enumIntValue = (int)currentRespawnPoint;
        m_player.GetComponent<CharacterController>().enabled = false;
        m_player.transform.position = respawnPoints[ enumIntValue ].position;
        m_player.GetComponent<CharacterController>().enabled = true;




        EventManager.StartSpawnEnemiesEvent( enumIntValue );
        EventManager.StartWakeEnemiesEvent( enumIntValue );
        //Prep Next Room
        EventManager.StartSpawnEnemiesEvent( enumIntValue+1 );




    }















}


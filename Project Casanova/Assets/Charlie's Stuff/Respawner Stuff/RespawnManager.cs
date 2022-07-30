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
    private GameObject[] respawnPoints;

    [HideInInspector]
    public static RespawnPoint currentRespawnPoint = RespawnPoint.Cell;
    // Start is called before the first frame update
    void Start()
    {

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

    public void CompleteCorridor()
    {
        currentRespawnPoint = RespawnPoint.Hall;
    }
    public void CompleteArmory()
    {

        currentRespawnPoint = RespawnPoint.Armory;
    }
    public void CompleteGuardRoom()
    {
        currentRespawnPoint = RespawnPoint.GuardRoom;
    }
    public void SpawnInArena( int wave )
    {
        currentRespawnPoint = RespawnPoint.Arena;
        switch( wave )
		{
            default:
                break;

            case 1:
                break;
		}
    }
}

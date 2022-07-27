using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RespawnPoint
{
    Cell,
    Hall,
    Armory,
    Final
}
public class RespawnManager : MonoBehaviour
{
    public static RespawnPoint currentRespawnPoint = RespawnPoint.Cell;
    // Start is called before the first frame update
    void Start()
    {


}

    // Update is called once per frame
    void Update()
    {
        
    }
}

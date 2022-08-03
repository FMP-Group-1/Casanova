using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField]

    private Room whatPointThisSets;

    RespawnManager respawnManager;
    // Start is called before the first frame update
    void Start()
    {
        //respawnManager = GameObject.FindGameObjectWithTag( "RespawnManager" ).GetComponent<RespawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter( Collider other )
	{
		if (other.gameObject.tag == "Player" )
		{
            respawnManager.SetRespawnPoint( whatPointThisSets );
		}
	}
}

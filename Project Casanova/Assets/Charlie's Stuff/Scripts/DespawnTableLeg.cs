using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnTableLeg : MonoBehaviour
{

    public void Despawn()
	{
        StartCoroutine(TimerDespawn());
	}

    private IEnumerator TimerDespawn()
	{
        yield return new WaitForSeconds( 10f );
        gameObject.SetActive( false ); 
    }
}

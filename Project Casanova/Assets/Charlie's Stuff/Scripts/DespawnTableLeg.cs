using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnTableLeg : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

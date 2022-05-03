using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollisionManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter( Collider other )
    {
        if ( other.gameObject.tag == "Enemy" )
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            if ( enemy.GetState() == AIState.Sleeping )
            {
                enemy.WakeUpAI( WakeTrigger.Attack );
            }

            enemy.TakeDamage( 50f );

            //m_collider.enabled = false;
        }
    }
}

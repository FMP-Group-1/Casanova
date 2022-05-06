using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollisionManager : MonoBehaviour
{
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Sword Collision with Enemy
    **************************************************************************************/
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

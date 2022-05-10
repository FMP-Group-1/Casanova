using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollisionManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip( "Damage" )]
    private float m_swordDamage = 10f;
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Sword Collision with Enemy
    **************************************************************************************/
    private void OnTriggerEnter( Collider other )
    {
        //We've collided, but is it with an enemy?
        if ( other.gameObject.tag == "Enemy" )
        {
            //Yes? Okay, get the enemy
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            //Asleep? Wake it up
            if ( enemy.GetState() == AIState.Sleeping )
            {
                enemy.WakeUpAI( WakeTrigger.Attack );
            }

            //Then hurt them
            enemy.TakeDamage( m_swordDamage );
        }
    }
}

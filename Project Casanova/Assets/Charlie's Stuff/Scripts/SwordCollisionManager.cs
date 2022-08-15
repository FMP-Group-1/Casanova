using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollisionManager : MonoBehaviour
{

    private float m_damage;

    [SerializeField, Range(0.0f, 2.0f)]
    private float m_damageMultiplier = 1.0f;

    // Dean Note: Adding a reference to the sound handler in here for collision SFX
    private PlayerSoundHandler m_soundHandler;

    private void Start()
    {
        // Dean Note: I know this is a hideous line, but I can't think of a better way at this moment
        m_soundHandler = GameObject.FindGameObjectWithTag(Settings.g_playerTag).GetComponent<PlayerController>().GetSoundHandler();
    }

    string thingCollided;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Dean Pearce
    *         Charlie Taylor
    *
    * Description: Sword Collision with Enemy
    **************************************************************************************/
    private void OnTriggerEnter( Collider other )
    {
        thingCollided = other.name;
        //We've collided, but is it with an enemy?
        if ( other.gameObject.tag == "Enemy" )
        {
            //Yes? Okay, get the enemy
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            //Asleep? Wake it up
            if ( enemy.GetState() == AIState.Sleeping )
            {
                enemy.WakeUpAI();
            }

            //Then hurt them
            if (enemy.GetState() != AIState.Dead )
			{
                enemy.GetHealthManager().TakeDamage( transform, m_damage * m_damageMultiplier );

                m_soundHandler.PlayNormalCollisionSFX();
            }
        }
    }


	private void Update()
	{
        Debug.Log( thingCollided );
	}

	public void SetDamage( float damage )
	{
        m_damage = damage;
	}
}

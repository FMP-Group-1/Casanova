using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDamageManager : MonoBehaviour
{
    [SerializeField]
    private float m_health = 100;
    private bool m_invulnerable = false;
    //The Animator Component
    private Animator m_animator;
    [SerializeField]
    private float m_invulnerableTime = 1f;

    private EnemyAI m_enemyAI;
    private PlayerController m_playerController;

    private enum CharacterType
    {
        Enemy,
        Player
	}

    [SerializeField]
    private CharacterType m_characterType;

    private int an_stumble;

    void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponent<Animator>();
        an_stumble = Animator.StringToHash( "stumble" );


        if (m_characterType == CharacterType.Player )
		{
            m_playerController = GetComponent<PlayerController>();
		}
        else if ( m_characterType == CharacterType.Enemy )
		{
            m_enemyAI = GetComponent<EnemyAI>();
		}

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(Transform othersTransform, float damage = 30f )
    {


        if( m_characterType == CharacterType.Player )
        {

        }
        else if( m_characterType == CharacterType.Enemy )
        {
            m_health -= damage;

            if( m_enemyAI.GetState() != AIState.Sleeping )
            {
                //m_enemyAI.ResetLastUsedAnimTrigger();
                //m_enemyAI.PlayDamageAnim();
            }

            if( m_health <= 0.0f )
            {
                Die();
            }
        }

        // if ( enemy )
        /*
            
         */

        if( !m_invulnerable )
        {


            //If Player
            m_playerController.DeactivateAllTheCanStuff();
            //rotate to face the thing, then animate 

            Vector3 startPosition= transform.position;
            //... to....
            Vector3 targetPosition = othersTransform.position;

            //This is now the angle (between -180 and 180) between origin and target
            float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );



            //Create a Quaternion, with new angle, to be what we want the new rotation point to be
            Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );

            //Lerp instead of SET it as I believe that will stop issues like frame rate skipping past or soemthing
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.99f );

            m_animator.SetTrigger( an_stumble );
            m_invulnerable = true;
            StartCoroutine( ResetInvulnerable( m_invulnerableTime ) );
        }



	}

    private void Die()
    {
        m_health = 0.0f;
        //SetAIState( AIState.Dead );
        //m_aiManager.UnregisterAttacker( this );
    }


    public IEnumerator ResetInvulnerable(float timer)
	{
        yield return new WaitForSeconds( timer );
        m_invulnerable = false;
        m_playerController.ResetAllTheCanStuff();
    }


    public void SetInvulnerable()
	{
        m_invulnerable = true;
        StartCoroutine(ResetInvulnerable( m_invulnerableTime ) );

	}

}

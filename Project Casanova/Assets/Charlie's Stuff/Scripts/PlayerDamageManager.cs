using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageManager : CharacterDamageManager
{
    private PlayerController m_playerController;

    GameObject m_gameController;

    private float m_respawnDelay;

    protected override void Start()
    {
        base.Start();
        m_playerController = GetComponent<PlayerController>();
        m_gameController = GameObject.FindGameObjectWithTag( "GameController" );
    }

    // Update is called once per frame
    void Update()
    {
    }


    public override void TakeDamage(Transform othersTransform, float damage = 30f )
    {
        if ( GetAlive() )
        {
            if ( !GetInvulnerable() )
            {
                m_playerController.LoseControl();

            }
            base.TakeDamage( othersTransform, damage );
        }
    }

    public void Respawn( Transform spawnPos )
	{
        //need to test if i actually need to disable controller
        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPos.position;
        GetComponent<CharacterController>().enabled = true;

        SetAlive( true );
        SetHealth( 100f );
        SetInvulnerable( false );
        GetAnimator().SetTrigger( "Respawn" );

    }

    protected override void PlayDamageSFX()
    {
        m_playerController.GetSoundHandler().PlayDamageSFX();
    }

    protected override void PlayDeathSFX()
    {
        m_playerController.GetSoundHandler().PlayDeathSFX();
    }

    protected override void Die()
    {
        base.Die();
        m_playerController.LoseControl();

        m_gameController.GetComponent<GameManager>().Die();
        /*
        m_animator.ResetTrigger( "light" );
        m_animator.ResetTrigger( "heavy" );
        m_animator.ResetTrigger( "attacked" );
        m_animator.ResetTrigger( "dodge" );
        m_animator.ResetTrigger( an_getHitTrigger );
        m_animator.ResetTrigger( an_death );*/
        
    }

    public void debugDie()
	{
        Die();
	}


    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.RegainControl();

    }
}

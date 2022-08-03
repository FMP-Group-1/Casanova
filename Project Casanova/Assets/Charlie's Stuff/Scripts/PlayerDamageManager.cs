using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageManager : CharacterDamageManager
{
    private PlayerController m_playerController;

    GameObject m_gameController;

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

    protected override void Die()
    {
        base.Die();
        DisplayDeathMessage();
        m_playerController.LoseControl();

        StartCoroutine( Respawn() );
    }

    public void debugDie()
	{
        Die();
	}

    private IEnumerator Respawn()
	{
        yield return new WaitForSeconds( 6.0f );
        m_gameController.GetComponent<RespawnManager>().Respawn();
	}

    private void DisplayDeathMessage()
	{
        m_gameController.GetComponent<UIManager>().DisplayDeathUI();
    }

    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.RegainControl();

    }
}

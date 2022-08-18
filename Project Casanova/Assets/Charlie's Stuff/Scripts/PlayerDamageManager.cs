using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: PlayerDamageManager
*
* Author: Charlie Taylor
*
* Description: Child of CharacterDamageManager to affect just the Player object
**************************************************************************************/
public class PlayerDamageManager : CharacterDamageManager
{
    //Reference to the PlayerContrller script
    private PlayerController m_playerController;
    //Game Controller
    private GameObject m_gameController;
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Call base setup and then fill member vars with components and objects
    **************************************************************************************/
    protected override void Start()
    {
        base.Start();
        m_playerController = GetComponent<PlayerController>();
        m_gameController = GameObject.FindGameObjectWithTag( Settings.g_controllerTag );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TakeDamage
    * Parameters: Transform othersTransform, float damage
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Take damage from enemies, adding the player exclusive LoseControl()
    **************************************************************************************/
    public override void TakeDamage( Transform othersTransform, float damage )
    {
        //Only run if alive
        if ( GetAlive() )
        {
            //Only run if not invulnerable
            if ( !GetInvulnerable() )
            {
                //Player does have staggerable functionality in base, but that is only utilised by enemies for now
                m_playerController.LoseControl();

                //Only bother calling if this is true as it checks the same stuff
                base.TakeDamage( othersTransform, damage );
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Respawn
    * Parameters: Transform spawnPos
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Respawn the player at the set spawn Position fed in by Respawn Manager
    **************************************************************************************/
    public void Respawn( Transform spawnPos )
	{
        //Move
        //This works as in the project settings > Physics, I enabled sync transforms
        transform.position = spawnPos.position;

        //Reset values
        SetAlive( true );
        ResetHealth();
        SetInvulnerable( false );
        UpdateHealthBar();
        //Only place this trigger is used.

        ResetAnimTriggers();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetAnimTriggers
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Reset animation triggers for player if any were active
    **************************************************************************************/
    private void ResetAnimTriggers()
	{
        GetAnimator().SetTrigger( "Respawn" );
        
        GetAnimator().ResetTrigger( "light" );
        GetAnimator().ResetTrigger( "heavy" );
        GetAnimator().ResetTrigger( "attacked" );
        GetAnimator().ResetTrigger( "dodge" );
        GetAnimator().ResetTrigger( GetStaggerAnimTrigger() );
        GetAnimator().ResetTrigger( GetDieAnimTrigger() );
        GetAnimator().SetBool( "comboActive", false );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayDamageSFX
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Play the player's damage SFX
    **************************************************************************************/
    protected override void PlayDamageSFX()
    {
        m_playerController.GetSoundHandler().PlayDamageSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayDeathSFX
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Play the player's death SFX
    **************************************************************************************/
    protected override void PlayDeathSFX()
    {
        m_playerController.GetSoundHandler().PlayDeathSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Die
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Player exclusive Die override.
    *              Call Game Manager's die, to prep for respawn
    **************************************************************************************/
    protected override void Die()
    {
        base.Die();
        m_playerController.LoseControl();

        m_gameController.GetComponent<GameManager>().Die();
        
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetInvulnerable
    * Parameters: float timer
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Wait a set time and then reset invulnerable
    **************************************************************************************/
    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.RegainControl();

    }
}

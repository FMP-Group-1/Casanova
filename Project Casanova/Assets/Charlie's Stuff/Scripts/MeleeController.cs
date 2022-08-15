using System.Collections;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
    private Animator m_animator;

    //This will be swapped to singular input actions, as the melee script does no need to know about moving
    private PlayerControls m_playerControls;

    //Player Controller Script
    private PlayerController m_playerController;

    [SerializeField]
    private SwordCollisionManager m_currentWeaponManager;
    private Collider m_weaponCollider;

    [SerializeField]
    private bool m_canStartNextAttack = true;

    //For box collider rotations
    //Object with the collider that actually rotates

    //How fast the player rotates at begining of an attack
    [SerializeField, Range(0f, 1f)]
    private float m_rotateSpeed = 0.2f;


    [SerializeField]
    private Transform m_sphereColliderTransform;

    [SerializeField]
    ParticleSystem m_swordTrail;

    [Header("Ground Pound")]
    [SerializeField, Range(0f, 5f)]
    float m_timeToGrow = 1f;
    [SerializeField, Range(5f, 10f)]
    float m_maxSphereSize = 7;

    //Attack Enum
    private enum Attack
    {
        Nothing,
        Light,
        Heavy,
    }
    //Member version of attack
    private Attack m_attackType;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Setup any variables or components
    **************************************************************************************/
    void Start()
    {
        m_weaponCollider = m_currentWeaponManager.gameObject.GetComponent<Collider>();

        m_playerControls = new PlayerControls();
        m_swordTrail.Stop();

        m_playerController = GetComponent<PlayerController>();

        m_playerControls.Enable();
        m_animator = GetComponent<Animator>();
        m_weaponCollider.enabled = false;
    }




    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Activate Mechanim Triggers and bools, allowing attacks to flow
    **************************************************************************************/
    void Update()
    {
        if ( !Settings.g_paused ) 
        { 
            //ACTUAL APPLICATION OF AN ATTACK, STUFF LATER IS BASED ON THIS HERE
            if ( m_playerController.GetGrounded() )
			{
                //Light Attack
                if ( m_playerControls.Combat.LightAtatck.triggered )
                {
                    m_attackType = Attack.Light;
                }
                //Heavy Attack
                if ( m_playerControls.Combat.HeavyAttack.triggered )
                {
                    m_attackType = Attack.Heavy;
                }

                if ( m_playerControls.Combat.HeavyAttack.IsPressed() )
                {
                    m_animator.SetBool( "whirlwindHeld", true ) ;
                }
                else
		        {
                    m_animator.SetBool( "whirlwindHeld", false );
                }
                //Add a (NON COROUTINE BASED) timer to check if it should be cancelled???? like 1 second
            }

            //Actually begin the attack stuff
            /* When you click an input, as above, you assign m_attackType to Light, Heavy etc
             * If you can start an attack (Be it, you going from idle or are available to do so
             * in a combo), and you are attacking, we can now enter this statement
             */
            if ( m_canStartNextAttack && m_attackType != Attack.Nothing )
            {

                //Stop being able to move or fall or rotate because we are in an attack
                m_playerController.m_canMove = false;
                m_playerController.m_canFall = false;
                m_playerController.m_canRotate = false;

                //We are attacking, so stop being able to again. (It is reset from CollisionsEnd)
                m_canStartNextAttack = false;
                m_animator.SetTrigger( "attacked" );
                //Combo has begun
                m_animator.SetBool( "comboActive", true );

                //What attack type?
                switch ( m_attackType )
                {
                    case Attack.Light:

                        m_animator.SetTrigger( "light" );
                        break;

                    case Attack.Heavy:

                        m_animator.SetTrigger( "heavy" );
					    break;

				    case Attack.Nothing:
                        Debug.Log( "You've reset the Attack type to nothing before executing the Switch. This should not happen" );
                        break;

			    }
			    //Next queued attack is nothing, until we add one in next run of update (If we click something, obviously)
			    m_attackType = Attack.Nothing;


                //The triggers now affect the animation played


                // Dean Note: Adding sound effect to play here, may need changing, let me know
                m_playerController.GetSoundHandler().PlayNormalAttackSFX();

            }
        }

    }

    public void SetCanAttack(bool canAttack)
	{
        m_canStartNextAttack = canAttack;
	}

    void SetAttackDamage( float damage = 10f )
	{
        m_currentWeaponManager.SetDamage( damage );

    }

    public void SwapWeapon(GameObject newWeapon, SwordCollisionManager manager)
	{
        m_swordTrail = newWeapon.GetComponentInChildren<ParticleSystem>();
        m_currentWeaponManager = manager;
        m_weaponCollider = newWeapon.GetComponent<Collider>();

    }




    /**************************************************************************************
    * Type: Function
    * 
    * Name: CollisionsStart
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attacks collisions should begin
    **************************************************************************************/
    public void CollisionsStart()
    {
        m_swordTrail.Play();
        //Set collider sweeper on
        m_weaponCollider.enabled =  true ;
        //You can dodge when the collisions are happening, as then when dodging it will turn off the collider.
        m_playerController.SetDodge( true );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CollisionsEnd
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attacks collisions should end
    **************************************************************************************/
    public void CollisionsEnd()
    {
        m_swordTrail.Stop();
        //Set collider sweeper off
        m_weaponCollider.enabled = false;

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CanStartNextAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the another Attack can begin
    **************************************************************************************/
    public void CanStartNextAttack()
    {
        //Begin attack again if you can
        m_canStartNextAttack = true;
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: AttackBegin
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attack animation FIRST begins.
    *               Used mainly for rotating the player to the direction they're inputting
    **************************************************************************************/
    private void AttackBegin(float damage)
	{
        //Set Damage based on this attack
        SetAttackDamage( damage );
        //Prevent dodging so it can't blend and leave the collider on
        m_playerController.SetDodge( false );

        //Current Position value
        Vector3 startPosition = transform.position;

        //Add the direction to where you are to get the vector
        Vector3 targetPosition = transform.position + m_playerController.GetMoveDirection();

        //Get target angle in degrees
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        //Debug.Log( targetAngle ); 

        Quaternion targetRotation = Quaternion.Euler( new Vector3 ( 0f, targetAngle, 0f ));


        //Only if the there is some level of input
        if ( m_playerController.GetMoveDirection() != Vector3.zero )
        {
            //Rotate player over a short time, to the place where the player is trying to move to
            StartCoroutine( RotatePlayer( targetRotation ) );

        }
        //otherweise we just attack forwards (Where player is facing)

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EndCombo
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attack combo ends, at the begining 
    *               of the leave anim, to reset variables that allow the player to move 
    *               or fall
    **************************************************************************************/
    public void EndCombo()
    {
        //Reset velocity to 0 so the player doesn't reach mach 4 in falling
        //m_playerController.m_playerVelocity.y = -6f; 
        CanStartNextAttack();
        m_playerController.m_canFall = true;
        m_playerController.m_canMove = true;
        m_playerController.m_canRotate = true;
        m_animator.SetBool( "comboActive", false );
    }

    /**************************************************************************************
    * Type: IEnumerator
    * 
    * Name: RotatePlayer
    * Parameters: Quaternion targetRotation
    * Return: null
    *
    * Author: Charlie Taylor
    *
    * Description: Over a set time, rotate the player to face the new direction
    **************************************************************************************/
    IEnumerator RotatePlayer( Quaternion targetRotation )
    {
        //While time is still less than the rotate time speed thing, rotate to the position
        for( var t = 0f; t < 1; t += Time.deltaTime / m_rotateSpeed )
        {
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, t );
            yield return null;
        }
	}















    private void GroundpoundActivated()
    {
        //m_playerController.m_playerVelocity.y -= Mathf.Sqrt( m_jumpForce * -3.0f * m_gravityValue )
        m_playerController.m_playerVelocity.y = -15f;
        //Begin plumetting to the ground

        m_playerController.m_canFall = true;

        m_playerController.m_canDodge = false;
    }

    private void GrounpoundLanded()
	{
        //AoE Attack
        Debug.Log( "Big AoE" );
        StartCoroutine( ExpandGroundpoundSphere() );
	}

    IEnumerator ExpandGroundpoundSphere()
	{
        float timer = 0;
        Vector3 MaxSize = new Vector3(m_maxSphereSize, m_maxSphereSize, m_maxSphereSize ); 


        while( timer < m_timeToGrow )
		{

            //Debug.Log( m_sphereColliderTransform.localScale );
            m_sphereColliderTransform.localScale = Vector3.Lerp( m_sphereColliderTransform.localScale, MaxSize, (timer/m_timeToGrow) );
            timer += Time.deltaTime;

            yield return null;
        }
        
        m_sphereColliderTransform.localScale = new Vector3( 0.5f, 0.5f, 0.5f );
    }
}

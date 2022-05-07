using System.Collections;
using UnityEngine;

public class melee : MonoBehaviour
{
    private Animator animator;

    private PlayerControls m_playerControls;

    private PlayerController m_playerController;

    [SerializeField]
    private bool canStartNextAttack = true;

    //[SerializeField]
    //private Text comboDebugText;

    [SerializeField]
    private Transform swordTip;
    [SerializeField]
    private Material lineMaterial;
    //[SerializeField]
    //private float lineTime = 1f;
    
    [SerializeField]
    private GameObject colliderSweepThing;

    [SerializeField]
    [Range(0f, 1f)]
    private float rotateSpeed = 0.2f;

    private enum Attack
    {
        Nothing,
        Light,
        Heavy,
    }

    private Attack attackType;

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
        m_playerControls = new PlayerControls();

        m_playerController = GetComponent<PlayerController>();

        m_playerControls.Enable();
        animator = GetComponent<Animator>();
        colliderSweepThing.SetActive(false);
    }




    // Update is called once per frame
    void Update()
    {
        //Attack straight from unarmed. No Blend Animation
        //Light Attack
        if ( m_playerControls.Combat.LightAtatck.triggered )
        {
            attackType = Attack.Light;

        }
        //Heavy Attack
        if ( m_playerControls.Combat.HeavyAttack.triggered )
        {
            attackType = Attack.Heavy;
        }
        //Heavy Attack
        if ( m_playerControls.Combat.Whirlwind.triggered )
        {
            attackType = Attack.Heavy;
            animator.SetTrigger( "whirlwind" );
        }

        //Basically, if you have reached the end of an attack, you are no longer attacking, but if "attackType" is not nothing, there's somehing queued up, so lets do it
        if ( canStartNextAttack && attackType != Attack.Nothing )
        {
            //Rotation stuff needs to go around here??????
            Vector3 targetDirection = m_playerController.GetMoveDirection();
            // So we are not attacking YET, but we want to

            //Stop being able to move or fall or rotate because we are in an attack
            m_playerController.canMove = false;
            m_playerController.canFall = false;
            m_playerController.canRotate = false;

            //We are attacking
            canStartNextAttack = false;
            animator.SetTrigger( "attacked" );
            //Combo has begun
            animator.SetBool( "comboActive", true );

            //What attack type?
            switch ( attackType )
            {
                case Attack.Light:

                    animator.SetTrigger( "light" );
                    break;

                case Attack.Heavy:

                    animator.SetTrigger( "heavy" );
					break;

				case Attack.Nothing:
                    Debug.Log( "You've reset the Attack type to nothing before executing the attack you dumb ass" );
                    break;

			}


			//Next queued attack is nothing, until we add one
			attackType = Attack.Nothing;

        }

        //rotate Collider Thingy to angle to the sword (ALL THE TIME)
        //Verbose for Readability
        //Rotate From...
        Vector3 startPosition = transform.position;
        startPosition.y = 0f;
        //... to....
        Vector3 targetPosition = swordTip.position;
        targetPosition.y = 0;

        //This is now the angle (between 0 and 180?) between origin and target
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        //Create a vector 3, with new angle, to be what we want the new rotation point to be
        Vector3 targetRotation = new Vector3 ( 0f, targetAngle, 0f );
        colliderSweepThing.transform.rotation = Quaternion.Euler(targetRotation);

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
        //comboDebugText.text += "\nColl. Start\n";
        colliderSweepThing.SetActive( true );

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
        //comboDebugText.text += "\nColl. End\n";
        canStartNextAttack = true;

        colliderSweepThing.SetActive( false );

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
    private void AttackBegin()
	{
        //The direction the player is inputing
        Vector3 moveDirection = transform.position + m_playerController.GetMoveDirection();


        //Current Position value
        Vector3 startPosition = transform.position;
        startPosition.y = 0f;

        Vector3 targetPosition = moveDirection;
        targetPosition.y = 0;

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
        //comboDebugText.text = "\nEnd\n";
        m_playerController.playerVelocity.y = 0f;

        m_playerController.canFall = true;
        m_playerController.canMove = true;
        m_playerController.canRotate = true;
        animator.SetBool( "comboActive", false );
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
        float inTime = rotateSpeed;
        for( var t = 0f; t < 1; t += Time.deltaTime / inTime )
        {
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, t );
            yield return null;
        }
	}
}

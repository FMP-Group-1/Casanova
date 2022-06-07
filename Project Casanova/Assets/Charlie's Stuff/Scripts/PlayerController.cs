using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent( typeof( CharacterController ) )]

public class PlayerController : MonoBehaviour
{
    //Input actions
    [SerializeField]
    [Tooltip( "Movement Control Input" )]
    private InputActionReference m_movementControl;
    [SerializeField]
    [Tooltip( "Jump Control Input" )]
    private InputActionReference m_jumpControl;

    [SerializeField]
    [Tooltip( "Dodge Control Input" )]
    private InputActionReference m_dodgeControl;

    //Player Components
    //Melee Controler
    private MeleeController m_meleeController;
    //The Animator Component
    private Animator m_animator;
    //Character Controller
    private CharacterController m_controller;

    //Player stats
    //Move Speed
    [SerializeField]
    [Tooltip("How fast player runs")]
    [Range( 3, 8 )]
    private float m_playerSpeed = 5.0f;
    [SerializeField]
    [Tooltip( "Player jump force" )]
    [Range( 1, 5 )]
    //How hard they jump
    private float m_jumpForce = 1.0f;
    //gravity
    [SerializeField]
    private float m_gravityValue = -9.81f;
    //How fast the player rotates when moving in a new direction
    [SerializeField]
    [Range( 2, 5 )]
    private float m_rotationSpeed = 4f;

    //Being Public is not finalised. This will become a getter/setter (Called in Melee.cs)
    public Vector3 m_playerVelocity;
    private bool m_groundedPlayer;
    //Camera's transform position, used for directional movmement/attacking
    private Transform m_cameraMainTransform;


    //Values that allow player to move or fall or rotate
    //All of these Being Public is not finalised. They will become getters/setters (Called in Melee.cs)

    [SerializeField]
    [Tooltip( "Movement damp time" )]
    [Range(0,1)]
    private float m_dampTime;
    public bool m_canMove = true;
    public bool m_canFall = true;
    public bool m_canRotate = true;
    public bool m_canDodge = true;

    //Normalised float of how much player is moving. Used in 1D Blend Tree
    private float m_moveAmount;


    //Animator Parameters
    private int an_movingSpeed;
    private int an_inAir;
    private int an_jumped;
    private int an_dodge;



    [Header("Debug Stuff")]

    [SerializeField]
    [Tooltip("Input Direction Visualiser")]
    private LineRenderer m_inputDirectionVisual;
    private Vector3 m_previousDirection;

    [SerializeField]
    [Tooltip( "Current Direction Visualiser" )]
    private LineRenderer m_currentDirectionFaced;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnEnable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enable input actions
    **************************************************************************************/
    private void OnEnable()
    {
        //Input actions need to be enabled
        m_movementControl.action.Enable();
        m_jumpControl.action.Enable();
        m_dodgeControl.action.Enable();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnDisable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Disable input actions
    **************************************************************************************/
    private void OnDisable()
    {
        //Input actions need to be diabled
        m_movementControl.action.Disable();
        m_jumpControl.action.Disable();
        m_dodgeControl.action.Disable();
    }

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
    private void Start()
    {
        //Asign stuff with GetComponent instead of in Inspector
        m_meleeController = GetComponent<MeleeController>();
        m_animator = GetComponent<Animator>();
        m_controller = gameObject.GetComponent<CharacterController>();
        m_cameraMainTransform = Camera.main.transform;



        an_movingSpeed = Animator.StringToHash( "movingSpeed" );
        an_inAir = Animator.StringToHash( "inAir" );
        an_jumped = Animator.StringToHash( "jumped" );
        an_dodge = Animator.StringToHash( "dodge" );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetPlayerInput
    * Parameters: n/a
    * Return: Vector3
    *
    * Author: Charlie Taylor
    *
    * Description: Return the players input values as a Vector3 direction
    **************************************************************************************/
    public Vector3 GetPlayerInput()
	{
        //The player inputs are a vector2, but everywhere I used it, it's in relation to a Vector 3, and
        //The Y value of the Vecrtor2 was always put in the Z and it was kind of confusing, so now it returns
        //a Vector3, with X and Z being the correct values.
        
        //Y always being 0 may be a waste, but it made it far more readable for me
        return new Vector3( m_movementControl.action.ReadValue<Vector2>().x, 0f, m_movementControl.action.ReadValue<Vector2>().y);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetMoveDirection
    * Parameters: n/a
    * Return: Vector3
    *
    * Author: Charlie Taylor
    *
    * Description: Using player inputs, and the camera direction, get the direction that 
    *               the player wants to go
    **************************************************************************************/
    public Vector3 GetMoveDirection()
    {
        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree animations
        m_moveAmount = Mathf.Clamp01( Mathf.Abs( GetPlayerInput().x ) + Mathf.Abs( GetPlayerInput().z ) );

        float NormalisedValue = m_moveAmount;


		#region Snapping Movement
		if ( m_moveAmount > 0.05f&& m_moveAmount < 0.55f )
		{
            m_moveAmount = 0.5f;
		}
        else if (m_moveAmount >= 0.55f )
		{
            m_moveAmount = 1;
		}
        else
		{
            m_moveAmount = 0;
		}
		#endregion

		//Animator variable set to move amount
		m_animator.SetFloat( an_movingSpeed, m_moveAmount, m_dampTime, Time.deltaTime );


        //Move Direction based on the camera angle
        Vector3 moveDirection = m_cameraMainTransform.forward * GetPlayerInput().z + m_cameraMainTransform.right * GetPlayerInput().x;
        //No move via Y. That's a jumping thing
        moveDirection.y = 0f;

        moveDirection.Normalize();

        Debug.Log( moveDirection );
        // Debug.Log( moveDirection );
        //moveDirection is now the direction I want to go
        return moveDirection;
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
    * Description: Update everything
    **************************************************************************************/
    void Update()
    {


        //Use the character Controller's isGrounded functionality to fill a member variable for readability
        m_groundedPlayer = m_controller.isGrounded;
        if ( m_groundedPlayer && m_playerVelocity.y < 0 )
        {
            m_playerVelocity.y = 0f;
        }


        //Dodging
        if ( m_dodgeControl.action.triggered && m_canDodge )
        {
            Dodge();
        }

        //If you're even touching Inputs
        if ( GetMoveDirection() != Vector3.zero )
        {
            if ( m_canMove )
            {
                //We are touching inputs AND we can move so, move
                //Multiply the move direction by  (speed * move amount) rather than just speed, and do it all before delta time
                m_controller.Move( ( GetMoveDirection() * ( m_playerSpeed * m_moveAmount ) ) * Time.deltaTime  );
            }

            //Rotate player when moving, not when Idle
            if ( m_canRotate )
            {
                //Get the angle where your inputs are, relative to camera
                float targetAngle = Mathf.Atan2( GetPlayerInput().x, GetPlayerInput().z ) * Mathf.Rad2Deg + m_cameraMainTransform.eulerAngles.y;
                //Pass that into a quaternion
                Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );
                //Rotate to it using rotation speed
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * m_rotationSpeed );
            }
        }


        //Jumping
        if ( m_jumpControl.action.triggered && m_groundedPlayer )
        {
            //Jumped
            m_animator.SetTrigger( an_jumped );
            m_playerVelocity.y += Mathf.Sqrt( m_jumpForce * -3.0f * m_gravityValue );
        }

        //If in air, at all
        if ( !m_groundedPlayer )
        {

            m_animator.SetBool( an_inAir, true );
        }
        else
        {
            //Landed
            m_animator.SetBool( an_inAir, false );
        }

        //Update a variable with how much you should be falling
        m_playerVelocity.y += m_gravityValue * Time.deltaTime;
        //And if you can fall, move that way.
        if ( m_canFall )
        {
            //Velocity is only used for falling and jumping
            m_controller.Move( m_playerVelocity * Time.deltaTime );
        }


        /////////////
        /// Debug ///
        /////////////
        
        //Rotate the Current Direction line renderer
        m_currentDirectionFaced.SetPosition( 0, transform.position );
        Vector3 facedDirection = transform.position + transform.forward;
        m_currentDirectionFaced.SetPosition( 1, facedDirection );

        //If there is a movement direction (So there's input)
        if ( GetMoveDirection() != Vector3.zero )
        {
            //Set vector3 for the line renderer
            m_previousDirection = GetMoveDirection();
        }
        //If no input, it will just use the last 

        //Set input direction Visualisers
        m_inputDirectionVisual.SetPosition( 0, transform.position );
        Vector3 inputDirection = transform.position + m_previousDirection;
        m_inputDirectionVisual.SetPosition( 1, inputDirection );

    }

    private void Dodge()
	{
       // Debug.Log( "Dodge" );
        m_animator.SetTrigger( an_dodge );
        m_canMove = false;
        m_canRotate = false;
        m_canDodge = false;

        m_meleeController.CollisionsEnd();
        //Current Position value
        Vector3 startPosition = transform.position;

        //Add the direction to where you are to get the vector
        Vector3 targetPosition;
        if ( GetPlayerInput() == Vector3.zero )
        {
            //Add the direction to where you are to get the vector
            targetPosition = transform.position + m_cameraMainTransform.forward*-1;

        }
        else
        {
            //Add the direction to where you are to get the vector
            targetPosition = transform.position + GetMoveDirection();
        }
        //Get target angle in degrees
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        //Debug.Log( targetAngle ); 

        transform.rotation = Quaternion.Euler( new Vector3( 0f, targetAngle, 0f ) );
        
    }
    private void ResetDodge()
	{
        m_canRotate = true;
        m_canMove = true;
        m_canDodge = true;
        m_canFall = true;
        m_meleeController.CanStartNextAttack();
	}
}
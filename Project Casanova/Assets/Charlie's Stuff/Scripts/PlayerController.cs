using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent( typeof( CharacterController ) )]

public class PlayerController : MonoBehaviour
{
    //Input actions
    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference jumpControl;

    //Player Components
    //The Animator Component
    private Animator animator;
    //Character Controller
    private CharacterController controller;

    //Player stats
    //Move Speed
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    //How hard they jump
    private float jumpForce = 1.0f;
    //gravity
    [SerializeField]
    private float gravityValue = -9.81f;
    //How fast the player rotates when moving in a new direction
    [SerializeField]
    private float rotationSpeed = 4f;

    public Vector3 playerVelocity;
    private bool groundedPlayer;
    //Camera's transform position, used for directional movmement/attacking
    private Transform cameraMainTransform;


    //Values that allow player to move or fall or rotate
    public bool canMove = true;
    public bool canFall = true;
    public bool canRotate = true;

    //Used for animating
    private float m_moveAmount;

    //Line renderer shows the player's input
    public LineRenderer inputDirectionVisual;

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
        movementControl.action.Enable();
        jumpControl.action.Enable();
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
        movementControl.action.Disable();
        jumpControl.action.Disable();
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
        animator = GetComponent<Animator>();
        controller = gameObject.GetComponent<CharacterController>();
        cameraMainTransform = Camera.main.transform;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetPlayerInput
    * Parameters: n/a
    * Return: Vector2
    *
    * Author: Charlie Taylor
    *
    * Description: Return the players input values as a Vector2
    **************************************************************************************/
    public Vector2 GetPlayerInput()
	{
        //The input values for Forward and Right, put into a member variable
        return movementControl.action.ReadValue<Vector2>();
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
        //Vector of what direction to move based on the inputs. Y is in the Z area, as the above holds the Z in it's Y value
        Vector3 moveInputs = new Vector3( GetPlayerInput().x, 0, GetPlayerInput().y );

        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree animations
        m_moveAmount = Mathf.Clamp01( Mathf.Abs( GetPlayerInput().x ) + Mathf.Abs( GetPlayerInput().y ) );
        animator.SetFloat( "movingSpeed", m_moveAmount );


        //Move Direction based on the camera angle
        Vector3 moveDirection = cameraMainTransform.forward * moveInputs.z + cameraMainTransform.right * moveInputs.x;
        //No move via Y. That's a jumping thing
        moveDirection.y = 0f;

        //Move is now the direction I want to go


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
        //Use the character Controller's isGrounded functionality to fill a member
        groundedPlayer = controller.isGrounded;
        if ( groundedPlayer && playerVelocity.y < 0 )
        {
            playerVelocity.y = 0f;
        }


        Vector3 moveDirection = GetMoveDirection();

        //Debug.Log( moveDirection );



        //If you're even touching Inputs
        if ( GetPlayerInput() != Vector2.zero )
        {
            if ( canMove )
            {
                //We are touching inputs AND we can move so, move
                controller.Move( moveDirection * Time.deltaTime * playerSpeed );
            }

            //Rotate player when moving, not when Idle
            if ( canRotate )
            {
                float targetAngle = Mathf.Atan2( GetPlayerInput().x, GetPlayerInput().y ) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * rotationSpeed );

            }
        }


        // Changes the height position of the player..
        if ( jumpControl.action.triggered && groundedPlayer )
        {
            //Jumped
            animator.SetTrigger( "jumped" );
            playerVelocity.y += Mathf.Sqrt( jumpForce * -3.0f * gravityValue );
        }

        if ( !groundedPlayer )
        {

            animator.SetBool( "inAir", true );
        }
        else
        {
            //Landed
            animator.SetBool( "inAir", false );
        }

        playerVelocity.y += gravityValue * Time.deltaTime;


        if ( canFall )
        {
            controller.Move( playerVelocity * Time.deltaTime );
        }


        //Debug.Log(playerVelocity);


        
        inputDirectionVisual.SetPosition( 0, transform.position );

        Vector3 inputDirection = transform.position + moveDirection;

        inputDirectionVisual.SetPosition( 1, inputDirection );



    }
}
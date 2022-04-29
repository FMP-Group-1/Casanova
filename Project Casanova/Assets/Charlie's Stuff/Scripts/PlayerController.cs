using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent( typeof( CharacterController ) )]

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference jumpControl;

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpForce = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 4f;

    private CharacterController controller;
    public Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraMainTransform;

    private Animator animator;

    public bool canMove = true;
    public bool canFall = true;
    public bool canRotate = true;

    //Used for animating
    private float m_moveAmount;



    private void OnEnable()
    {
        movementControl.action.Enable();
        jumpControl.action.Enable();
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        jumpControl.action.Disable();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        controller = gameObject.GetComponent<CharacterController>();
        cameraMainTransform = Camera.main.transform;
    }

    public Vector2 GetPlayerInput()
	{
        //The input values for Forward and Right, put into a member variable
        return movementControl.action.ReadValue<Vector2>();
    }

    public Vector3 GetMoveDirection()
    {

        //Vector of what direction to move based on the inputs. Y is in the Z area, as the above
        //Holds the Z in it's Y value
        Vector3 move = new Vector3( GetPlayerInput().x, 0, GetPlayerInput().y );

        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree
        m_moveAmount = Mathf.Clamp01( Mathf.Abs( GetPlayerInput().x ) + Mathf.Abs( GetPlayerInput().y ) );

        //Move is now based on the camera angle
        move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
        move.y = 0f;
        //Move is now the direction I want to go

        animator.SetFloat( "forwardSpeed", m_moveAmount );

        return move;
    }

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


    }



}
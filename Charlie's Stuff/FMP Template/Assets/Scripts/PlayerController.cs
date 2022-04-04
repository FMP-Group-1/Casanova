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
    private float jumpHeight = 1.0f;
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

    private bool movingWithAttack = false;

    private float moveWithAttackDistance;
    private float moveWithAttackTime;

    private Vector3 positionAtAttack;
    private Vector3 targetForAttack;



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

    void Update()
    {
        //Use the character Controller's isGrounded functionality to fill a member
        groundedPlayer = controller.isGrounded;
        if ( groundedPlayer && playerVelocity.y < 0 )
        {
            playerVelocity.y = 0f;
        }

        //Just the input values, put into a member variable
        Vector2 movement = movementControl.action.ReadValue<Vector2>();

        //Vector of what direction to move based on the inputs. Y is in the Z area, as the above
        //vector2 holds the Z in it's second value
        Vector3 move = new Vector3( movement.x, 0, movement.y );

        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree
        m_moveAmount = Mathf.Clamp01( Mathf.Abs( movement.x ) + Mathf.Abs( movement.y ) );

        //Move in direction of camera
        move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
        move.y = 0f;

        if ( canMove )
        {
            controller.Move( move * Time.deltaTime * playerSpeed );
        }


        // Changes the height position of the player..
        if ( jumpControl.action.triggered && groundedPlayer )
        {
            //Jumped
            animator.SetTrigger( "jumped" );
            playerVelocity.y += Mathf.Sqrt( jumpHeight * -3.0f * gravityValue );
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

        //Rotate player when moving

        //If you are moving at all
        if ( movement != Vector2.zero )
        {
            animator.SetBool( "moving", true );
            if ( canRotate )
            {
                float targetAngle = Mathf.Atan2( movement.x, movement.y ) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * rotationSpeed );

            }
        }
        else
        {

            animator.SetBool( "moving", false );
        }

        //Debug.Log(playerVelocity);

        animator.SetFloat( "forwardSpeed", m_moveAmount );



        Debug.Log( transform.forward );

    }



    /*public void MoveWithAttack( AnimationEvent animationEvent )
    {
        movingWithAtatck = true;
        moveWithAttackDistance = animationEvent.floatParameter;
        //Convert into to a float. the int is in hundreths of a second (0.14 = 14)
        moveWithAttackTime = ( float )animationEvent.intParameter / 100;


        positionAtAttack = transform.position;
        targetForAttack = new Vector3( 0, 0, transform.position.z + moveWithAttackDistance );

    }*/

    public IEnumerator MoveWithAttack( AnimationEvent animationEvent )
    {
        Vector3 forwardDirection = transform.forward;


        movingWithAttack = true;
        moveWithAttackDistance = animationEvent.floatParameter;
        //Convert into to a float. the int is in hundreths of a second (0.14 = 14)
        moveWithAttackTime = ( float )animationEvent.intParameter / 100;
        targetForAttack = new Vector3( transform.position.x + moveWithAttackDistance * forwardDirection.x, transform.position.y , transform.position.z + moveWithAttackDistance * forwardDirection.z );




        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        while ( elapsedTime < moveWithAttackTime )
        {

            transform.position = Vector3.Lerp( startingPos, targetForAttack, ( elapsedTime / moveWithAttackTime ) );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetForAttack;


    }
    private void EndMoveWithAttack()
    {
        movingWithAttack = false;

    }

}

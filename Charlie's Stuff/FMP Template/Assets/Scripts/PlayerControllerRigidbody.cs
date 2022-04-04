using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControllerRigidbody : MonoBehaviour
{
    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference jumpControl;

    private Animator animator;
    private Rigidbody m_rb;

    private Transform cameraMainTransform;

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpForce = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 4f;

    private bool groundedPlayer = true;



    //Used for animating
    private float m_moveAmount;


    public bool canMove = true;
    public bool canFall = true;
    public bool canRotate = true;

    //Move with Attacks

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
        m_rb = GetComponent<Rigidbody>();
        cameraMainTransform = Camera.main.transform;
        animator = GetComponent<Animator>();
    }
	private void FixedUpdate()
    {
        MovePlayer();

        //Falling
        if ( canFall )
        {
            //RB velocity add gravity value relative, just to Y
            m_rb.velocity += new Vector3( 0, gravityValue * Time.deltaTime, 0 );
        }
    }

	private void Update()
	{

        Jump();
    }

	private void MovePlayer()
	{
        //Just the input values, put into a member variable
        Vector2 movement = movementControl.action.ReadValue<Vector2>();

        //Vector of what direction we would move based on the inputs.
        //Y is in the Z area, as the above vector2 holds the Z in it's second value
        Vector3 move = new Vector3( movement.x, 0, movement.y );


        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree
        m_moveAmount = Mathf.Clamp01( Mathf.Abs( movement.x ) + Mathf.Abs( movement.y ) );

        //Move in direction of camera
        //move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
        //move.y = 0f;

        Debug.Log( movement );
        ////////////////////////////////////////////
        ///Move Player
        if ( canMove )
        {
            //Vector3 moveVector = transform.TransformDirection( move ) * playerSpeed;
            Vector3 moveVector = transform.forward * m_moveAmount * playerSpeed;
            //Rigidbody Velocity based on the movement set up in update
            m_rb.velocity = new Vector3( moveVector.x, m_rb.velocity.y, moveVector.z );
        }

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

        animator.SetFloat( "forwardSpeed", m_moveAmount );
    }

    private void Jump()
	{
        // Changes the height position of the player..
        if ( jumpControl.action.triggered && groundedPlayer )
        {
            //Jumped
            animator.SetTrigger( "jumped" );
            m_rb.AddForce( Vector3.up * jumpForce, ForceMode.Impulse );
            ////////////////////////////////////////////
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
    }
    public IEnumerator MoveWithAttack( AnimationEvent animationEvent )
    {
        Vector3 forwardDirection = transform.forward;


        movingWithAttack = true;
        moveWithAttackDistance = animationEvent.floatParameter;
        //Convert into to a float. the int is in hundreths of a second (0.14 = 14)
        moveWithAttackTime = ( float )animationEvent.intParameter / 100;
        targetForAttack = new Vector3( transform.position.x + moveWithAttackDistance * forwardDirection.x, transform.position.y, transform.position.y + moveWithAttackDistance * forwardDirection.z );


        
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        while ( elapsedTime < moveWithAttackTime )
        {

            Vector3 offset = targetForAttack - transform.position;
            if ( offset.magnitude > .1f )
            {
                //If we're further away than .1 unit, move towards the target.
                //The minimum allowable tolerance varies with the speed of the object and the framerate. 
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                float newSpeed = moveWithAttackDistance / moveWithAttackTime;
                //normalize it and account for movement speed.
                //controller.Move( offset * Time.deltaTime );

                Vector3 moveVector = transform.forward * newSpeed;
                //Rigidbody Velocity based on the movement set up in update
                //m_rb.velocity = new Vector3( moveVector.x, m_rb.velocity.y, moveVector.z );
                m_rb.MovePosition( transform.position * newSpeed * Time.deltaTime );

            }
            //transform.position = Vector3.Lerp( startingPos, targetForAttack, ( elapsedTime / moveWithAttackTime ) );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetForAttack;


    }
    private void EndMoveWithAttack()
    {
        m_rb.velocity = Vector3.zero;

        movingWithAttack = false;

    }
}

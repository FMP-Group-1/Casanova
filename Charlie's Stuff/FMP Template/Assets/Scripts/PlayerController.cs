using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
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

    public bool canMove;
    public bool canFall;

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

    void Update()
    {



        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = movementControl.action.ReadValue<Vector2>();
        //Movement.y is the Z value
        Vector3 move = new Vector3(movement.x, 0, movement.y);

        //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree

        m_moveAmount = Mathf.Clamp01(Mathf.Abs(movement.x) + Mathf.Abs(movement.y));

        //Move in direction of camera
        move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
        move.y = 0f;

        if (canMove)
        {
            controller.Move(move * Time.deltaTime * playerSpeed);
        }


        // Changes the height position of the player..
        if (jumpControl.action.triggered && groundedPlayer)
        {
            //Jumped
            animator.SetTrigger("jumped");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        if (!groundedPlayer)
        {

            animator.SetBool("inAir", true);
        }
        else
		{
            //Landed
            animator.SetBool("inAir", false);
		}

        playerVelocity.y += gravityValue * Time.deltaTime;


        if (canFall)
        {
            controller.Move(playerVelocity * Time.deltaTime);
        }

        //Rotate player when moving
        if (movement != Vector2.zero)
		{
            float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
		}

        Debug.Log(playerVelocity);

        animator.SetFloat("forwardSpeed", m_moveAmount);
    }
}

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
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraMainTransform;

    private Animator animator;

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

        //Move in direction of camera
        move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

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
        controller.Move(playerVelocity * Time.deltaTime);

        //Rotate player when moving
        if (movement != Vector2.zero)
		{
            float targetAngle = cameraMainTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
		}

        Debug.Log(movement);

        animator.SetFloat("forwardSpeed", movement.y);
        animator.SetFloat("sidewaysSpeed", movement.x);
    }
}

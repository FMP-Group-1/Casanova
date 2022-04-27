using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class melee : MonoBehaviour
{
    //[SerializeField]
    //private GameObject holsteredSword;
    //[SerializeField]
    //private GameObject realSword;

    [SerializeField]
    private swordAttack swordScript;

    public BoxCollider swordCollider;

    bool swordEquipped = true;
    bool currentlyInTheProcessOfSheathing = false;

    private Animator animator;

    private PlayerControls m_playerControls;

    private PlayerController m_playerController;

    public bool canStartNextAttack = true;

    public Text comboDebugText;

    private enum Attack
    {
        Nothing,
        Light,
        Heavy,
    }

    private Attack attackType;

    // Start is called before the first frame update
    void Start()
    {
        m_playerControls = new PlayerControls();

        m_playerController = GetComponent<PlayerController>();

        m_playerControls.Enable();
        animator = GetComponent<Animator>();
    }




    // Update is called once per frame
    void Update()
    {
        /*if ( m_playerControls.Combat.SheatheUnsheathe.triggered )
        {
            if ( !currentlyInTheProcessOfSheathing )
            {
                currentlyInTheProcessOfSheathing = true;

                if ( swordEquipped )
                {
                    animator.SetTrigger( "sheatheSword" );
                    // swordEquipped = false;
                }
                else
                {
                    animator.SetTrigger( "drawSword" );
                    // swordEquipped = true;
                }
            }
        }*/

        //Attack straight from unarmed. No Blend Animation
        //Light Attack
        if ( m_playerControls.Combat.LightAtatck.triggered )
        {
            //So Sword is NOT equipped
            if ( !swordEquipped )
            {
                swordEquipped = true;
            }

            attackType = Attack.Light;

        }
        //Heavy Attack
        if ( m_playerControls.Combat.HeavyAttack.triggered )
        {
            //So Sword is NOT equipped
            if ( !swordEquipped )
            {
                swordEquipped = true;
            }

            attackType = Attack.Heavy;
        }
        //Heavy Attack
        if ( m_playerControls.Combat.Whirlwind.triggered )
        {
            //So Sword is NOT equipped
            if ( !swordEquipped )
            {
                swordEquipped = true;
            }

            attackType = Attack.Heavy;
            animator.SetTrigger( "whirlwind" );
        }
        //Whirlwind has been released
        if ( m_playerControls.Combat.Whirlwind.ReadValue<float>() == 0 )
        {
            animator.SetBool( "whirlwindHeld", false );
        }
        else
        {
            animator.SetBool( "whirlwindHeld", true );
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

    }

    public void CollisionsStart()
    {
        comboDebugText.text += "\nColl. Start\n";
        swordCollider.enabled = true;
    }

    public void CollisionsEnd()
    {
        comboDebugText.text += "\nColl. End\n";
        swordCollider.enabled = false;
        canStartNextAttack = true;

    }

    /*
    public void endSheathing()
    {
        currentlyInTheProcessOfSheathing = false;
    }

    public void EquipSword()
    {
        swordEquipped = true;
        realSword.SetActive( true );
        holsteredSword.SetActive( false );
    }

    public void SheatheSword()
    {

        swordEquipped = false;
        realSword.SetActive( false );
        holsteredSword.SetActive( true );
    }
    */

    private void AttackBegin()
	{
        //Cap to how far can rotate;
        float maxRotate = 40;
        float rotationSpeed = 4f;
        Transform cameraMainTransform = Camera.main.transform;

        Vector2 playerInput = m_playerController.GetPlayerInput();

        float targetAngle = Mathf.Atan2( playerInput.x, playerInput.y ) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );
        // We have the TARGET angle, where we really are wanting to go, but we don't wanna go that far.

        Debug.Log( "Current Angle: " + transform.rotation.eulerAngles.y );
        Debug.Log( "Target Angle:  " + targetAngle );


        //Quaternion MoveDirection = Quaternion.Euler(m_playerController.GetMoveDirection());


        //Need to make it based on inputs.

        StartCoroutine(Rotate( targetRotation ) );
    }

    public void EndCombo()
    {
		if( !animator.IsInTransition(0) )
        {

            comboDebugText.text = "\nEnd\n";
            m_playerController.playerVelocity.y = 0f;

            m_playerController.canFall = true;
            m_playerController.canMove = true;
            m_playerController.canRotate = true;
            animator.SetBool( "comboActive", false );
            //MAKE SURE IT'S AVAILABLE AGAIN. CURRENTLY BROKE a bit This is duplicated
            //canStartNextAttack = true;

        }
		else
		{
            comboDebugText.text += "\nthis attack was started on a transitiony state";
        }
		

    }

    //Target angle is not actually where you will end up! It is the point you are looking, so it could be 180 degrees away, we are not going that far
    IEnumerator Rotate( Quaternion targetRotation )
    {
        float inTime = 0.2f; ;
        for( var t = 0f; t < 1; t += Time.deltaTime / inTime )
        {
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, t );
            yield return null;
        }
	}
}

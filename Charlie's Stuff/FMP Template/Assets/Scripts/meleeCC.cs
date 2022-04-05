using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class meleeCC : MonoBehaviour
{
    [SerializeField]
    private GameObject holsteredSword;
    [SerializeField]
    private GameObject realSword;

    [SerializeField]
    private swordAttack swordScript;

    public BoxCollider swordCollider;

    bool swordEquipped = true;
    bool currentlyInTheProcessOfSheathing = false;

    private Animator animator;

    private PlayerControls m_playerControls;

    private PlayerController m_playerController;

    public bool canStartNextAttack = true;


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
        if ( m_playerControls.Combat.SheatheUnsheathe.triggered )
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
        }

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
            // So we are not attacking YET, but we want to

            switch ( attackType )
            {
                case Attack.Light:

                    animator.SetTrigger( "light" );
                    break;

                case Attack.Heavy:

                    animator.SetTrigger( "heavy" );
                    break;

                case Attack.Nothing:
                    Debug.Log( "" );
                    break;

            }
            m_playerController.canMove = false;
            m_playerController.canFall = false;
            m_playerController.canRotate = false;

            //We are attacking
            canStartNextAttack = false;
            animator.SetTrigger( "attacked" );
            //Combo has begun
            animator.SetBool( "comboActive", true );

            //Next queued attack is nothing, until we add one
            attackType = Attack.Nothing;


        }

    }

    public void CollisionsStart()
    {
        swordCollider.enabled = true;
        //swordScript.setCollidersAcitve(collidersActive);
        //isAttackingText.text = "Attacking";
    }

    public void CollisionsEnd()
    {
        swordCollider.enabled = false;
        //swordScript.setCollidersAcitve(collidersActive);
        //isAttackingText.text = "Not Attacking";
        canStartNextAttack = true;

    }


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

    public void EndCombo()
    {
        //m_playerController.playerVelocity.y = 0f;

        m_playerController.canFall = true;
        m_playerController.canMove = true;
        m_playerController.canRotate = true;
        animator.SetBool( "comboActive", false );
        //MAKE SURE IT'S AVAILABLE AGAIN. CURRENTLY BROKE a bit This is duplicated
        canStartNextAttack = true;


    }

}

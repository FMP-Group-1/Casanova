using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class melee : MonoBehaviour
{
    [SerializeField]
    private Text isAttackingText;
    [SerializeField]
    private GameObject holsteredSword;
    [SerializeField]
    private GameObject realSword;

    [SerializeField]
    private swordAttack swordScript;

    public BoxCollider swordCollider;

    bool swordEquipped = true;
    bool currentlyInTheProcessOfSheathing = false;

    float baseComboTimer = 0.4f;
    float timerIterator = 0.0f;
    bool timerCounting = false;
    private Animator animator;

    private PlayerControls m_playerControls;

    private bool isCurrentlyAttacking = false;

    private bool collidersActive = false;

    private enum Attack
    {
        Nothing,
        Light,
        Heavy,
        Spell
    }

    private Attack attackType;

    // Start is called before the first frame update
    void Start()
    {
        m_playerControls = new PlayerControls();

        m_playerControls.Enable();
        animator = GetComponent<Animator>();
    }



    // Update is called once per frame
    void Update()
    {
        if (m_playerControls.Play.SheatheUnsheathe.triggered)
        {
            if (!currentlyInTheProcessOfSheathing)
            {
                currentlyInTheProcessOfSheathing = true;

                if (swordEquipped)
                {
                    animator.SetTrigger("sheatheSword");
                   // swordEquipped = false;
                }
                else
                {
                    animator.SetTrigger("drawSword");
                   // swordEquipped = true;
                }
            }
        }
        
        //Attack straight from unarmed
        //Light Attack
        if (m_playerControls.Play.LightAtatck.triggered)
        {
            if (!swordEquipped)
            {
                animator.SetTrigger("drawSword");
                //swordEquipped = true;
            }
            
            attackType = Attack.Light;
            
        }
        //Heavy Attack
        if (m_playerControls.Play.HeavyAttack.triggered)
        {
            if (!swordEquipped)
            {
                animator.SetTrigger("drawSword");
               // swordEquipped = true;
            }
            
            attackType = Attack.Heavy;
        }    
        
        /*
        //Just equip sword if click attack from unarmed

        //Light Attack
        if (m_playerControls.Play.LightAtatck.triggered)
        {
            if (!swordEquipped)
            {
                if (!currentlyInTheProcessOfSheathing)
                {
                    currentlyInTheProcessOfSheathing = true;
                    animator.SetTrigger("drawSword");
                    swordEquipped = true;
                }
            }
            else
            {
                attackType = Attack.Light;
            }
        }
        //Heavy Attack
        if (m_playerControls.Play.HeavyAttack.triggered)
        {
            if (!swordEquipped)
            {
                if (!currentlyInTheProcessOfSheathing)
                {
                    currentlyInTheProcessOfSheathing = true;
                    animator.SetTrigger("drawSword");
                    swordEquipped = true;
                }
            }
			else
            {
                attackType = Attack.Heavy;
            }
        }
        */

        //Could possibly replace timer with events on individual animations
        if (timerCounting)
        {
            timerIterator += Time.deltaTime;
        }

        if (timerIterator >= baseComboTimer)
        {
            timerIterator = baseComboTimer;
            timerCounting = false;
            animator.SetBool("comboActive", false);
        }

        //Basically, if you have reached the end of an attack, you are no longer attacking, but if "attackTyp" is not nothing, there's somehing queued up, so lets do it
        if (!isCurrentlyAttacking && attackType != Attack.Nothing)
        {
            // So we are not attacking YET, but we want to

            switch (attackType)
            {
                case Attack.Light:

                    animator.SetTrigger("light");
                    break;

                case Attack.Heavy:

                    animator.SetTrigger("heavy");
                    break;

                case Attack.Spell:

                    animator.SetTrigger("miscAttack");
                    animator.SetTrigger("spell");
                    break;
            }

            //We are attacking
            isCurrentlyAttacking = true;
            //Combo has begun
            animator.SetBool("comboActive", true);
            //Reset combo counter
            timerIterator = 0.0f;
            //We are counting (Have to set true everytime sadly, even if already true. Probably faster than checking if it isn't all the time.
            timerCounting = true;
            attackType = Attack.Nothing;


        }

    }

    void CollisionsStart()
	{
        swordCollider.enabled = true;
        collidersActive = true;
        //swordScript.setCollidersAcitve(collidersActive);
        isAttackingText.text = "Attacking";
    }

    void CollisionsEnd()
    {
        swordCollider.enabled = false;
        collidersActive = false;
        //swordScript.setCollidersAcitve(collidersActive);
        isAttackingText.text = "Not Attacking";
        isCurrentlyAttacking = false;
    }
    

    public void endSheathing()
	{
        currentlyInTheProcessOfSheathing = false;
	}

    public void EquipSword()
    {
        swordEquipped = true;
        realSword.SetActive(true);
        holsteredSword.SetActive(false);
    }

    public void SheatheSword()
	{

        swordEquipped = false;
        realSword.SetActive(false);
        holsteredSword.SetActive(true);
    }

}

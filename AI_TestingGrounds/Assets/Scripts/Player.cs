using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player script purely for testing AI Behaviour towards Player
// Will be replaced by actual Player script later in project
public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject m_camera;

    private Animator m_animController;
    private float m_moveSpeed = 5.0f;
    private float m_rotateSpeed = 300.0f;
    private Renderer m_playerRenderer;
    private Color m_defaultColor;
    private Color m_redTransparent;
    private bool m_playerIsMoving = false;
    private bool m_playerIsAttacking = false;

    [SerializeField]
    private GameObject m_weapon;
    private BoxCollider m_weaponCollider;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float m_damage = 15.0f;

    void Awake()
    {
        m_animController = GetComponent<Animator>();
        m_playerRenderer = transform.Find("Alpha_Surface").GetComponent<Renderer>();
        m_defaultColor = m_playerRenderer.material.color;
        m_redTransparent = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        m_weaponCollider = m_weapon.GetComponent<BoxCollider>();

        DisableCollision();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if ( !m_playerIsAttacking )
        {
            if (!m_playerIsMoving && IsPlayerMoving())
            {
                m_playerIsMoving = true;
                ResetAnimTriggers();
                StartRun();
            }

            if (m_playerIsMoving && !IsPlayerMoving())
            {
                m_playerIsMoving = false;
                ResetAnimTriggers();
                StandStill();
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * ( m_moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * (m_moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * (m_moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * (m_moveSpeed * Time.deltaTime);
            }

            if (Input.GetMouseButtonDown(0))
            {
                ResetAnimTriggers();
                StartAttack();
                m_playerIsAttacking = true;
                m_playerIsMoving = false;
            }
        }

        transform.Rotate(0.0f, (Input.GetAxis("Mouse X") * (m_rotateSpeed * Time.deltaTime)), 0.0f);

    }

    private bool IsPlayerMoving()
    {
        bool playerIsMoving = false;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            playerIsMoving = true;
        }

        return playerIsMoving;
    }

    public float GetDamageNum()
    {
        return m_damage;
    }

    public void EndAttack()
    {
        m_playerIsAttacking = false;
    }

    private void DisableCollision()
    {
        m_weaponCollider.enabled = false;
    }

    private void EnableCollision()
    {
        m_weaponCollider.enabled = true;
    }

    private void StartWalk()
    {
        m_animController.SetTrigger("Walk");
    }

    private void StartRun()
    {
        m_animController.SetTrigger("Run");
    }

    private void StandStill()
    {
        m_animController.SetTrigger("Idle");
    }

    private void StartAttack()
    {
        m_animController.SetTrigger("Attack");
    }

    private void StandUp()
    {
        m_animController.SetTrigger("StandUp");
    }

    private void ResetAnimTriggers()
    {
        m_animController.ResetTrigger("Walk");
        m_animController.ResetTrigger("Idle");
        m_animController.ResetTrigger("Attack");
        m_animController.ResetTrigger("Run");
        m_animController.ResetTrigger("StandUp");
    }

    public void SetHitVisual( bool isHit )
    {
        if ( isHit )
        {
            m_playerRenderer.material.SetColor("_Color", m_redTransparent);
        }
        else if ( !isHit )
        {
            m_playerRenderer.material.SetColor("_Color", m_defaultColor);
        }
    }
}

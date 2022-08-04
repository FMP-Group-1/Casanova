using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDamageManager : MonoBehaviour
{
    [SerializeField]
    private float m_health = 100.0f;
    private bool m_invulnerable = false;
    [SerializeField]
    private float m_invulnerableTime = 1f;

    //The Animator Component
    protected Animator m_animator;


    [SerializeField]
    protected Image m_healthBarFill;

    protected int an_getHitTrigger;
    protected int an_death;

    private bool m_alive = true;

    protected virtual void Start()
    {
        m_animator = GetComponent<Animator>();
        an_getHitTrigger = Animator.StringToHash( "TakeHit" );
        an_death = Animator.StringToHash( "Death" );

        UpdateHealthBar();

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void UpdateHealthBar()
	{

        m_healthBarFill.fillAmount = GetHealth() / 100;
    }

    public virtual void TakeDamage(Transform othersTransform, float damage = 30f )
    {
        if( !m_invulnerable )
        {
            m_health -= damage;
            

            Vector3 startPosition= transform.position;
            //... to....
            Vector3 targetPosition = othersTransform.position;

            //This is now the angle (between -180 and 180) between origin and attacker
            float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );



            //Create a Quaternion, with new angle, to be what we want the new rotation point to be
            Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );

            //Set rotation to face who attacked it
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 1f );


            m_invulnerable = true;

            if ( m_health <= 0.0f ) // Die
            {
                Die();
            }
            else // Just get hurt
			{

                m_animator.SetTrigger( an_getHitTrigger );
                StartCoroutine( ResetInvulnerable( m_invulnerableTime ) );
            }

            UpdateHealthBar();
        }



    }

    protected virtual void Die()
    {
        m_health = 0.0f;
        m_alive = false;
        m_animator.SetTrigger( an_death );

    }

    protected void SetAlive( bool alive )
	{
        m_alive = alive;
	}

    protected bool GetAlive()
	{
        return m_alive;
	}

    public float GetHealth()
	{
        return m_health;
	}

    public void SetHealth( float health )
	{
        m_health = health;
	}

    public virtual IEnumerator ResetInvulnerable(float timer)
	{
        yield return new WaitForSeconds( timer );
        m_invulnerable = false;
    }


    public void SetIFrames()
	{
        m_invulnerable = true;
        StartCoroutine(ResetInvulnerable( m_invulnerableTime ) );

	}

    public void SetInvulnerable( bool invulnerable )
    {
        m_invulnerable = invulnerable;
    }

    public bool GetInvulnerable()
    {
        return m_invulnerable;
    }

}

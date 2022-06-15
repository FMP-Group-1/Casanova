using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    private bool m_canBeHurt = true;
    //The Animator Component
    private Animator m_animator;
    private PlayerController m_playerController;


    private int an_stumble;

    void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponent<Animator>();
        an_stumble = Animator.StringToHash( "stumble" );
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void GetHurt(Transform othersTransform)
    {
        if( m_canBeHurt )
        {
            m_playerController.DeactivateAllTheCanStuff();
            //rotate to face the thing, then animate 

            Vector3 startPosition= transform.position;
            //... to....
            Vector3 targetPosition = othersTransform.position;

            //This is now the angle (between -180 and 180) between origin and target
            float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );



            //Create a Quaternion, with new angle, to be what we want the new rotation point to be
            Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );

            //Lerp instead of SET it as I believe that will stop issues like frame rate skipping past or soemthing
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.99f );

            m_animator.SetTrigger( an_stumble );
            m_canBeHurt = false;
            StartCoroutine( ResetInvulnerable() );
        }

	}

	public IEnumerator ResetInvulnerable()
	{
        yield return new WaitForSeconds( 1f );
        m_canBeHurt = true;
        m_playerController.ResetAllTheCanStuff();
    }


    public void SetInvulnerable(int isInvulnerable)
	{
        if ( isInvulnerable == 1 )
		{
            m_canBeHurt = false;
        }
		else
		{
            m_canBeHurt = true;
        }
	}

}

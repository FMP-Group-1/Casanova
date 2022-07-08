using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDamageManager : MonoBehaviour
{
    [SerializeField]
    private float m_health = 10;
    private bool m_invulnerable = false;
    [SerializeField]
    private float m_invulnerableTime = 1f;

    private EnemyAI m_enemyAI;
    private PlayerController m_playerController;


    private List<Material> m_materialList = new List<Material>();


    //The Animator Component
    private Animator m_animator;

    [SerializeField]
    private HealthDisplay healthDisplay;

    private enum CharacterType
    {
        Enemy,
        Player
	}

    [SerializeField]
    private CharacterType m_characterType;

    private int an_getHitTrigger;
    private int an_death;


    [SerializeField]
    private Image m_youDied;
    Color m_defaultColour;

    void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponent<Animator>();
        an_getHitTrigger = Animator.StringToHash( "TakeHit" );
        an_death = Animator.StringToHash( "Death" );


        if (m_characterType == CharacterType.Player )
		{
            m_playerController = GetComponent<PlayerController>();
            m_defaultColour = m_youDied.color;
        }
        else if ( m_characterType == CharacterType.Enemy )
		{
            m_enemyAI = GetComponent<EnemyAI>();

            int iteration = 0;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach ( Renderer child in renderers )
            {
                m_materialList.Add( renderers[ iteration ].material );
                m_materialList[ iteration ] = renderers[ iteration ].material;
                m_materialList[ iteration ].SetFloat( "_FadeStartTime", float.MaxValue );
                m_materialList[ iteration ].SetInt( "_ForceVisible", 0 );
                iteration++;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(Transform othersTransform, float damage = 30f )
    {



        if( !m_invulnerable )
        {
            m_health -= damage;
            //If Player
            if ( m_characterType == CharacterType.Player )
			{
                m_playerController.DeactivateAllTheCanStuff();
            }
            else if ( m_characterType == CharacterType.Enemy )
            {
                m_enemyAI.ResetLastUsedAnimTrigger();
                m_enemyAI.StopNavMesh();
                m_enemyAI.DisableCollision();
                m_enemyAI.SetLastUsedAnimTrigger( an_getHitTrigger );

            }
            //rotate to face the thing, then animate 


            Vector3 startPosition= transform.position;
            //... to....
            Vector3 targetPosition = othersTransform.position;

            //This is now the angle (between -180 and 180) between origin and attacker
            float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );



            //Create a Quaternion, with new angle, to be what we want the new rotation point to be
            Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );

            //Set rotation to face who attacked it
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 1f );

            m_animator.SetTrigger( an_getHitTrigger );
            m_invulnerable = true;
            StartCoroutine( ResetInvulnerable( m_invulnerableTime ) );
        }

        healthDisplay.UpdateHealth( m_health );
        if ( m_health <= 0.0f )
        {
            Die();
        }


    }

    private void Die()
    {
        m_health = 0.0f;

        if (m_characterType == CharacterType.Enemy )
		{
            m_enemyAI.SetAIState( AIState.Dead );
            m_enemyAI.UnregisterAttacker();
            StartCoroutine( DissolveEnemy( 2f ) );
        }
        if( m_characterType == CharacterType.Player )
        {
            m_playerController.DeactivateAllTheCanStuff();

            m_youDied.gameObject.SetActive( true );
            Color newColour = m_defaultColour;
            newColour.a = 0f;

            m_youDied.color = newColour;


            StartCoroutine( FadeTo( 1f, 3f ) );
        }

        m_animator.SetTrigger( an_death );

    }

    public float GetHealth()
	{
        return m_health;
	}

    private IEnumerator DissolveEnemy( float time )
	{
        yield return new WaitForSeconds( time );

        foreach ( Material mat in m_materialList )
        {
            mat.SetFloat( "_FadeStartTime", Time.time );
        }
    }

    public IEnumerator ResetInvulnerable(float timer)
	{
        yield return new WaitForSeconds( timer );
        m_invulnerable = false;
        if (m_characterType == CharacterType.Player )
        {
            m_playerController.ResetAllTheCanStuff();

        }
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

    IEnumerator FadeTo( float aValue, float aTime )
    {
        for( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / aTime )
        {
            Color newColor = new Color(1, 1, 1, alpha);
            m_youDied.color = newColor;
            yield return null;
        }
    }
}

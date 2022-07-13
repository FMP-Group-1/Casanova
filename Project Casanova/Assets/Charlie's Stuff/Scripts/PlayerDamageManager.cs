using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageManager : CharacterDamageManager
{
    [SerializeField]
    private Image m_youDied;
    Color m_defaultColour;


    private PlayerController m_playerController;

    protected override void Start()
    {
        base.Start();
        m_playerController = GetComponent<PlayerController>();
        m_defaultColour = m_youDied.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void TakeDamage(Transform othersTransform, float damage = 30f )
    {
        if( !GetInvulnerable() )
        {
            m_playerController.DeactivateAllTheCanStuff();

        }
        base.TakeDamage( othersTransform, damage );
    }

    protected override void Die()
    {
        base.Die();
        m_playerController.DeactivateAllTheCanStuff();

        m_youDied.gameObject.SetActive( true );
        Color newColour = m_defaultColour;
        newColour.a = 0f;

        m_youDied.color = newColour;


        StartCoroutine( YouDiedFade( 1f, 3f ) );
        
        m_animator.SetTrigger( an_death );

    }

    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.ResetAllTheCanStuff();

    }



    IEnumerator YouDiedFade( float aValue, float aTime )
    {
        for( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / aTime )
        {
            Color newColor = new Color(1, 1, 1, alpha);
            m_youDied.color = newColor;
            yield return null;
        }
    }
}

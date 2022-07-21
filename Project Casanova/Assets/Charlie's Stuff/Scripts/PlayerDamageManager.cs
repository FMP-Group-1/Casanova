using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageManager : CharacterDamageManager
{
    [SerializeField]
    private Image m_youDied;
    Color m_defaultColour;

    public Image m_healthBarFill;

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
        m_healthBarFill.fillAmount = GetHealth() / 100;
    }


    public override void TakeDamage(Transform othersTransform, float damage = 30f )
    {
        if ( GetAlive() )
        {
            if ( !GetInvulnerable() )
            {
                m_playerController.LoseControl();

            }
            base.TakeDamage( othersTransform, damage );
        }
    }

    protected override void Die()
    {
        base.Die();
        DisplayDeathMessage();
        m_playerController.LoseControl();

    }

    private void DisplayDeathMessage()
	{

        m_youDied.gameObject.SetActive( true );
        Color newColour = m_defaultColour;
        newColour.a = 0f;

        m_youDied.color = newColour;


        StartCoroutine( YouDiedFade( 3f ) );
    }

    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.RegainControl();

    }



    IEnumerator YouDiedFade( float aTime )
    {
        for( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / aTime )
        {
            Color newColor = new Color(1, 1, 1, alpha);
            m_youDied.color = newColor;
            yield return null;
        }
    }
}

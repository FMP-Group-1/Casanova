using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Input actions
    [SerializeField]
    private Image m_blackScreen;
    [SerializeField]
    private Image m_youDied;


    [SerializeField]
    CanvasGroup m_menuUIGroup;
    [SerializeField]
    CanvasGroup m_gameUIGroup;

    float m_uiFadeInTime = 1.5f;
    float m_blackScreenFade = 2.5f;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void Respawn( float howLongFade )
	{
        //Fade Black Screen in
        StartCoroutine( FadeIn( m_blackScreen, howLongFade ) );

        //Turn off You Died
        StartCoroutine( FadeOut( m_youDied, 0.1f, howLongFade + 0.5f ) );

        //Fade back in with 0.5 wait
        StartCoroutine( FadeOut( m_blackScreen, howLongFade, howLongFade + 0.5f ) );
        StartCoroutine( FadeInGroup( m_gameUIGroup, m_uiFadeInTime, howLongFade * 2 + 0.5f ) );

    }

    public void DisplayDeathUI()
	{
        StartCoroutine( FadeOutGroup( m_gameUIGroup, 2.0f ) );
        StartCoroutine( FadeIn( m_youDied, 3.0f ) );
	}

    public void BeginScene()
    {
        m_gameUIGroup.gameObject.SetActive( false );

        m_menuUIGroup.gameObject.SetActive( false );

        m_blackScreen.gameObject.SetActive( true );


        float blackscreenFadeTime = 2.0f;

        StartCoroutine( FadeOut( m_blackScreen, blackscreenFadeTime ) );

        //Make Menu UI fade in delay same as Black fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_menuUIGroup, 1.5f, blackscreenFadeTime ) );
        

    }

    public void StartGame()
	{
        float menuFadeOutTime = 2.0f; 

		StartCoroutine( FadeOutGroup( m_menuUIGroup, menuFadeOutTime ) );
        //Make Game UI fade in delay same as menu fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_gameUIGroup, 1.5f, menuFadeOutTime ) );
	}






    private Color ChangeImageAlpha( Image image, float newAlpha )
	{
        return new Color( image.color.r, image.color.g, image.color.b, newAlpha);
	}






    public IEnumerator FadeIn( Image image, float time, float delay = 0.0f )
    {
        yield return new WaitForSeconds( delay );

        image.gameObject.SetActive( true );
        image.color = ChangeImageAlpha( image, 0.0f );

        for ( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            image.color = ChangeImageAlpha( image, alpha );
            yield return null;
        }
        // Fully Set it
        image.color = ChangeImageAlpha( image, 1.0f );
    }


    public IEnumerator FadeOut( Image image, float time, float delay = 0.0f )
    {
        yield return new WaitForSeconds( delay );

        image.color = ChangeImageAlpha( image, 1.0f );

        for ( float alpha = 1.0f; alpha > 0.0f; alpha -= Time.deltaTime / time )
        {
            image.color = ChangeImageAlpha( image, alpha );
            yield return null;
        }
        // Fully Set it
        image.color = ChangeImageAlpha( image, 0.0f );

        image.gameObject.SetActive( false );
    }

    public IEnumerator FadeInGroup( CanvasGroup group, float time, float delay = 0.0f )
    {
        yield return new WaitForSeconds( delay );
        
        group.alpha = 0.0f;

        group.gameObject.SetActive( true );

        for ( float alpha = group.alpha; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            group.alpha = alpha;
            yield return null;
        }
        group.alpha = 1.0f;
    }

    public IEnumerator FadeOutGroup( CanvasGroup group, float time, float delay = 0.0f )
    {
        yield return new WaitForSeconds( delay );
        
        group.alpha = 1.0f;

        for ( float alpha = group.alpha; alpha > 0.0f; alpha -= Time.deltaTime / time )
        {
            group.alpha = alpha;
            yield return null;
        }
        group.alpha = 0.0f;

        group.gameObject.SetActive( false );
    }

}

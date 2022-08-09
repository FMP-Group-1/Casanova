using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private enum Menu
	{
        Main,
        Options,
        Controls,
        Credits,
        Game
	}

    private Menu m_currentMenu = Menu.Main;
    //Input actions
    [SerializeField]
    private Image m_blackScreen;
    [SerializeField]
    private Image m_youDied;


    [SerializeField]
    CanvasGroup m_mainMenu;
    [SerializeField]
    CanvasGroup m_optionsUIGroup;
    [SerializeField]
    CanvasGroup m_controlsUIGroup;
    [SerializeField]
    CanvasGroup m_creditsUIGroup;
    [SerializeField]
    CanvasGroup m_gameUIGroup;

    [SerializeField]
    CanvasGroup m_backButton;



    [SerializeField]
    Image m_background;

    float m_uiFadeInTime = 1.5f;
    float m_blackScreenFade = 2.5f;

    private float m_menuSwapSpeed = 0.75f;

	private void Start()
	{


    }

	public void QuitGame()
	{
        Application.Quit();
	}

    public void SwapMenus( string menuToGoTo )
    {
        CanvasGroup groupToFadeOut;
        switch ( m_currentMenu )
        {
            default: 
                groupToFadeOut = m_mainMenu; 
                break;
            case Menu.Main:
                groupToFadeOut = m_mainMenu;
                break;
            case Menu.Options:
                groupToFadeOut = m_optionsUIGroup;
                GetComponent<OptionsManager>().RefreshSliders();
                break;
            case Menu.Controls:
                groupToFadeOut = m_controlsUIGroup;
                break;
            case Menu.Credits:
                groupToFadeOut = m_creditsUIGroup;
                break;
            case Menu.Game:
                groupToFadeOut = m_gameUIGroup;
                break;
        }
        StartCoroutine( FadeOutGroup( groupToFadeOut, m_menuSwapSpeed ) );

        CanvasGroup groupToFadeIn;
        switch ( menuToGoTo )
        {
            default:
                groupToFadeIn = m_mainMenu;
                StartCoroutine( FadeOutGroup(m_backButton, m_menuSwapSpeed ) );
                m_currentMenu = Menu.Main;
                break;
            case "Options":
                groupToFadeIn = m_optionsUIGroup;
                m_currentMenu = Menu.Options;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
            case "Controls":
                groupToFadeIn = m_controlsUIGroup;
                m_currentMenu = Menu.Controls;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
            case "Credits":
                groupToFadeIn = m_creditsUIGroup;
                m_currentMenu = Menu.Credits;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
        }

        StartCoroutine( FadeInGroup( groupToFadeIn, m_menuSwapSpeed, m_menuSwapSpeed ) );
    }

    public void PauseMenu(bool paused)
    {
        if ( paused )
		{

            m_optionsUIGroup.gameObject.SetActive( true );
            m_gameUIGroup.gameObject.SetActive( false );
            m_optionsUIGroup.gameObject.GetComponentInChildren<Button>().interactable = true;
            //GetComponent<OptionsManager>().RefreshSliders();
        }
		else
		{

            m_optionsUIGroup.gameObject.SetActive( false );
            m_gameUIGroup.gameObject.SetActive( true );
        }

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
        m_optionsUIGroup.gameObject.SetActive ( false );
        m_controlsUIGroup.gameObject.SetActive ( false );
        m_creditsUIGroup.gameObject.SetActive ( false );
        m_mainMenu.gameObject.SetActive( false );

        m_blackScreen.gameObject.SetActive( true );

        StartCoroutine( FadeOut( m_blackScreen, m_blackScreenFade ) );

        //Make Menu UI fade in delay same as Black fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_mainMenu, 1.5f, m_blackScreenFade ) );
        StartCoroutine( FadeIn( m_background, 1.5f, m_blackScreenFade ) );


    }

    public void StartGame()
	{
        float menuFadeOutTime = 2.0f;

        StartCoroutine( FadeOutGroup( m_mainMenu, menuFadeOutTime ) );
        StartCoroutine( FadeOut( m_background, menuFadeOutTime ) );
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
        Button[] activeButtons = group.gameObject.GetComponentsInChildren<Button>();

        foreach ( Button button in activeButtons )
        {
            button.interactable = true;
        }

        group.alpha = 1.0f;
    }

    public IEnumerator FadeOutGroup( CanvasGroup group, float time, float delay = 0.0f )
    {
        yield return new WaitForSeconds( delay );
        
        Button[] activeButtons = group.gameObject.GetComponentsInChildren<Button>();
        
        foreach( Button button in activeButtons )
		{
            button.interactable = false;
		}

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

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
    CanvasGroup m_deadUIGroup;
    [SerializeField]
    CanvasGroup m_winGroup;

    [SerializeField]
    CanvasGroup m_pauseScreen;
    [SerializeField]
    CanvasGroup m_pauseBackButton;

    [SerializeField]
    CanvasGroup m_backButton;

    private string m_pauseScreenToHide;


    [SerializeField]
    Image m_background;

    float m_uiFadeInTime = 1f;

    private float m_menuSwapSpeed = 0.25f;

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
            //Pause
            GetComponent<OptionsManager>().RefreshSliders();
            m_pauseScreen.gameObject.SetActive( true );
            m_gameUIGroup.gameObject.SetActive( false );
        }
		else
		{
            //Unpause
            m_pauseScreen.gameObject.SetActive( false );
            m_gameUIGroup.gameObject.SetActive( true );
        }

    }

    public void DisplayPauseScreen( string screen )
	{
        switch ( screen )
		{
			case "Options":
                m_optionsUIGroup.gameObject.SetActive( true );

                break;
            case "Controls":
                m_controlsUIGroup.gameObject.SetActive( true );
                break;
		}
        m_pauseBackButton.gameObject.SetActive( true );
        m_pauseScreen.gameObject.SetActive( false );
        m_pauseScreenToHide = screen;

    }

    public void ReturnToPause()
	{
        switch ( m_pauseScreenToHide )
        {
            case "Options":
                m_optionsUIGroup.gameObject.SetActive( false );

                break;
            case "Controls":
                m_controlsUIGroup.gameObject.SetActive( false );
                break;
        }

        m_pauseBackButton.gameObject.SetActive( false );
        m_pauseScreen.gameObject.SetActive( true );
    }

    public void Respawn( float howLongFade )
	{
        m_pauseScreen.gameObject.SetActive ( false );
        //Fade Black Screen in
        StartCoroutine( FadeIn( m_blackScreen, howLongFade ) );

        //Turn off You Died
        StartCoroutine( FadeOutGroup( m_deadUIGroup, 0.1f, howLongFade + 0.5f ) );

        //Fade back in with 0.5 wait
        StartCoroutine( FadeOut( m_blackScreen, howLongFade, howLongFade + 0.5f ) );
        StartCoroutine( FadeInGroup( m_gameUIGroup, m_uiFadeInTime, howLongFade * 2 + 0.5f ) );

    }

    public void ReturnToMenu(float fadeTime)
	{

        m_pauseScreen.alpha = 0.0f;
        StartCoroutine( FadeIn( m_blackScreen, fadeTime ) );

    }


    public void DisplayDeathUI()
	{
        StartCoroutine( FadeOutGroup( m_gameUIGroup, m_uiFadeInTime ) );
        StartCoroutine( FadeInGroup( m_deadUIGroup, m_uiFadeInTime + 0.5f ) );
	}

    public void BeginScene()
    {
        m_gameUIGroup.gameObject.SetActive( false );
        m_optionsUIGroup.gameObject.SetActive ( false );
        m_controlsUIGroup.gameObject.SetActive ( false );
        m_creditsUIGroup.gameObject.SetActive ( false );
        m_mainMenu.gameObject.SetActive( false );
        m_pauseScreen.gameObject.SetActive( false );
        m_pauseBackButton.gameObject.SetActive( false );
        m_deadUIGroup.gameObject.SetActive( false );

        m_blackScreen.gameObject.SetActive( true );

        StartCoroutine( FadeOut( m_blackScreen, m_uiFadeInTime ) );

        //Make Menu UI fade in delay same as Black fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_mainMenu, 1.5f, m_uiFadeInTime / 2 ) );
        StartCoroutine( FadeIn( m_background, 1.5f, m_uiFadeInTime / 2 ) );


    }


    public void CompleteGame()
	{

        StartCoroutine( FadeOutGroup( m_gameUIGroup, m_uiFadeInTime ) );
        StartCoroutine( FadeInGroup( m_winGroup, m_uiFadeInTime ) );
        

    }

    public void StartGame()
	{
        float menuFadeOutTime = 2.0f;

        StartCoroutine( FadeOutGroup( m_mainMenu, menuFadeOutTime ) );
        StartCoroutine( FadeOut( m_background, menuFadeOutTime ) );
        //Make Game UI fade in delay same as menu fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_gameUIGroup, 1.5f, menuFadeOutTime / 2 ) );

        //If these screens are used in the MAIN menu, their alpha is set to 0 and deactivated.
        //To stop needing to set them to 1 eevrytime I pause, if we just do it here, they stay deactivated, but alpha is 1
        m_optionsUIGroup.alpha = 1;
        m_controlsUIGroup.alpha = 1;
        //Make Confirm Button for Options interactable
        m_optionsUIGroup.gameObject.GetComponentInChildren<Button>().interactable = true ;

        Settings.g_canPause = true;

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

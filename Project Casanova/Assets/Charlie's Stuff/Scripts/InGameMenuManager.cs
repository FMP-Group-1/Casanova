using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuManager : MonoBehaviour
{
    GameObject playerGameObject;
    private Animator m_cinemachineAnimator;

    [SerializeField]
    CanvasGroup m_menuUIGroup;
    [SerializeField]
    CanvasGroup m_gameUIGroup;


    private List<CanvasRenderer> gamePlayCanvasRenderers;

    // Start is called before the first frame update
    void Start()
    {
        m_menuUIGroup.alpha = 0;
        StartCoroutine( FadeIn( m_menuUIGroup ) );
        m_gameUIGroup.alpha = 0;

        playerGameObject = GameObject.FindGameObjectWithTag( "Player" );
        m_cinemachineAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        StartCoroutine( FadeOut( m_menuUIGroup ) );
        StartCoroutine( FadeIn( m_gameUIGroup ) );
        
        m_cinemachineAnimator.Play( "Game State" );
        playerGameObject.GetComponent<Animator>().SetTrigger("WakeUp");
        playerGameObject.GetComponent<PlayerController>().enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;

    }



    private IEnumerator FadeInMenu( float time = 1.0f )
	{
        yield return new WaitForSeconds( 1f );
        for ( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            m_menuUIGroup.alpha = alpha;
            yield return null;
        }
    }



    private IEnumerator FadeIn( CanvasGroup uiGroup, float time = 1.0f )
    {
        yield return new WaitForSeconds( 1f );
        for ( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            uiGroup.alpha = alpha;
            yield return null;
        }
    }
    private IEnumerator FadeOut( CanvasGroup uiGroup, float time = 1.0f )
    {
        for ( float alpha = 1.0f; alpha > 0.0f; alpha -= Time.deltaTime / time )
        {
            uiGroup.alpha = alpha;
            yield return null;
        }
    }
    /*
    private IEnumerator FadeUI( float aValue, float aTime )
    {
        for ( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / aTime )
        {
            Color newColor = new Color( 1, 1, 1, alpha );
           // m_youDied.color = newColor;
            yield return null;
        }
    }*/
}

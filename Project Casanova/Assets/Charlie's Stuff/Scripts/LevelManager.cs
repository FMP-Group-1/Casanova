using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //Input actions
    [SerializeField]
    private InputActionReference m_resetLevel;
    [SerializeField]
    private InputActionReference m_quitGame;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Hide Mouse cursor (Though likes to not work sometimes)
    **************************************************************************************/
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnEnable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enable input actions
    **************************************************************************************/
    private void OnEnable()
	{
        m_resetLevel.action.Enable();
        m_quitGame.action.Enable();

    }

	/**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Reset or Quit Game
    **************************************************************************************/
	private void Update()
	{
        //Reset Level
		if ( m_resetLevel.action.triggered )
		{
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene( scene.name );
        }
        if( m_quitGame.action.triggered )
		{
            Application.Quit();
		}
	}
}

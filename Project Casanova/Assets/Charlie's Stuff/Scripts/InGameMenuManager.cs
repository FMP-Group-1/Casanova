using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuManager : MonoBehaviour
{
    GameObject playerGameObject;
    private Animator m_cinemachineAnimator;
    private UIManager m_uiManager;


    // Start is called before the first frame update
    void Start()
    {
        m_uiManager = GameObject.FindGameObjectWithTag( Settings.g_ControllerTag ).GetComponent<UIManager>();


        playerGameObject = GameObject.FindGameObjectWithTag( "Player" );
        m_cinemachineAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        //Manage UI Elements
        m_uiManager.StartGame();

        m_cinemachineAnimator.Play( "Game State" );
        playerGameObject.GetComponent<Animator>().SetTrigger("WakeUp");
        playerGameObject.GetComponent<PlayerController>().enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;

    }

}

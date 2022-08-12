using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Room
{
    Cell,
    Hall,
    Armory,
    GuardRoom,
    Arena
}

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Animator m_cinemachineAnimator;

    [Header( "Player" )]
    [SerializeField]
    PlayerController m_playerController;
    [SerializeField]
    PlayerDamageManager m_playerHealthManager;

    private Room m_currentRoom;
    private UIManager m_uiManager;
    private RespawnManager m_respawnManager;
    private GateManager m_gateManager;

    private AIManager m_aiManager;

    private bool m_roomComplete;

    [Header("Cell Exit Trigger")]
    [SerializeField] 
    private GameObject m_cellExitTrigger;

    [Header( "Pause Input" )]
    [SerializeField, Tooltip( "Pause Input" )]
    private InputActionReference m_pauseInput;

    AsyncOperation m_sceneLoad;

    private void OnEnable()
	{
        m_pauseInput.action.Enable();

    }

	private void OnDisable()
	{

        m_pauseInput.action.Disable();
    }
	void Start()
    {
        //Very Begining of Game
        Settings.g_canPause = false;
        Settings.g_paused = false;
        m_currentRoom = Room.Cell;
        m_uiManager = GetComponent<UIManager>();
        m_respawnManager = GetComponent<RespawnManager>();
        m_gateManager = GetComponent<GateManager>();
        m_aiManager = GetComponent<AIManager>();

        m_respawnManager.SetRespawnPoint( Room.Cell );
        m_uiManager.BeginScene();

        EventManager.StartSpawnEnemiesEvent( 0 );
        EventManager.StartSpawnEnemiesEvent( 1 );
    }

	private void Update()
	{

        //If Pause Clicked
        if ( m_pauseInput.action.triggered )
        {
            if ( Settings.g_canPause )
            {

                //If we are ALREADY paused, unpause
                if ( Settings.g_paused )
                {
                    Time.timeScale = 1;
                    Settings.g_paused = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else //Pause
                {
                    Time.timeScale = 0;
                    Settings.g_paused = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                m_uiManager.PauseMenu( Settings.g_paused );

            }
        }

        if ( !m_roomComplete )
		{

            switch( m_currentRoom )
		    {
                case Room.Cell:

                    break;

                case Room.Hall:

                    //If you're in the hall, check if this group is a1l dead
                    if( m_aiManager.RemainingEnemiesInGroup( 0 ) <= 0 )
                    {
                        CompleteRoom( Room.Hall );
                    }
                    break;
                case Room.Armory:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 1 ) <= 0 )
                    {
                        CompleteRoom( Room.Armory );
                    }
                    break;
                case Room.GuardRoom:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 2 ) <= 0 )
                    {
                        CompleteRoom( Room.GuardRoom );
                    }
                    break;
                case Room.Arena:

                    //If you're in the hall, check if this group is a1l dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 3 ) <= 0 )
                    {
                        CompleteRoom( Room.Arena );
                    }
                    break;
            }
        }

    }


    public void PlayGame()
    {
        //Manage UI Elements
        m_uiManager.StartGame();
        m_playerController.SetMenuLock( false );

        m_cinemachineAnimator.Play( "Game State" );
        m_playerController.gameObject.GetComponent<Animator>().SetTrigger( "WakeUp" );
        m_playerController.GetComponent<MeleeController>().enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;

    }


    public void ReturnToMenu()
    {
        m_playerController.LoseControl();
        m_playerHealthManager.SetInvulnerable( true );
        Time.timeScale = 1f;
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
	{
        float respawnFadeTime = 2.0f;
        m_uiManager.ReturnToMenu( respawnFadeTime );
        yield return new WaitForSeconds( respawnFadeTime + 0.1f );
        Scene scene = SceneManager.GetActiveScene();
        StartCoroutine( ReloadSceneAsync( scene.name ) );
    }
    private IEnumerator ReloadSceneAsync( string scene )
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        m_sceneLoad = SceneManager.LoadSceneAsync( scene );
        while ( !m_sceneLoad.isDone )
        {
            yield return null;
        }
    }

    public void Die()
	{
        Settings.g_canPause = false;
        m_uiManager.DisplayDeathUI();
        BeginRespawn();

    }

    public void RespawnFromMenu()
	{
        Time.timeScale = 1;
        m_playerController.SetMenuLock( true );
        m_playerController.LoseControl();
        m_playerHealthManager.SetInvulnerable( true );
        BeginRespawn();
    }

    private void BeginRespawn()
    {
        //Both these need same value. Value will fade out for that time, and delay the ACTUAL stuff until then
        float fadeTimeAndDelay = 4.0f;
        m_uiManager.Respawn( fadeTimeAndDelay );
        //UI Manager puts an additional 0.5seconds on the clcok so it's fully black when we move eevrything in respawn Manager
        StartCoroutine( m_respawnManager.Respawn( fadeTimeAndDelay ) );
    }

    public void ActuallyRespawn()
    {
        m_aiManager.DeactivateActiveEnemies();

        Settings.g_paused = false;
        m_playerController.SetMenuLock( false );
        Room respawnPoint = m_respawnManager.GetRespawnPoint();
        switch ( respawnPoint )
        {
            case Room.Cell:
                EventManager.StartSpawnEnemiesEvent( 0 );
                EventManager.StartSpawnEnemiesEvent( 1 );
                m_cellExitTrigger.SetActive( true );
                break;
            case Room.Hall:
                EventManager.StartSpawnEnemiesEvent( 1 );
                break;
            case Room.Armory:

                EventManager.StartSpawnEnemiesEvent( 2 );
                break;
            case Room.GuardRoom:

                EventManager.StartSpawnEnemiesEvent( 3 );
                break;
            case Room.Arena:

                EventManager.StartSpawnEnemiesEvent( 3 );
                break;
        }

        m_gateManager.ResetGate( respawnPoint );

        Settings.g_canPause = true;
    }

    public void EnterRoom( Room room )
	{
        m_roomComplete = false;
        m_currentRoom = room;
        Debug.Log( m_currentRoom );
	}

    public void CompleteRoom( Room room )
    {
        m_roomComplete = true;
        switch( room )
        {
            case Room.Cell:
                break;
            case Room.Hall:
                m_gateManager.OpenCellHallExitGate();
                m_respawnManager.SetRespawnPoint( Room.Hall );
                break;
            case Room.Armory:
                m_gateManager.OpenArmoryExitGate();
                m_respawnManager.SetRespawnPoint( Room.Armory );
                break;
            case Room.GuardRoom:
                m_gateManager.OpenGuardRoomExitGate();
                m_respawnManager.SetRespawnPoint( Room.GuardRoom );
                break;
            case Room.Arena:
                //Entering Arena sets Arena respawn point
                //no respawn, you can't die now
                m_gateManager.OpenArenaExitGate();
                break;
        }
	}

}

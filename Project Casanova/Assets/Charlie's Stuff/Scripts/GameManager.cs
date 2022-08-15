using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


// Names of rooms of the game. Used for Respawn Points and triggering Enemies
//THE ORDER OF THESE CORRELATE WITH THE ARRAY FOR THE RESPAWN POINT TRANSFORMS IN RespawnManager.cs
// !!!!! DO NOT CHANGE WITHOUT TAKING THIS INTO ACCOUNT !!!!!
public enum Room
{
    Cell,
    Hall,
    Armory,
    GuardRoom,
    Arena
}

/**************************************************************************************
* Type: Class
* 
* Name: Game Manager
*
* Author: Charlie Taylor
*
* Description: Main Game manager that does A LOT of the heavy lifting when it comes to 
*              global events such as respawning and enemy spawning and UI
**************************************************************************************/
public class GameManager : MonoBehaviour
{
    [SerializeField, Tooltip("Camera Manager Animator, to manage the Menu > Gameplay camera")]
    private Animator m_cinemachineAnimator;

    [Header( "Player" )]
    [SerializeField, Tooltip("The Player's Controller script")]
    PlayerController m_playerController;
    [SerializeField, Tooltip( "The Player's Damage Manager script" )]
    PlayerDamageManager m_playerHealthManager;

    //The room the player is currently in (Based on room completion, not physical location)
    private Room m_currentRoom;

    //Managers on the same Game Object, populated in start
    private UIManager m_uiManager;
    private RespawnManager m_respawnManager;
    private GateManager m_gateManager; 
    private AIManager m_aiManager;

    //A bool that is updated based on if the room you are in is complete.
    private bool m_roomComplete;

    [Header("Cell Exit Trigger")]
    [SerializeField, Tooltip( "The trigger box outside of the Player's starting Cell" )]
    private GameObject m_cellExitTrigger;

    [Header( "Pause Input" )]
    [SerializeField, Tooltip( "Pause Input control" )]
    private InputActionReference m_pauseInput;

    //Scene load, for reseting game
    AsyncOperation m_sceneLoad;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnEnable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enable input for pause (No OnDisable as this object is never disabled)
    **************************************************************************************/
    private void OnEnable()
	{
        m_pauseInput.action.Enable();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Populate member variables and some global variables
    **************************************************************************************/
    private void Start()
    {
        //Very Begining of Game
        Settings.g_canPause = false;
        Settings.g_paused = false;
        //Components attached to same GO
        m_uiManager = GetComponent<UIManager>();
        m_respawnManager = GetComponent<RespawnManager>();
        m_gateManager = GetComponent<GateManager>();
        m_aiManager = GetComponent<AIManager>();

        //Always begin in cell
        m_currentRoom = Room.Cell;
        m_respawnManager.SetRespawnPoint( m_currentRoom );

        //Call UI Managers begin scene script to begin showing menu
        m_uiManager.BeginScene();

        //Prep first group of enemies
        EventManager.StartSpawnEnemiesEvent( 0 );
        //Spawn group in armory (Patrolling)
        EventManager.StartSpawnEnemiesEvent( 1 );
        //Spawn Guards in sleeping state
        EventManager.StartSpawnEnemiesEvent( 2 );

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
    * Description: Check Pause and Room Completion
    **************************************************************************************/
    private void Update()
	{
        //Check if Paused clicked, and manage it
        PauseCheck();
        //Room Completion check
        RoomCompleteCheck();

    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: PauseCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if Pause was pressed, and if so, pause or unpause
    **************************************************************************************/
    private void PauseCheck()
	{
        //Pressed
        if ( m_pauseInput.action.triggered )
        {
            //If we CAN pause, so, not at end of game or menu or something
            if ( Settings.g_canPause )
            {

                //If we are ALREADY paused, unpause
                if ( Settings.g_paused )
                {
                    //Time scale isn't the BEST but it works
                    Time.timeScale = 1;
                    Settings.g_paused = false;
                    //Hide cursor and lock it
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else //Pause
                {
                    Time.timeScale = 0;
                    Settings.g_paused = true;
                    //Show cursor on pause screen
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                //Hide or display pause UI based on if we jsut paused or not
                m_uiManager.PauseMenu( Settings.g_paused );
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoomCompleteCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if what rooms are completed, and open gates and stuff based on that
    *              All of this is GROSS but it was the best I could do for the time, skill
    *              and it works for the scale of the game
    **************************************************************************************/
    private void RoomCompleteCheck()
	{
        //If room is not complete...
        if ( !m_roomComplete )
        {
            //Check CURRENT room...
            switch ( m_currentRoom )
            {
                //And based on that room...
                case Room.Cell:

                    break;

                case Room.Hall:
                    //Check if this specific group is all dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 0 ) <= 0 )
                    {
                        //If so, complete the room
                        CompleteRoom( Room.Hall );
                    }
                    break;
                case Room.Armory:
                    if ( m_aiManager.RemainingEnemiesInGroup( 2 ) <= 0 )
                    {
                        CompleteRoom( Room.Armory );
                    }
                    break;
                case Room.GuardRoom:
                    if ( m_aiManager.RemainingEnemiesInGroup( 3 ) <= 0 )
                    {
                        CompleteRoom( Room.GuardRoom );
                    }
                    break;
                case Room.Arena:
                    /* Arena will be different if we get waves. For now this shows simple 1 wave, then open exit door,
                     * but we could do 3 tiers or something of this. For now, stick with 1 group
                     */
                    if ( m_aiManager.RemainingEnemiesInGroup( 4 ) <= 0 )
                    {
                        CompleteRoom( Room.Arena );
                        //EventManager.StartSpawnEnemiesEvent( 5 );
                    }
                    /*
                    //DISGUSTING Wave Logic just to show how it COULD exist, just.. gross
                    if ( m_aiManager.RemainingEnemiesInGroup( 5 ) <= 0 )
                    {
                        EventManager.StartSpawnEnemiesEvent( 6 );
                    }
                    if ( m_aiManager.RemainingEnemiesInGroup( 6 ) <= 0 )
                    {
                        EventManager.StartSpawnEnemiesEvent( 7 );
                    }
                    if ( m_aiManager.RemainingEnemiesInGroup( 7 ) <= 0 )
                    {
                        //end?
                    }*/
                    break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Play Game button in Main Menu, this does all the relevant stuff
    *              to begin the game
    **************************************************************************************/
    public void PlayGame()
    {
        //Manage UI Elements
        m_uiManager.StartGame();
        //Unlock player menu lock  
        m_playerController.SetMenuLock( false );
        //Name of Cinemachine animator's game state. Not a variable as this is the ONLY place it is used, which makes it equal to a variable in time written
        m_cinemachineAnimator.Play( "Game State" );
        //Wake up player via animation. Not a variable as this is the ONLY place it is used, which makes it equal to a variable in time written
        m_playerController.gameObject.GetComponent<Animator>().SetTrigger( "WakeUp" );
        //Melee controller is disabled by default as clicking play will queue up attacks and it's just easier this way
        m_playerController.GetComponent<MeleeController>().enabled = true;

        //Hide cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnToMenu
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called in pause, when you die, or complete the game to return to the main    
    *              menu. It simply just reloads the scene.
    **************************************************************************************/
    public void ReturnToMenu()
    {
        //Make player not able to move and invulnerable to prevent possible death, which creates it's own coroutines which may cause issues
        m_playerController.LoseControl();
        m_playerHealthManager.SetInvulnerable( true );
        //Make sure timescale is returned to 1 before the coroutine
        Time.timeScale = 1f;
        StartCoroutine(ReloadScene());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReloadScene
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade out game, wait an additional 0.1 seconds so as to make sure screen
    *              is fully black before starting load so as to avoid weird effects
    **************************************************************************************/
    private IEnumerator ReloadScene()
	{
        float respawnFadeTime = 2.0f;
        m_uiManager.ReturnToMenu( respawnFadeTime );
        yield return new WaitForSeconds( respawnFadeTime + 0.1f );

        m_sceneLoad = SceneManager.LoadSceneAsync( SceneManager.GetActiveScene().name );
        while ( !m_sceneLoad.isDone )
        {
            yield return null;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Die
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: When Player dies, fade in UI to allow respawn
    **************************************************************************************/
    public void Die()
	{
        Settings.g_canPause = false;
        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;
        m_uiManager.DisplayDeathUI();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RespawnFromMenu
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Function on the Pause and Death menu to respawn. (Step 1 of many for respawning)
    **************************************************************************************/
    public void RespawnFromMenu()
	{
        Time.timeScale = 1;

        /* This is done to prevent the player doing a melee 
         * attack before pausing, clicking respawn and then 
         * when unpaused, an animation event regains their control */
        m_playerController.SetMenuLock( true );
        m_playerController.LoseControl();
        //Invulnerable to prevent death to prevent weirdness
        m_playerHealthManager.SetInvulnerable( true );
        //BEGIN respawning
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

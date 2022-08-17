/**************************************************************************************
* Type: Static Class
* 
* Name: Settings
*
* Author: Charlie Taylor
*
* Description: Global settings for information that either is needed in nearly everywhere
*			   or it just was cleaner/easier to make it a global
**************************************************************************************/
public static class Settings
{
	//Game is Paused
	public static bool g_paused = false;
	//Game CAN be paused
	public static bool g_canPause = false;

	//Has the player picked up the sword
	public static bool g_pickedUpSword = false;

	//Strings for tags for objects called A LOT
	public static string g_controllerTag = "GameController";
	public static string g_playerTag = "Player";

	//Camera Sensitivity
	public static float g_minCameraSensitiviy = 100f;
	public static float g_maxCameraSensitiviy = 300f;
	public static float g_currentXCameraSensitiviy = 200f;
	public static float g_currentYCameraSensitiviy = 2f;

}

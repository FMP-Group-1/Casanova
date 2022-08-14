using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{

	public static bool g_paused = false;
	public static bool g_canPause = false;

	public static string g_ControllerTag = "GameController";
	public static string g_playerTag = "Player";

	public static float g_audioVolume = 1f;
	public static float g_minCameraSensitiviy = 100f;
	public static float g_maxCameraSensitiviy = 300f;

	public static float g_currentXCameraSensitiviy = 200f;
	public static float g_currentYCameraSensitiviy = 2f;

}

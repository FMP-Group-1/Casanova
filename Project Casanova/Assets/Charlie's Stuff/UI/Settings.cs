using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{

	public static bool g_paused = false;
	public static string g_ControllerTag = "GameController";

	public static float g_audioVolume = 1f;
	public static float g_minXCameraSensitiviy = 100f;
	public static float g_maxXCameraSensitiviy = 300f;
	public static float g_currentXCameraSensitiviy = 200f;

	public static float g_minYCameraSensitiviy = 1f;
	public static float g_maxYCameraSensitiviy = 3f;
	public static float g_currentYCameraSensitiviy = 2f;

}

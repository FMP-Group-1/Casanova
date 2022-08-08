using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraSensitivityManager : MonoBehaviour
{
	public void SetSensitivity()
    {
        Vector2 newSensitivity = GameObject.FindGameObjectWithTag( Settings.g_ControllerTag ).GetComponent<OptionsManager>().GetSensitivity();
        Settings.g_currentXCameraSensitiviy = newSensitivity.x;
        Settings.g_currentYCameraSensitiviy = newSensitivity.y;
        GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed = Settings.g_currentYCameraSensitiviy;
        GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = Settings.g_currentXCameraSensitiviy;
    }
}

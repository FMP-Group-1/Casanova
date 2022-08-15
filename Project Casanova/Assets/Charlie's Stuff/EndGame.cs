using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{

	private void Start()
	{
		
	}
	UIManager m_uiUIManager;
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag == "Player" )
		{
			other.gameObject.GetComponent<PlayerController>().LoseControl();
			CompleteGame();
		}
	}

	private void CompleteGame()
	{
		Settings.g_canPause = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<UIManager>().CompleteGame();
	}
}

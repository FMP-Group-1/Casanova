using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
	UIManager m_uiUIManager;
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag == "Player" )
		{
			CompleteGame();
		}
	}

	private void CompleteGame()
	{
		Settings.g_canPause = false;
	}
}

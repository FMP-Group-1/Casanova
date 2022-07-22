using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	protected GameObject m_player;

	private void Start()
	{
	}
	public virtual void Interact()
	{
		m_player = GameObject.FindGameObjectWithTag( "Player" );
		m_player.GetComponent<PlayerController>().LoseControl();
		m_player.GetComponent<Animator>().SetTrigger( "Interact" );
	}
}

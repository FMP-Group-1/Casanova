using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	protected GameObject m_player;

	private void Start()
	{
		m_player = GameObject.FindGameObjectWithTag( "Player" );
	}
	public virtual void Interact()
	{
		m_player.GetComponent<Animator>().SetTrigger( "Interact" );
	}
}

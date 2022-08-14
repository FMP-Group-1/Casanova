using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordInteract : Interactable
{
	[SerializeField]
	GameObject m_light;
	[SerializeField]
	private int m_enemyGroupToSpawn;
	[SerializeField]
	Room m_roomToActivate;
	public override void Interact()
	{
		base.Interact();


		EventManager.StartSpawnEnemiesEvent( m_enemyGroupToSpawn );
		EventManager.StartAlertEnemiesEvent( m_enemyGroupToSpawn );
		EventManager.StartSpawnEnemiesEvent( m_enemyGroupToSpawn + 1 );

		GameObject.FindGameObjectWithTag( Settings.g_ControllerTag ).GetComponent<GameManager>().EnterRoom( m_roomToActivate );

		m_light.SetActive(false);
		m_player.GetComponent<WeaponSwap>().DropTableLeg(gameObject);
	}
}

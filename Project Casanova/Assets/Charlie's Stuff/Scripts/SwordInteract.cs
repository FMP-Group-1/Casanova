using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordInteract : Interactable
{
	[SerializeField]
	GameObject m_light;
	[SerializeField]
	private int m_enemyGroupToAlert;
	[SerializeField]
	Room m_roomToActivate;
	public override void Interact()
	{
		base.Interact();


		EventManager.StartAlertEnemiesEvent( m_enemyGroupToAlert );
		EventManager.StartSpawnEnemiesEvent( m_enemyGroupToAlert + 1 );

		GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<GameManager>().EnterRoom( m_roomToActivate );

		m_light.SetActive(false);
		GetPlayer().GetComponent<WeaponSwap>().DropTableLeg(gameObject);
	}
}

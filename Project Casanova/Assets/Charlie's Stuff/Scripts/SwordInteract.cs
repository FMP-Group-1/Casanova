using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordInteract : Interactable
{
	public override void Interact()
	{
		base.Interact();
		m_player.GetComponent<WeaponSwap>().DropTableLeg(gameObject);
	}
}

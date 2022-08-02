using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateCloseTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject m_gateToClose;

	private void OnTriggerEnter( Collider other )
	{
		if ( other.tag == "Player" )
		{
			m_gateToClose.GetComponent<GateMover>().CloseGate();
			gameObject.SetActive( false );
		}
	}
}

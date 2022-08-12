using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    [SerializeField]
    private GameObject m_tableLeg;
    private GameObject m_worldSword;
    [SerializeField]
    private GameObject m_swordInHand;

	public void DropTableLeg( GameObject worldSword )
    {
        m_worldSword = worldSword;
        GetComponent<PlayerController>().LoseControl();


        //Deactivate Trigger
        m_tableLeg.GetComponent<Collider>().enabled = false;
        //Activate Collider on the MESH to work as the physics collider
        m_tableLeg.GetComponentInChildren<Collider>().enabled = true;
        m_tableLeg.GetComponentInChildren<Rigidbody>().isKinematic = false;
        m_tableLeg.GetComponent<DespawnTableLeg>().Despawn();
        m_tableLeg.transform.parent = null;

        StartCoroutine(EquipSword());
	}

    private IEnumerator EquipSword()
	{
        yield return new WaitForSeconds( 1f );
        m_swordInHand.SetActive( true );
        m_worldSword.SetActive( false );
        //GetComponent<PlayerController>().RegainControl();

        GetComponent<MeleeController>().SwapWeapon( m_swordInHand, m_swordInHand.GetComponent<SwordCollisionManager>() ) ;
        
    }

}

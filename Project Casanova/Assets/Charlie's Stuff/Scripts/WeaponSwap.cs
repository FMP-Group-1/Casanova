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

    Rigidbody m_tableLegRB;

	public void DropTableLeg( GameObject worldSword )
    {
        m_worldSword = worldSword;
        GetComponent<PlayerController>().LoseControl();


        //Deactivate Trigger box
        m_tableLeg.GetComponents<Collider>()[ 0 ].enabled = false;
        //Activate Collider on the MESH to work as the physics collider
        m_tableLeg.GetComponents<Collider>()[ 1 ].enabled = true;

        m_tableLegRB = m_tableLeg.GetComponent<Rigidbody>();

        m_tableLegRB.isKinematic = false;
        m_tableLegRB.mass = 1;
        m_tableLegRB.useGravity = true;
        m_tableLegRB.constraints = RigidbodyConstraints.None;

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

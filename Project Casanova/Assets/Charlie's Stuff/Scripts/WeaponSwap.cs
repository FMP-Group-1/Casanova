using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    [SerializeField]
    private GameObject m_originalWeapon;
    private GameObject m_worldSword;
    [SerializeField]
    private GameObject m_swordInHand;

	public void DropWeapon( GameObject worldSword )
    {
        m_worldSword = worldSword;
        GetComponent<PlayerController>().LoseControl();
        m_originalWeapon.GetComponent<Collider>().enabled = true;
        m_originalWeapon.GetComponent<Rigidbody>().isKinematic = false;
        m_originalWeapon.transform.parent = null;

        StartCoroutine(EquipSword());
        //m_swordInHand.SetActive(true);
	}

    private IEnumerator EquipSword()
	{
        yield return new WaitForSeconds( 1f );
        m_swordInHand.SetActive( true );
        m_worldSword.SetActive( false );
        //GetComponent<PlayerController>().RegainControl();

    }
}

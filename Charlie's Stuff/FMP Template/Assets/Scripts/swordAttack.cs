using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class swordAttack : MonoBehaviour
{
    private bool collidersActive = false;
    public Text collidingText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
	{
        if (collision.gameObject.tag == "Enemy")
        {
            collidingText.text = "Touching";
            //if (collidersActive)
           // {
                collision.gameObject.GetComponent<enemyHit>().GetHit();
            //}
        }

	}
	private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collidingText.text = "";
        }

    }
	private void OnCollisionStay(Collision collision)
	{
		
	}

	public void setCollidersAcitve(bool active)
	{
        collidersActive = active;
	}
}

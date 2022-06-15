using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug_FakeEnemyCollider : MonoBehaviour
{
    [SerializeField]
    private float m_rotationSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newRotation = transform.rotation.eulerAngles;
        newRotation.y += m_rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newRotation); 
    }

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag == "Player" )
		{
            PlayerHealth playerHealthScript;
            playerHealthScript = other.gameObject.GetComponent<PlayerHealth>();
            playerHealthScript.GetHurt( transform );
		}
	}
}

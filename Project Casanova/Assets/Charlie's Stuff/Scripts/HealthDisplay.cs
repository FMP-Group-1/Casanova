using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    TMP_Text m_textObj;
    Camera m_mainCamera;
    [SerializeField]
    CharacterDamageManager m_characterDamageManager;
    // Start is called before the first frame update
    [SerializeField]
    private bool m_playerHealth = false;
    void Start()
    {
        m_mainCamera = Camera.main;
        m_textObj = GetComponent<TMP_Text>();
        if ( !m_playerHealth )
		{
            m_characterDamageManager = transform.parent.gameObject.GetComponent<CharacterDamageManager>();
        }
        

        UpdateHealth( m_characterDamageManager.GetHealth());


    }

    // Update is called once per frame
    void Update()
    {
        if( !m_playerHealth )
		{

            Vector3 target = m_mainCamera.transform.position ;
            target.y = transform.position.y;
            transform.LookAt( target );

        }



        /*

        Quaternion newRotation =  m_mainCamera.transform.rotation;
        newRotation.y = 0;
        transform.rotation = newRotation;*/
    }

    public void UpdateHealth( float newValue )
	{
        m_textObj.SetText("Health: " + newValue.ToString() );

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class enemyHealthDisplay : MonoBehaviour
{
    TextMeshPro m_textObj;
    Camera m_mainCamera;
    [SerializeField]
    CharacterDamageManager m_characterDamageManager;
    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera = Camera.main;
        m_textObj = GetComponent<TextMeshPro>();

        UpdateHealth( m_characterDamageManager.GetHealth());


    }

    // Update is called once per frame
    void Update()
    {
        Vector3 target = m_mainCamera.transform.position ;
        target.y = transform.position.y;
        transform.LookAt( target );




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

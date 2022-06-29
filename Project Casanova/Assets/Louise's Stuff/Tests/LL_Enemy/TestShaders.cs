using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class TestShaders : MonoBehaviour
{
    [SerializeField]
    [Tooltip( "Action Input" )]
    private InputActionReference m_action;

    private Material m_material;


	private void OnEnable()
	{
        m_action.action.Enable();
	}
	// Start is called before the first frame update
	void Start()
    {
        m_material = GetComponent<Renderer>().sharedMaterial;
        m_material.SetFloat( "_FadeStartTime", float.MaxValue );
    }

    // Update is called once per frame
    void Update()
    {
		if( m_action.action.triggered )
		{
            m_material.SetFloat( "_FadeStartTime", Time.time );
            Debug.Log( "Ya boi" );
		}
    }
}

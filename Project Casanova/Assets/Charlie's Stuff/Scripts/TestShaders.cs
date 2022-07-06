using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class TestShaders : MonoBehaviour
{
    [SerializeField]
    [Tooltip( "Action Input" )]
    private InputActionReference m_action;

    private List<Material> m_materialList = new List<Material>();



    private void OnEnable()
	{
        m_action.action.Enable();
	}
	// Start is called before the first frame update
	void Start()
    {
    
        int iteration = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach ( Renderer child in renderers )
        {
            m_materialList.Add( renderers[ iteration ].material );
            m_materialList[ iteration ] = renderers[ iteration ].material;
            m_materialList[ iteration ].SetFloat( "_FadeStartTime", float.MaxValue );
            m_materialList[ iteration ].SetInt( "_ForceVisible", 0 );
            iteration++;
        }
    }


    // Update is called once per frame
    void Update()
    {
		if( m_action.action.triggered )
		{
           foreach( Material mat in m_materialList )
			{
                mat.SetFloat( "_FadeStartTime", Time.time );
            }
		}
    }
}

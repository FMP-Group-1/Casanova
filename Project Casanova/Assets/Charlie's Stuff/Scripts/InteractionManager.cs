using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionManager : MonoBehaviour
{


    [SerializeField]
    [Tooltip( "Interact Input" )]
    private InputActionReference m_interact;

    [SerializeField]
    private GameObject m_thingToAffect;

    [SerializeField]
    TextMeshPro m_text;


    private bool m_isInteractive = false;

    private void OnEnable()
    {
        m_interact.action.Enable();
    }
    private void OnDisable()
    {
    }

    void Start()
    {
        m_text.enabled = false;
    }

	private void Update()
	{
       // Debug.Log( "GameObject: " + gameObject.name + " || Bool: " + m_isInteractive );

        if( m_isInteractive )
		{
            if( m_interact.action.triggered )
		    {
                ActivateInteractable();
                Destroy(transform.parent.gameObject);
            }
        }
    }

	private void OnTriggerEnter( Collider other )
    {
        if( other.tag == "Player" )
        {
            m_text.enabled = true;
            m_isInteractive = true;
        }
    }
    private void OnTriggerExit( Collider other )
    {
        if( other.tag == "Player" )
        {
            m_isInteractive = false;
            m_text.enabled = false;
        }
    }

    public void ActivateInteractable()
	{
        m_thingToAffect.GetComponent<Interactable>().Interact();
    }
}

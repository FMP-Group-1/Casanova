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


    bool m_isInteractive = false;

    private void OnEnable()
    {
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
		if( m_interact.action.triggered )
		{
            ActivateInteractable();
            Destroy( m_text );
            Destroy(gameObject);
		}
	}

	private void OnTriggerEnter( Collider other )
    {
        if( other.tag == "Player" )
        {
            m_interact.action.Enable();
            m_text.enabled = true;
        }
    }
    private void OnTriggerExit( Collider other )
    {
        if( other.tag == "Player" )
        {
            m_text.enabled = false;
            m_interact.action.Disable();
        }
    }

    public void ActivateInteractable()
	{
        m_thingToAffect.GetComponent<Interactable>().Interact();
    }
}

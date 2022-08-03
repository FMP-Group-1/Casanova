using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateMover : MonoBehaviour
{
    [SerializeField]
    float m_gateOpenTime = 3f;
    [SerializeField]
    float m_gateCloseTime = 1f;

    [SerializeField]
    GameObject m_gateTrigger;

    [SerializeField]
    private float m_openYTarget = 5.0f;

    private float m_closedY = 0.0f;

    GameObject m_visualGate;

    // Start is called before the first frame update
    void Start()
    {
        m_visualGate = transform.GetChild( 0 ).gameObject;
        //OpenGate();
    }

    public void ResetGate()
	{
        OpenGate();
        m_gateTrigger.SetActive(true);

    }

    public void OpenGate()
	{

        //Activate moving One, so you can get through, JUST as it's open enough
        m_visualGate.GetComponent<BoxCollider>().enabled = true;
        //Disable permenant collider, don't need both
        GetComponent<Collider>().enabled = false;

        StartCoroutine( MoveGate( m_openYTarget, m_gateOpenTime ) );
    }

    public void CloseGate()
    {
        //Activate permenant One, stops sneaking through
        GetComponent<BoxCollider>().enabled = true;
        //Disable moving collider, don't need both
        m_visualGate.GetComponent<Collider>().enabled = false;


        StartCoroutine( MoveGate( m_closedY, m_gateCloseTime) );
    }

    IEnumerator MoveGate( float yTarget, float overTime )
	{

        Vector3 currentPosition = m_visualGate.transform.position;
        
        Vector3 targetPosition = transform.position;
        targetPosition.y += yTarget;


        float timeElapsed = 0.0f;

        while ( timeElapsed < overTime )
		{

            currentPosition.y = Mathf.Lerp( m_visualGate.transform.position.y, targetPosition.y, timeElapsed / overTime );
            m_visualGate.transform.position = currentPosition;

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        m_visualGate.transform.position = targetPosition;

    }
}

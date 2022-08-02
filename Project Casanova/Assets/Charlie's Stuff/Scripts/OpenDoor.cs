using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : Interactable
{
    [SerializeField]
    private float m_targetAngle;
    
    [SerializeField, Range(0.0f, 2.0f)]
    private float m_rotationTime;

    private ObjectSoundHandler m_soundHandler;

    // Start is called before the first frame update
    void Start()
    {
        //m_targetAngle -= transform.eulerAngles.y;
        m_soundHandler = GetComponent<ObjectSoundHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
	{
        base.Interact();
        //Play door noise?
        //m_soundHandler.PlayGateOpenSFX();
        //Get the angle where your inputs are, relative to camera
        //Pass that into a quaternion
        Quaternion targetRotation = Quaternion.Euler( 0f, m_targetAngle, 0f );

        //gameObject.GetComponent<Collider>().enabled = false;
        //Rotate to it using rotation speed
        StartCoroutine(OpenCellDoor(targetRotation));


    }

    private IEnumerator OpenCellDoor(Quaternion targetRotation)
	{
        yield return new WaitForSeconds( 1.0f );

        
        float timer = 0f;
        while (timer < m_rotationTime )
		{
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * m_rotationTime );
            yield return null;
        }
        //transform.rotation = targetRotation;
    }
}

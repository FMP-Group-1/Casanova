using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : Interactable
{
    [SerializeField]
    private float m_targetAngle;
    [SerializeField]
    private Transform m_pivotPoint;
    [SerializeField]
    private float m_rotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        m_targetAngle -= transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
	{
        base.Interact();
        //Play door noise?
        //Get the angle where your inputs are, relative to camera
        //Pass that into a quaternion
        Quaternion targetRotation = Quaternion.Euler( 0f, m_targetAngle, 0f );

        gameObject.GetComponent<Collider>().enabled = false;
        //Rotate to it using rotation speed
        transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * m_rotationSpeed );
        transform.rotation = targetRotation;
    }
}

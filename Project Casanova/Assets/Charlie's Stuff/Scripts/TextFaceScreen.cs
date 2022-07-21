using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFaceScreen : MonoBehaviour
{
    Camera m_mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        m_mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 target = m_mainCamera.transform.position ;
        //target.y = transform.position.y;
        transform.LookAt( target );

    }
}

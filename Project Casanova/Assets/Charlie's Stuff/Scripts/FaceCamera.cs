using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FaceCamera : MonoBehaviour
{
    Camera m_mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = m_mainCamera.transform.rotation;

    }
}

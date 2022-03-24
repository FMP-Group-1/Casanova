using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject m_camera;

    private float m_moveSpeed = 5.0f;
    private float m_rotateSpeed = 300.0f;
    private Renderer m_playerRenderer;
    private Color m_defaultColor;
    private Color m_redTransparent;

    void Awake()
    {
        m_playerRenderer = GetComponent<Renderer>();
        m_defaultColor = m_playerRenderer.material.color;
        m_redTransparent = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * ( m_moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * (m_moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * (m_moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * (m_moveSpeed * Time.deltaTime);
        }

        transform.Rotate(0.0f, (Input.GetAxis("Mouse X") * (m_rotateSpeed * Time.deltaTime)), 0.0f);
    }

    public void SetHitVisual( bool isHit )
    {
        if ( isHit )
        {
            m_playerRenderer.material.SetColor("_Color", m_redTransparent);
        }
        else if ( !isHit )
        {
            m_playerRenderer.material.SetColor("_Color", m_defaultColor);
        }
    }
}

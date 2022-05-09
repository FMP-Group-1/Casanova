using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePassiveVisualizer : MonoBehaviour
{
    [SerializeField]
    private GameObject m_parentAI;
    private EnemyAI m_parentScript;
    [SerializeField]
    private float m_height = 2.0f;
    private Renderer m_renderer;
    private bool m_parentScriptExists = false;
    [SerializeField]
    private Color m_passiveColor = Color.blue;
    [SerializeField]
    private Color m_activeColor = Color.green;    

    void Start()
    {
        m_renderer = GetComponent<Renderer>();

        // Check to see if the parent object exists, if not, sends a message to the console, and disables the object
        if (m_parentAI != null)
        {
            m_parentScript = m_parentAI.GetComponent<EnemyAI>();
            m_parentScriptExists = true;
        }
        else
        {
            Debug.Log("No Parent AI found. Please attach a parent for this visualizer to work.");
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only run this code if the parent script exists, which is checked in Start()
        if (m_parentScriptExists)
        {
            MatchParentPos();
            ColorCheck();
        }
    }

    // Matching the visualizer position to be slightly above the AI
    private void MatchParentPos()
    {
        Vector3 tempPos = m_parentAI.transform.position;
        tempPos.y += m_height;
        transform.position = tempPos;
    }

    private void ColorCheck()
    {
        // Checking if the attacker type and the color match, and if not, then setting the color
        if (m_parentScript.GetAttackingType() == AttackingType.Passive && m_renderer.material.color != m_passiveColor)
        {
            m_renderer.material.color = m_passiveColor;
        }
        else if (m_parentScript.GetAttackingType() == AttackingType.Active && m_renderer.material.color != m_activeColor)
        {
            m_renderer.material.color = m_activeColor;
        }
    }
}

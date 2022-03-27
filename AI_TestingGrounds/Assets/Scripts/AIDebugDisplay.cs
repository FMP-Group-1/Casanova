using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIDebugDisplay : MonoBehaviour
{
    [SerializeField]
    private EnemyAI m_aiDebugTarget;

    private Text m_aiNameText;
    private Text m_aiStateText;
    private Text m_playerDetectedText;
    void Awake()
    {
        SetupDebugDisplay();
    }

    void Update()
    {
        DebugTextUpdate();
    }

    private void SetupDebugDisplay()
    {
        m_aiNameText = GameObject.Find("AINameText").GetComponent<Text>();
        m_aiStateText = GameObject.Find("AIStateText").GetComponent<Text>();
        m_playerDetectedText = GameObject.Find("PlayerDetectText").GetComponent<Text>();

        m_aiNameText.text = "AI: " + m_aiDebugTarget.name;
    }

    private void DebugTextUpdate()
    {
        m_aiStateText.text = "AI State: " + m_aiDebugTarget.GetState();
        m_playerDetectedText.text = "Player Detected: " + m_aiDebugTarget.IsPlayerVisible();
    }
}

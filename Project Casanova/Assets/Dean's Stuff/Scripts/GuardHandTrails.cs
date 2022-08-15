using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardHandTrails : MonoBehaviour
{
    EnemyAI m_parentAI;
    [SerializeField]
    private GameObject m_lHand;
    [SerializeField]
    private GameObject m_rHand;
    private List<ParticleSystem> m_handParticleList = new List<ParticleSystem>();
    private CombatState m_prevAIState;

    void Start()
    {
        m_parentAI = GetComponent<EnemyAI>();

        foreach (Transform child in m_lHand.transform)
        {
            m_handParticleList.Add(child.gameObject.GetComponent<ParticleSystem>());
        }
        foreach (Transform child in m_rHand.transform)
        {
            m_handParticleList.Add(child.gameObject.GetComponent<ParticleSystem>());
        }

        m_prevAIState = m_parentAI.GetCombatState();

        TrailsEnabled(false);
    }

    void Update()
    {
        UpdateTrails();
    }


    private void UpdateTrails()
    {
        CombatState currentState = m_parentAI.GetCombatState();

        if (m_prevAIState != currentState)
        {
            if (currentState == CombatState.Attacking)
            {
                TrailsEnabled(true);
            }
            else if (m_prevAIState == CombatState.Attacking)
            {
                TrailsEnabled(false);
            }
        }

        m_prevAIState = currentState;
    }

    private void TrailsEnabled( bool shouldEnable)
    {
        foreach (ParticleSystem handTrail in m_handParticleList)
        {
            if (shouldEnable)
            {
                handTrail.Play();
            }
            else
            {
                handTrail.Clear();
                handTrail.Pause();
            }
        }
    }
}

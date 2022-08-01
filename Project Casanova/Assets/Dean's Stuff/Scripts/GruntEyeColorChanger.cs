using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEyeColorChanger : MonoBehaviour
{
    private EnemyAI m_parentAI;

    [SerializeField]
    private GameObject m_parentObject;
    private Color m_neutralColorMin;
    private Color m_neutralColorMax;
    [SerializeField]
    private Color m_lightAttackColorMin;    
    [SerializeField]
    private Color m_lightAttackColorMax;
    [SerializeField]
    private Color m_heavyAttackColorMin;
    [SerializeField]
    private Color m_heavyAttackColorMax;
    private ParticleSystem m_particleSystem;
    private ParticleSystem.MainModule m_particleSettings;
    private CombatState m_prevAIState;

    void Start()
    {
        m_parentAI = m_parentObject.GetComponent<EnemyAI>();
        m_particleSystem = GetComponent<ParticleSystem>();
        m_particleSettings = m_particleSystem.main;

        ChangeEyeColor(m_lightAttackColorMin, m_lightAttackColorMax);

        m_neutralColorMin = m_particleSettings.startColor.colorMin;
        m_neutralColorMax = m_particleSettings.startColor.colorMax;

        m_prevAIState = m_parentAI.GetCombatState();
    }

    void Update()
    {
        TrackAIState();
    }

    private void ChangeEyeColor(Color min, Color max)
    {
        //ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(min, max);
        ParticleSystem.ColorOverLifetimeModule col = m_particleSystem.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(min, 0.0f), new GradientColorKey(max, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        m_particleSettings.startColor = grad;
        col.color = grad;
    }

    private void TrackAIState()
    {
        CombatState currentState = m_parentAI.GetCombatState();

        if (m_prevAIState != currentState)
        {
            if (currentState == CombatState.Attacking)
            {
                switch (m_parentAI.GetAttackMode())
                {
                    case AttackMode.Primary:
                    {
                        ChangeEyeColor(m_neutralColorMin, m_neutralColorMax);
                        break;
                    }
                    case AttackMode.Secondary:
                    {
                        ChangeEyeColor(m_heavyAttackColorMin, m_heavyAttackColorMax);
                        break;
                    }
                    case AttackMode.Both:
                    {
                        ChangeEyeColor(m_lightAttackColorMin, m_lightAttackColorMax);
                        break;
                    }
                }
            }
            else if (m_prevAIState == CombatState.Attacking)
            {
                ChangeEyeColor(m_neutralColorMin, m_neutralColorMax);
            }
        }

        m_prevAIState = currentState;
    }
}

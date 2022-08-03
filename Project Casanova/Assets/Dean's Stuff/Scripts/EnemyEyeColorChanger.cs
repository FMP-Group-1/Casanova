using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEyeColorChanger : MonoBehaviour
{
    private EnemyAI m_parentAI;

    [SerializeField]
    private GameObject m_eye1;
    [SerializeField]
    private GameObject m_eye2;
    [SerializeField]
    private GameObject m_trail1;
    [SerializeField]
    private GameObject m_trail2;
    [SerializeField]
    private Color m_neutralColorMin;
    [SerializeField]
    private Color m_neutralColorMax;
    [SerializeField]
    private Color m_lightAttackColorMin;    
    [SerializeField]
    private Color m_lightAttackColorMax;
    [SerializeField]
    private Color m_heavyAttackColorMin;
    [SerializeField]
    private Color m_heavyAttackColorMax;
    private ParticleSystem m_particleEye1;
    private ParticleSystem m_particleEye2;
    private ParticleSystem m_particleTrail1;
    private ParticleSystem m_particleTrail2;
    private CombatState m_prevAIState;

    void Start()
    {
        m_parentAI = GetComponent<EnemyAI>();
        m_particleEye1 = m_eye1.GetComponent<ParticleSystem>();
        m_particleEye2 = m_eye2.GetComponent<ParticleSystem>();
        m_particleTrail1 = m_trail1.GetComponent<ParticleSystem>();
        m_particleTrail2 = m_trail2.GetComponent<ParticleSystem>();

        m_prevAIState = m_parentAI.GetCombatState();
    }

    void Update()
    {
        TrackAIState();
    }

    private void ChangeEyeColor(Color min, Color max)
    {
        //ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(min, max);
        ParticleSystem.ColorOverLifetimeModule eyeCol1 = m_particleEye1.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule eyeCol2 = m_particleEye2.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule trailCol1 = m_particleTrail1.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule trailCol2 = m_particleTrail2.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(min, 0.0f), new GradientColorKey(max, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        eyeCol1.color = grad;
        eyeCol2.color = grad;
        trailCol1.color = grad;
        trailCol2.color = grad;
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

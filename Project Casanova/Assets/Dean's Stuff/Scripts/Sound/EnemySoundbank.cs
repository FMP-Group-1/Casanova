using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundbank : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_damageSFX;
    [SerializeField]
    private AudioClip m_normalAttackSFX;
    [SerializeField]
    private AudioClip m_normalCollisionSFX;
    [SerializeField]
    private AudioClip m_heavyAttackSFX;
    [SerializeField]
    private AudioClip m_heavyCollisionSFX;
    [SerializeField]
    private AudioClip m_quickAttackSFX;
    [SerializeField]
    private AudioClip m_quickCollisionSFX;
    [SerializeField]
    private AudioClip m_deathSFX;

    public ref AudioClip GetDamageSFX()
    {
        return ref m_damageSFX;
    }

    public ref AudioClip GetNormalAttackSFX()
    {
        return ref m_normalAttackSFX;
    }

    public ref AudioClip GetNormalCollisionSFX()
    {
        return ref m_normalCollisionSFX;
    }

    public ref AudioClip GetHeavyAttackSFX()
    {
        return ref m_heavyAttackSFX;
    }

    public ref AudioClip GetHeavyCollisionSFX()
    {
        return ref m_heavyCollisionSFX;
    }

    public ref AudioClip GetQuickAttackSFX()
    {
        return ref m_quickAttackSFX;
    }

    public ref AudioClip GetQuickCollisionSFX()
    {
        return ref m_quickCollisionSFX;
    }

    public ref AudioClip GetDeathSFX()
    {
        return ref m_deathSFX;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundbank : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_footstepSFX;
    [SerializeField]
    private AudioClip m_dodgeSFX;
    [SerializeField]
    private AudioClip m_damageSFX;
    [SerializeField]
    private AudioClip m_attackGruntSFX;
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
    [SerializeField]
    private AudioClip m_wakeSFX;
    [SerializeField]
    private AudioClip m_tauntSFX;

    public ref AudioClip GetFootstepSFX()
    {
        return ref m_footstepSFX;
    }

    public ref AudioClip GetDodgeSFX()
    {
        return ref m_dodgeSFX;
    }

    public ref AudioClip GetDamageSFX()
    {
        return ref m_damageSFX;
    }

    public ref AudioClip GetAttackGruntSFX()
    {
        return ref m_attackGruntSFX;
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

    public ref AudioClip GetWakeSFX()
    {
        return ref m_wakeSFX;
    }

    public ref AudioClip GetTauntSFX()
    {
        return ref m_tauntSFX;
    }
}

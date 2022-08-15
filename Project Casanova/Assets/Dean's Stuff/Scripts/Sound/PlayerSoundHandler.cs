using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundHandler : CharacterSoundHandler
{
    [SerializeField]
    private AudioClip[] m_damageSFX;
    [SerializeField]
    private AudioClip[] m_attackGruntSFX;
    [SerializeField]
    private AudioClip m_normalAttackSFX;
    [SerializeField]
    private AudioClip[] m_normalCollisionSFX;
    [SerializeField]
    private AudioClip m_heavyAttackSFX;
    [SerializeField]
    private AudioClip[] m_heavyCollisionSFX;
    [SerializeField]
    private AudioClip[] m_footstepSFX;
    [SerializeField]
    private AudioClip m_dodgeSFX;
    [SerializeField]
    private AudioClip[] m_deathSFX;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void PlayDamageSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_damageSFX));
    }

    public override void PlayAttackGruntSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_attackGruntSFX));
    }

    public override void PlayNormalAttackSFX()
    {
        m_audioSource.PlayOneShot(m_normalAttackSFX);
    }

    public override void PlayNormalCollisionSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_normalCollisionSFX));
    }

    public override void PlayHeavyAttackSFX()
    {
        m_audioSource.PlayOneShot(m_heavyAttackSFX);
    }

    public override void PlayHeavyCollisionSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_heavyCollisionSFX));
    }

    public override void PlayDeathSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_deathSFX));
    }

    public override void PlayFootstepSFX()
    {
        m_audioSource.PlayOneShot(RandomiseSFX(ref m_footstepSFX));
    }

    public override void PlayDodgeSFX()
    {
        m_audioSource.PlayOneShot(m_dodgeSFX);
    }

    private ref AudioClip RandomiseSFX( ref AudioClip[] clipArray )
    {
        int indexToGet = 0;

        if (clipArray.Length > 1)
        {
            indexToGet = Random.Range(0, clipArray.Length);
        }

        return ref clipArray[indexToGet];
    }
}

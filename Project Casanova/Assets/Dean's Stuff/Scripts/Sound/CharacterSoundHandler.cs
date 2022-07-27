using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundHandler : SoundHandler
{
    [SerializeField]
    private AudioClip m_damageSFX;
    [SerializeField]
    private AudioClip m_normalAttackSFX;
    [SerializeField]
    private AudioClip m_normalCollisionSFX;
    [SerializeField]
    private AudioClip m_quickAttackSFX;
    [SerializeField]
    private AudioClip m_quickCollisionSFX;
    [SerializeField]
    private AudioClip m_heavyAttackSFX;
    [SerializeField]
    private AudioClip m_heavyCollisionSFX;


    public void PlayDamageSFX()
    {
        m_audioSource.PlayOneShot(m_damageSFX);
    }

    public void PlayNormalAttackSFX()
    {
        m_audioSource.PlayOneShot(m_normalAttackSFX);
    }

    public void PlayNormalCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_normalCollisionSFX);
    }

    public void PlayQuickAttackSFX()
    {
        m_audioSource.PlayOneShot(m_quickAttackSFX);
    }

    public void PlayQuickCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_quickCollisionSFX);
    }

    public void PlayHeavyAttackSFX()
    {
        m_audioSource.PlayOneShot(m_heavyAttackSFX);
    }

    public void PlayHeavyCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_heavyCollisionSFX);
    }
}

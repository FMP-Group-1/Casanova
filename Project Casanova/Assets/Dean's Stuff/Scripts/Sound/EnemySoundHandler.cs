using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundHandler : CharacterSoundHandler
{
    private EnemySoundbank m_enemySoundbank;

    protected override void Awake()
    {
        base.Awake();
        m_enemySoundbank = GameObject.FindGameObjectWithTag("EnemySoundbank").GetComponent<EnemySoundbank>();
    }
    public override void PlayDamageSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetDamageSFX());
    }

    public override void PlayNormalAttackSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetNormalAttackSFX());
    }

    public override void PlayNormalCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetNormalCollisionSFX());
    }

    public override void PlayHeavyAttackSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetHeavyAttackSFX());
    }

    public override void PlayHeavyCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetHeavyCollisionSFX());
    }

    public override void PlayDeathSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetDeathSFX());
    }

    public void PlayQuickAttackSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetQuickAttackSFX());
    }

    public void PlayQuickCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_enemySoundbank.GetQuickCollisionSFX());
    }
}

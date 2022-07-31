using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundHandler : CharacterSoundHandler
{
    private EnemySoundbank m_soundbank;

    protected override void Awake()
    {
        base.Awake();

        if (gameObject.GetComponent<EnemyAI>().GetEnemyType() == EnemyType.Grunt)
        {
            m_soundbank = GameObject.FindGameObjectWithTag("GruntSoundbank").GetComponent<EnemySoundbank>();
        }
        else
        {
            m_soundbank = GameObject.FindGameObjectWithTag("GuardSoundbank").GetComponent<EnemySoundbank>();
        }
    }
    public override void PlayDamageSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDamageSFX());
    }

    public override void PlayNormalAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetNormalAttackSFX());
    }

    public override void PlayNormalCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetNormalCollisionSFX());
    }

    public override void PlayHeavyAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetHeavyAttackSFX());
    }

    public override void PlayHeavyCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetHeavyCollisionSFX());
    }

    public override void PlayDeathSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDeathSFX());
    }

    public void PlayQuickAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetQuickAttackSFX());
    }

    public void PlayQuickCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetQuickCollisionSFX());
    }
}

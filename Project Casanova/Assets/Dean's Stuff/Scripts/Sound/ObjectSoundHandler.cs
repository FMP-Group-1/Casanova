using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSoundHandler : SoundHandler
{
    private EnvironmentSoundbank m_enviroSoundbank;

    protected override void Awake()
    {
        base.Awake();
        m_enviroSoundbank = GameObject.FindGameObjectWithTag("EnvironmentSoundbank").GetComponent<EnvironmentSoundbank>();
    }

    public void PlayGateOpenSFX()
    {
        m_audioSource.PlayOneShot(m_enviroSoundbank.GetGateOpenSFX());
    }
}

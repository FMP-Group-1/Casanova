using UnityEngine;

public class ObjectSoundHandler : SoundHandler
{
    private EnvironmentSoundbank m_enviroSoundbank;

    protected override void Awake()
    {
        base.Awake();
        m_enviroSoundbank = GameObject.FindGameObjectWithTag("EnvironmentSoundbank").GetComponent<EnvironmentSoundbank>();
    }

    public void PlayCellDoorOpenSFX()
    {
        m_audioSource.PlayOneShot(m_enviroSoundbank.GetCellDoorOpenSFX());
    }

    public void PlayGateOpenSFX()
    {
        m_audioSource.PlayOneShot(m_enviroSoundbank.GetGateOpenSFX());
    }

    public void PlayGateCloseSFX()
    {
        m_audioSource.PlayOneShot(m_enviroSoundbank.GetGateCloseSFX());
    }

    public void PlayWaterDripSFX()
    {
        m_audioSource.PlayOneShot(m_enviroSoundbank.GetWaterDripSFX());
    }
}
